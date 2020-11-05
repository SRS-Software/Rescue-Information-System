#region

using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

#endregion

namespace RIS.Core.Helper
{
    public class TiffConverter
    {
        public static Bitmap TiffToBitmap(string path)
        {
            var finalWidth = 0;
            var finalHeigth = 0;
            Bitmap finalImage = null;

            var bitmapList = TiffToBitmapList(path);
            if (bitmapList.Count == 1) return new Bitmap(bitmapList[0]);

            //update the size of the final bitmap
            foreach (var bitmap in bitmapList)
            {
                finalHeigth += bitmap.Height;
                finalWidth = bitmap.Width > finalWidth ? bitmap.Width : finalWidth;
            }

            //create a bitmap to hold the combined image
            finalImage = new Bitmap(finalWidth, finalHeigth);

            //get a graphics object from the image so we can draw on it
            using (var g = Graphics.FromImage(finalImage))
            {
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.InterpolationMode = InterpolationMode.High;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                //go through each image and draw it on the final image
                var offset = 0;
                foreach (var bitmap in bitmapList)
                {
                    g.DrawImage(bitmap, 0, offset, bitmap.Width, bitmap.Height);
                    offset += bitmap.Height;
                }
            }

            return new Bitmap(finalImage);
        }

        public static List<Bitmap> TiffToBitmapList(string path)
        {
            var bitmapList = new List<Bitmap>();

            if (!WaitFileReady.Check(path)) return null;

            using (var fileStream = new FileStream(path, FileMode.Open))
            {
                var tifImage = Image.FromStream(fileStream);
                var tifGuid = tifImage.FrameDimensionsList[0];
                var tifDimension = new FrameDimension(tifGuid);
                var countPages = tifImage.GetFrameCount(tifDimension);

                for (var i = 0; i < countPages; i++)
                    using (var memoryStream = new MemoryStream())
                    {
                        tifImage.SelectActiveFrame(tifDimension, i);
                        tifImage.Save(memoryStream, ImageFormat.Bmp);
                        var bitmap = new Bitmap(memoryStream);
                        bitmapList.Add(bitmap);
                    }
            }

            return bitmapList;
        }

        public static void SaveTiffAsImage(string sourcePath, string targetPath)
        {
            var bitmap = TiffToBitmap(sourcePath);
            bitmap.Save(targetPath);

            WaitFileReady.Check(targetPath);
        }
    }
}