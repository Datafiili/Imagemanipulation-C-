using System;
using System.Collections.Generic;

namespace ImageEdit
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ImageOpener ImgO = new ImageOpener();
            Image I = ImgO.OpenImage("C:\\Users\\aarnjunk\\Documents\\Big-O.png");
            Console.ReadLine();
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

    internal class Chunk
    {
        byte[] ChunkType = new byte[4];
        byte[] ChunkData = new byte[0];
        byte[] CRC = new byte[4];
        public Chunk(byte[] Data)
        {
            Array.Copy(Data, 0, ChunkType, 0, 4);
            ChunkData = new byte[Data.Length - 8]; //Resizes the ChunkData
            Array.Copy(Data, 4, ChunkData, 0, Data.Length - 8);
            Array.Copy(Data, Data.Length - 4 - 1, CRC, 0, 4);
            //Debugging
            Console.WriteLine(ByteToASCII(ChunkType));
            //Console.WriteLine(ChunkData);
            //Console.WriteLine(CRC);
        }

        public string ByteToASCII(byte[] data)
        {
            return System.Text.Encoding.ASCII.GetString(data);
        }
    }

    internal class ImageOpener
    {
        public byte[] Bytes;
        public Image OpenImage(string path)
        {
            byte[] Bytes = System.IO.File.ReadAllBytes(path);

            byte[] Header = new byte[8];
            Array.Copy(Bytes, 0, Header, 0, 8);

            Console.WriteLine("Is png:" + IsPNG(Header));
            if(IsPNG(Header) == false){
                return null;
            }

            Chunk[] Chunks = new Chunk[0];
            int ChunkIndex = 8;
            while(true){
                byte[] ChunkLength = new byte[4];
                Array.Copy(Bytes, ChunkIndex, ChunkLength, 0, 4);
                int ChunkLengthInt = BytesToDecimal(ChunkLength);
                byte[] ChunkData = new byte[ChunkLengthInt + 8];
                Array.Copy(Bytes, ChunkIndex + 4, ChunkData, 0, ChunkLengthInt + 8);
                
                //Lengthens the array by one. We don't know how many chunks will be in the file.
                //TODO: determine how many chunks are there, before checking each of them.
                Chunk[] Holder = new Chunk[Chunks.Length];
                Array.Copy(Chunks,0,Holder,0,Chunks.Length);
                Chunks = new Chunk[Chunks.Length + 1];
                Array.Copy(Holder,0,Chunks,0,Holder.Length);

                Chunks[Chunks.Length - 1] = new Chunk(ChunkData);
                ChunkIndex += ChunkLengthInt + 12;
                if(ChunkIndex >= Bytes.Length){
                    break;
                }
            }

            Image I = new Image();
            
            return I;
        }

        public int BytesToDecimal(byte[] data){
            int number = 0;
            for(int i = 0; i < data.Length; i++){
                number += (int)(Math.Pow(2, (data.Length - 1 - i) * 8) * data[i]);
            }

            return number;
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
