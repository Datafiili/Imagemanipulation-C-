using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http.Headers;

namespace ImageEdit
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ImageOpener ImgO = new ImageOpener();
            Image I = ImgO.OpenImage();
        }
    }

    class Color
    {
        public byte red = 0;
        public byte green = 0;
        public byte blue = 0;
        public byte alpha = 0;

        public Color(byte r,byte g, byte b, byte a)
        {
            red = r;
            green = g;
            blue = b;
            alpha = a;
        }
    }

    internal class Image
    {
        int Width = 0;
        int Height = 0;
        List<Color> Colors = new List<Color>();

        public List<Color> InvertColors(List<Color> C)
        {
            for(int i = 0; i < C.Count; i++)
            {
                C[i] = new Color((byte)(255 - C[i].red), (byte)(255 - C[i].green),(byte)(255 - C[i].blue), C[i].alpha);
            }
            return C;
        }
    }

    internal class ImageOpener
    {
        public byte[] Bytes;
        public Image OpenImage()
        {
            byte[] Bytes = System.IO.File.ReadAllBytes("C:\\Users\\aarnjunk\\Documents\\Omena.png");

            byte[] Part = new byte[8];
            for (int i = 0; i < 8; i++)
            {
                Part[i] = Bytes[i];
            }
            Console.WriteLine(IsPNG(Part));
            return new Image();
        }

        public bool IsPNG(byte[] Header)
        {
            return CompareBytes(Header, new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
        }

        public bool CompareBytes(byte[] ByteSet1, byte[] ByteSet2)
        {
            //Compares if two byte sets are the same.
            if (ByteSet1.Length != ByteSet2.Length) //Must be same lenght
            {
                return false;
            }

            for (int i = 0; i < ByteSet1.Length; i++)
            {
                //Compares all bytes.
                if (ByteSet1[i] != ByteSet2[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
