using System;
using System.Collections.Generic;

//Notes:
//https://www.w3.org/TR/PNG-Chunks.html

namespace ImageEdit
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ImageOpener ImgO = new ImageOpener();
            Image I = ImgO.OpenImage("Horror.png");
        }
    }

    class Color
    {
        public byte red = 0;
        public byte green = 0;
        public byte blue = 0;
        public byte alpha = 0;

        public Color(byte r, byte g, byte b, byte a)
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
            for (int i = 0; i < C.Count; i++)
            {
                C[i] = new Color((byte)(255 - C[i].red), (byte)(255 - C[i].green), (byte)(255 - C[i].blue), C[i].alpha);
            }
            return C;
        }
    }

    internal class Chunk
    {
        public byte[] ChunkType = new byte[4];
        public byte[] ChunkData = new byte[0];
        public byte[] CRC = new byte[4];
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

            // -- Checks for png file signature -- //
            byte[] Header = new byte[8];
            Array.Copy(Bytes, 0, Header, 0, 8);

            if (IsPNG(Header) == false)
            {
                Console.WriteLine("Isn't .png file");
                return null;
            }
            Console.WriteLine("Is .png file");

            // -- Count the amount of chunks -- //
            int ChunkCount = 0;
            int ChunkIndex = 8; //Where to start looking for
            while (true) {
                byte[] ChunkLength = new byte[4];
                Array.Copy(Bytes, ChunkIndex, ChunkLength, 0, 4);
                int ChunkLengthInt = BytesToDecimal(ChunkLength);
                ChunkCount++;
                ChunkIndex += 12 + ChunkLengthInt;

                if (ChunkIndex >= Bytes.Length)
                {
                    break;
                }
            }

            
            // -- Turning bytes to chunks -- //
            Console.WriteLine("// -- Chunks -- //");
            Chunk[] Chunks = new Chunk[ChunkCount];
            ChunkIndex = 8;
            for(int i = 0; i < ChunkCount; i++)
            {
                byte[] ChunkLength = new byte[4];
                Array.Copy(Bytes, ChunkIndex, ChunkLength, 0, 4);
                int ChunkLengthInt = BytesToDecimal(ChunkLength);
                byte[] ChunkData = new byte[ChunkLengthInt + 8];
                Array.Copy(Bytes, ChunkIndex + 4, ChunkData, 0, ChunkLengthInt + 8);

                Chunks[i] = new Chunk(ChunkData);
                ChunkIndex += ChunkLengthInt + 12;
            }

            // -- Checking if all required chunks appear -- //
            bool[] requiredChunks = new bool[] { false,false,false}; // IHDR, IDAT, IEND
            for (int i = 0; i < Chunks.Length; i++)
            {
                //IHDR
                if(CompareBytes(Chunks[i].ChunkType, new byte[] { 73, 72, 68, 82 })){
                    requiredChunks[0] = true;
                }
                //IDAT
                if (CompareBytes(Chunks[i].ChunkType, new byte[] { 73, 68, 65, 84 }))
                {
                    requiredChunks[1] = true;
                }
                //IEND
                if (CompareBytes(Chunks[i].ChunkType, new byte[] { 73, 69, 78, 68 }))
                {
                    requiredChunks[2] = true;
                }
            }
            for(int i = 0; i < requiredChunks.Length; i++){
                if(requiredChunks[i] == false){
                    Console.WriteLine("Missing Critical Chunk!");
                    if (i == 0) { Console.WriteLine("IHDR"); }
                    if (i == 1) { Console.WriteLine("IDAT"); }
                    if (i == 2) { Console.WriteLine("IEND"); }
                    return null;
                }
            }
            Console.WriteLine("All critical chunks appear.");

            // -- Opener variables -- //
            int width = 0; //4 bytes
            int height = 0; //4 bytes
            int bitDepth = 0; //1 byte
            int colorType = 0; //1 byte -> 0 = Indexed, 2 = Grayscale, 3 = Grayscale & alpha, 4 = Truecolor, 6 = Truecolor & alpha
            int compressionMethod = 0; //1 byte
            int filterMethod = 0; //1 byte
            int interlaceMethod = 0; //1 byte

            // -- Convert chunks to image -- //
            Console.WriteLine("// -- Chunks -- //");

            // -- IHDR -- //
            int IHDRindex = -1;
            for (int i = 0; i < Chunks.Length; i++)
            {
                if (CompareBytes(Chunks[i].ChunkType, new byte[] { 73, 72, 68, 82 }))
                {
                    IHDRindex = i;
                }
                break;
            }

            if (CompareBytes(Chunks[IHDRindex].ChunkType, new byte[] { 73, 72, 68, 82 })){
                
                byte[] holder = new byte[4];
                Array.Copy(Chunks[IHDRindex].ChunkData, 0, holder, 0, 4);
                width = BytesToDecimal(holder);

                Array.Copy(Chunks[IHDRindex].ChunkData, 4, holder, 0, 4);
                height = BytesToDecimal(holder);

                bitDepth = Chunks[IHDRindex].ChunkData[8];
                colorType = Chunks[IHDRindex].ChunkData[9];
                compressionMethod = Chunks[IHDRindex].ChunkData[10];
                filterMethod = Chunks[IHDRindex].ChunkData[11];
                interlaceMethod = Chunks[IHDRindex].ChunkData[12];

                Console.WriteLine("IHDR:");
                Console.WriteLine("width: " + width);
                Console.WriteLine("height: " + height);
                Console.WriteLine("bitDepth: " + bitDepth);
                Console.WriteLine("colorType: " + colorType);
                Console.WriteLine("compressionMethod: " + compressionMethod);
                Console.WriteLine("filterMethod: " + filterMethod);
                Console.WriteLine("interlaceMethod: " + interlaceMethod);
            }





            Image I = new Image();

            return I;
        }

        public int BytesToDecimal(byte[] data)
        {
            //Used for converting byte arrays to strings.
            //Mostly for determening the length of some elements.
            int number = 0;
            for (int i = 0; i < data.Length; i++)
            {
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
