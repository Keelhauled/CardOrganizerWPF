using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace PluginLibrary
{
    public static class Watermark
    {
        public static Texture2D LoadResourceTexture(string resourceName)
        {
            try
            {
                using(var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                {
                    byte[] buffer = new byte[16 * 1024];
                    var pngData = ReadFully(stream);
                    return PngAssist.ChangeTextureFromByte(pngData);
                }
            }
            catch(Exception)
            {
                Console.WriteLine("Error accessing resources");
                throw;
            }
        }

        public static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using(MemoryStream ms = new MemoryStream())
            {
                int read;
                while((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        public static Texture2D AddWatermark(Texture2D background, Texture2D watermark, int margin)
        {
            int startX = margin;
            int startY = background.height - watermark.height - margin;
            int endX = startX + watermark.width;
            int endY = startY + watermark.height;

            for(int x = startX; x < endX; x++)
            {
                for(int y = startY; y < endY; y++)
                {
                    var bgColor = background.GetPixel(x, y);
                    var wmColor = watermark.GetPixel(x - startX, y - startY);
                    var final_color = Color.Lerp(bgColor, wmColor, wmColor.a / 1.0f);
                    background.SetPixel(x, y, final_color);
                }
            }

            background.Apply();
            return background;
        }
    }
}
