using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using DittoToClipboardFusion.Models;
using Newtonsoft.Json;
using PetaPoco;

namespace DittoToClipboardFusion.Services
{
    internal sealed class DittoToClipboardFusion
    {
        private const long PageSize = 100;
        private static DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        private readonly JsonSerializer serializer;

        private static long TimeFromUnixTimestamp(int unixTimestamp)
        {
            try
            {
                var nDate = dtDateTime.AddSeconds(unixTimestamp).ToUniversalTime();
                return nDate.Ticks;
            } catch
            {
                return -1;
            }
        }

        public DittoToClipboardFusion()
        {
            serializer = new JsonSerializer { NullValueHandling = NullValueHandling.Ignore };
        }

        public void Migrate(string cstringDitto, string cstringClipboardFusion)
        {
            using (var ditto = new Database(cstringDitto, "System.Data.SQLite"))
            using (var cf = new Database(cstringClipboardFusion, "System.Data.SQLite"))
            {
                var page = 1L;
                long totalPages;

                var queryDitto = Sql.Builder
                    .Select("m.*, d.strClipBoardFormat, d.ooData")
                    .From("Main m")
                    .LeftJoin("Data d").On("m.LID = d.LParentID")
                    .OrderBy("m.LID");

                var machine = Environment.MachineName;
                var user = default(Guid).ToString().ToLowerInvariant();
                var i = 0;

                do
                {
                    var items = ditto.Page<DittoItem>(page, PageSize, queryDitto);
                    totalPages = items.TotalPages;
                    i += items.Items.Count;

                    Console.WriteLine($"Migarting {i} items out of total {items.TotalItems}.");

                    var transformed = items.Items.GroupBy(x => x.LID).Select(t =>
                    {
                        var x = t.First();
                        byte[] data;
                        try
                        {
                            data = ItemDataFrom(t.ToList());
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(
                                $"Error converting LID {x.LID}: {x.MText}{Environment.NewLine}\t{e.Message}");
                            return null;
                        }

                        return data != null
                            ? new ClipboardFusionItem
                            {
                                ItemAPIMachineID = machine,
                                ItemId = Guid.NewGuid().ToString().ToLowerInvariant(),
                                ItemLastModifiedDate = TimeFromUnixTimestamp(x.LDate),
                                ItemIsDeleted = 0,
                                ItemName = string.Empty,
                                ItemAPIUser = user,
                                ItemSource = "Ditto",
                                ItemData = data
                            }
                            : null;
                    }).Where(x => x != null).ToList();

                    if (transformed.Any())
                    {
                        using (var tran = cf.GetTransaction())
                        {
                            foreach (var item in transformed)
                            {
                                cf.Insert(item);
                            }

                            tran.Complete();
                        }
                    }

                    page++;
                } while (page <= totalPages);
            }
        }

        // ReSharper disable once ParameterTypeCanBeEnumerable.Local
        // ReSharper disable once SuggestBaseTypeForParameter
        private byte[] ItemDataFrom(List<DittoItem> l)
        {
            var x = l.First();

            var item = new ClipboardFusionItemData()
            {
                CommandLine = string.Empty,
                OwnerPath = string.Empty,
                WindowClass = string.Empty
            };

            switch (x.StrClipBoardFormat)
            {
                case "CF_UNICODETEXT":
                    {
                        item.Data = Encoding.Unicode.GetString(x.OoData);
                        break;
                    }
                case "CF_TEXT":
                    {
                        var unicode = l.FirstOrDefault(t =>
                            t.StrClipBoardFormat.Equals("CF_UNICODETEXT", StringComparison.OrdinalIgnoreCase));
                        item.Data = unicode != null ? Encoding.Unicode.GetString(unicode.OoData) : Encoding.ASCII.GetString(x.OoData);
                        break;
                    }
                case "CF_HDROP":
                    {
                        var data = x.OoData;
                        var hDrop = IntPtr.Zero;
                        try
                        {
                            hDrop = Marshal.AllocHGlobal(data.Length);
                            Marshal.Copy(data, 0, hDrop, data.Length);

                            var entries = NativeMethods.DragQueryFile(hDrop, 0xFFFFFFFF, null, 0);

                            if (entries <= 0)
                            {
                                return null;
                            }
                            
                            var files = new List<string>();

                            for (uint i = 0; i < entries; i++)
                            {                                
                                var buffer = NativeMethods.DragQueryFile(hDrop, i, null, 0);
                                var sb = new StringBuilder((int)buffer);
                                NativeMethods.DragQueryFile(hDrop, i, sb, (uint)sb.Capacity);                                
                                files.Add(sb.ToString());                                
                            }

                            item.FileDrop = string.Join("|", files);
                            item.PreferredDropEffect = string.Empty;
                        }
                        finally
                        {
                            if (hDrop != IntPtr.Zero)
                            {
                                Marshal.FreeHGlobal(hDrop);
                            }
                        }                        
                        break;
                    }
                case "CF_DIB":
                    {                        
                        var image = ImageHelper.GetBitmap(x.OoData);
                        item.Bitmap = image;
                        break;
                    }
                default:
                    {
                        return null;
                    }
            }

            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, item);
                return Encoding.Unicode.GetBytes(writer.ToString());
            }
        }
    }
}