using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace DittoToClipboardFusion.Services
{
    internal static class ImageHelper
    {
        public static byte[] GetBitmap(byte[] data)
        {
            var ptr = IntPtr.Zero;

            try
            {
                ptr = Marshal.AllocHGlobal(data.Length);
                Marshal.Copy(data, 0x0, ptr, data.Length);

                var bmi = (NativeMethods.BITMAPINFO)Marshal.PtrToStructure(ptr, typeof(NativeMethods.BITMAPINFO));

                using (var bitmap = new Bitmap(bmi.biWidth, bmi.biHeight))
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    var hdc = graphics.GetHdc();

                    try
                    {
                        NativeMethods.SetDIBitsToDevice(
                            hdc,
                            0,
                            0,
                            bmi.biWidth,
                            bmi.biHeight,
                            0,
                            0,
                            0,
                            bmi.biHeight,
                            ptr,
                            ptr,
                            0);
                    }
                    finally
                    {
                        graphics.ReleaseHdc(hdc);
                    }

                    return (byte[])Converter.ConvertTo(bitmap, typeof(byte[]));
                }
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(ptr);
                }
            }
        }

        private static readonly ImageConverter Converter = new ImageConverter();
    }
}
