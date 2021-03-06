﻿using System;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace QRReader.TwineData
{
    static class DibToBitmap
    {
        /// <summary>
        /// Get managed BitmapSource from a DIB provided as a low level windows hadle 
        ///
        /// Notes:
        /// Data is copied from the source so the windows handle can be saftely discarded
        /// even when the BitmapSource is in use.
        /// 
        /// Only a subset of possible DIB forrmats is supported.
        /// 
        /// </summary>
        /// <param name="dibHandle"></param>
        /// <returns>A copy of the image in a managed BitmapSource </returns>
        /// 
        public static BitmapSource FormHDib(IntPtr dibHandle)
        {
            BitmapSource bs = null;
            IntPtr bmpPtr = IntPtr.Zero;
            bool flip = true; // vertivcally flip the image

            try
            {
                bmpPtr = Win32.GlobalLock(dibHandle);
                Win32.BITMAPINFOHEADER bmi = new Win32.BITMAPINFOHEADER();
                Marshal.PtrToStructure(bmpPtr, bmi);

                if (bmi.biSizeImage == 0)
                    bmi.biSizeImage = (uint)(((((bmi.biWidth * bmi.biBitCount) + 31) & ~31) >> 3) * bmi.biHeight);

                int palettSize = 0;

                if (bmi.biClrUsed != 0)
                    throw new NotSupportedException("DibToBitmap: DIB with pallet is not supported");

                // pointer to the beginning of the bitmap bits
                IntPtr pixptr = (IntPtr)((int)bmpPtr + bmi.biSize + palettSize);

                // Define parameters used to create the BitmapSource.
                PixelFormat pf = PixelFormats.Default;
                switch (bmi.biBitCount)
                {
                    case 32:
                        pf = PixelFormats.Bgr32;
                        break;
                    case 24:
                        pf = PixelFormats.Bgr24;
                        break;
                    case 8:
                        pf = PixelFormats.Gray8;
                        break;
                    case 1:
                        pf = PixelFormats.BlackWhite;
                        break;
                    default:   // not supported
                        throw new NotSupportedException("DibToBitmap: Can't determine picture format (biBitCount=" + bmi.biBitCount + ")");
                    // break;
                }
                int width = bmi.biWidth;
                int height = bmi.biHeight;
                int stride = (int)(bmi.biSizeImage / height);
                byte[] imageBytes = new byte[stride * height];

                //Debug: Initialize the image with random data.
                //Random value = new Random();
                //value.NextBytes(rawImage);

                if (flip)
                {
                    for (int i = 0, j = 0, k = (height - 1) * stride; i < height; i++, j += stride, k -= stride)
                        Marshal.Copy(((IntPtr)((int)pixptr + j)), imageBytes, k, stride);
                }
                else
                {
                    Marshal.Copy(pixptr, imageBytes, 0, imageBytes.Length);
                }

                int xDpi = (int)Math.Round(bmi.biXPelsPerMeter * 2.54 / 100); // pels per meter to dots per inch
                int yDpi = (int)Math.Round(bmi.biYPelsPerMeter * 2.54 / 100);

                // Create a BitmapSource.
                bs = BitmapSource.Create(width, height, xDpi, yDpi, pf, null, imageBytes, stride);

            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }
            finally
            {
                // cleanup
                if (bmpPtr != IntPtr.Zero)
                { // locked sucsessfully
                    Win32.GlobalUnlock(dibHandle);
                }
            }
            return bs;
        }

    }
}
