using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace TelegaBot.BotStuff
{
    public class PhotoProcessing
    {
        public Bitmap BlendingPhotos(string backgroundPath, string foregroundPath)
        {
            const int k = 71;
            const double d = 1.0855;

            var origFg = new Bitmap(foregroundPath);

            var bg = new Bitmap(backgroundPath);
            var fg = new Bitmap(origFg, new Size((int)(bg.Width / d) - k, (int)(bg.Width / d) - 112 - k));

            (var fgW, var fgH) = (fg.Width, fg.Height);
            (var bgW, var bgH) = (bg.Width, bg.Height);

            /*var fgBmpData = fg.LockBits(new Rectangle(0, 0, fgW, fgH), ImageLockMode.WriteOnly,
                PixelFormat.Format24bppRgb);
            var fgScan = fgBmpData.Scan0;
            
            var bgBmpData = bg.LockBits(new Rectangle(0, 0, bgW, bgH), ImageLockMode.WriteOnly,
                PixelFormat.Format24bppRgb);
            var bgScan = bgBmpData.Scan0;


            unsafe
            {
                byte* ptr = (byte*)fgScan.ToPointer();
                byte* ptrHelper = ptr;
            
                for (int y = 0; y < fgH; y++)
                {
                    for (int x = 0; x < fgW; x++)
                    {
            
                    }
            
                    ptrHelper += fgBmpData.Stride;
                    ptr = ptrHelper;
                }
            }
            
            fg.UnlockBits(fgBmpData);
            bg.UnlockBits(bgBmpData);*/

            for (int x = 0; x < fg.Width; x++)
            {
                for (int y = 0; y < fg.Height; y++)
                {
                    Color color = fg.GetPixel(x, y);
                    bg.SetPixel(x + k, y + k, Color.FromArgb(color.R, color.G, color.B));
                }
            }

            origFg.Dispose();
            fg.Dispose();
            File.Delete(foregroundPath);

            return bg;
        }

        public Bitmap TextOnPhoto(Bitmap photo, string topText, string bottomText)
        {
            const int textHeight = 70;
            var topY = photo.Height / 2 + 330;
            var bottomY = topY + textHeight;

            var fromImage = Graphics.FromImage(photo);
            fromImage.DrawString(topText == string.Empty ? "Текст" : topText,
                new Font("Arial", 36, FontStyle.Italic), new SolidBrush(Color.White),
                new RectangleF(0, topY, photo.Width, textHeight),
                new StringFormat(StringFormatFlags.NoWrap) { Alignment = StringAlignment.Center });
            fromImage.DrawString(bottomText == string.Empty ? "Текст" : bottomText,
                new Font("Arial", 22, FontStyle.Regular), new SolidBrush(Color.White),
                new RectangleF(0, bottomY, photo.Width, textHeight),
                new StringFormat(StringFormatFlags.NoWrap) { Alignment = StringAlignment.Center });


            fromImage.Dispose();

            return photo;
        }
    }
}