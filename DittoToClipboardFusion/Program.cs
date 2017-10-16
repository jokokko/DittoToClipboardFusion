using System;
using System.IO;
using System.Linq;
using DittoToClipboardFusion.Services;
using Microsoft.Win32;

namespace DittoToClipboardFusion
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            Tuple<string, string> dbs;
            if (args.Any())
            {
                Console.WriteLine("Ditto db connection string > ");
                var ditto = Console.ReadLine();
                Console.WriteLine("ClipboardFsuion db connection string: ");
                var cf = Console.ReadLine();
                dbs = Tuple.Create(ditto, cf);
            }
            else
            {
                dbs = GetDbsFromRegistry();
            }

            if (dbs == null)
            {
                Console.WriteLine("No dbs provided.");
                return;
            }

            var migrator = new Services.DittoToClipboardFusion();
            migrator.Migrate(dbs.Item1, dbs.Item2);
            Console.WriteLine("Done");            
        }

        private static Tuple<string, string> GetDbsFromRegistry()
        {
            var dittoDb = (string)Registry.GetValue(@"HKEY_CURRENT_USER\Software\Ditto", "DBPath3", null);
            if (dittoDb == null)
            {
                Console.WriteLine(@"Ditto db path could not be resolved from the registry (HKEY_CURRENT_USER\Software\Ditto)");
                return null;
            }

            var cfDb = (string)Registry.GetValue(@"HKEY_CURRENT_USER\Software\Binary Fortress Software\ClipboardFusion", "DatabaseLocation", null);
            
            if (cfDb == null)
            {
                Console.WriteLine(
                    @"ClipboardFusion db path could not be resolved from the registry (HKEY_CURRENT_USER\Software\Ditto)");
                return null;
            }
            cfDb = Path.Combine(cfDb, "clipboardfusion.db");
            var pw = StringHelper.Get();

            Console.WriteLine($"Migrate from {dittoDb} to {cfDb}?");

            var a = Console.ReadLine() ?? string.Empty;

            return !a.StartsWith("y", StringComparison.OrdinalIgnoreCase) ? null : Tuple.Create($"Data Source={dittoDb};Version=3;;", $"Data Source={cfDb};Version=3;Password={pw};");
        }
    }

 
}
