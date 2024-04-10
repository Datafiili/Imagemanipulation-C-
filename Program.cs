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
            Image I = ImgO.OpenImage("Hullu.png");
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
            Console.WriteLine(ByteToASCII(ChunkType) + " " + ChunkType[0]  + " " + ChunkType[1]  + " " + ChunkType[2]  + " " + ChunkType[3]);
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
            //IHDR -> Must be first chunk
            if(CompareBytes(Chunks[0].ChunkType, new byte[] { 73, 72, 68, 82 }) == false){
                Console.WriteLine("ERROR: IHDR Chunk missing!");
                return null;
            }
            //IEND -> Must be last chunk
            if (CompareBytes(Chunks[Chunks.Length - 1].ChunkType, new byte[] { 73, 69, 78, 68 }) == false)
            {
                Console.WriteLine("ERROR: IEND Chunk missing!");
                return null;
            }
            //IDAT -> Must somewhere and must be all IDATs in a row
            bool HasIDAT = false;
            for (int i = 1; i < Chunks.Length - 1; i++)
            {
                //IDAT
                if (CompareBytes(Chunks[i].ChunkType, new byte[] { 73, 68, 65, 84 }))
                {
                    HasIDAT = true;
                    break;
                }
            }

            if(HasIDAT == false)
            {
                Console.WriteLine("ERROR: IDAT Chunk missing!");
                return null;
            }
            Console.WriteLine("All critical chunks appear.");

            // -------------------- Checking Chunks In Order -------------------- //

            int CurrenctChunk = 0;

            // -- variables -- //
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
            //Must be first and always is.

            if (CompareBytes(Chunks[CurrenctChunk].ChunkType, new byte[] { 73, 72, 68, 82 }))
            {
                
                byte[] holder = new byte[4];
                Array.Copy(Chunks[CurrenctChunk].ChunkData, 0, holder, 0, 4);
                width = BytesToDecimal(holder);

                Array.Copy(Chunks[CurrenctChunk].ChunkData, 4, holder, 0, 4);
                height = BytesToDecimal(holder);

                bitDepth = Chunks[CurrenctChunk].ChunkData[8];
                colorType = Chunks[CurrenctChunk].ChunkData[9];
                compressionMethod = Chunks[CurrenctChunk].ChunkData[10];
                filterMethod = Chunks[CurrenctChunk].ChunkData[11];
                interlaceMethod = Chunks[CurrenctChunk].ChunkData[12];

                Console.WriteLine("IHDR:");
                Console.WriteLine("width: " + width);
                Console.WriteLine("height: " + height);
                Console.WriteLine("bitDepth: " + bitDepth);
                Console.WriteLine("colorType: " + colorType);
                Console.WriteLine("compressionMethod: " + compressionMethod);
                Console.WriteLine("filterMethod: " + filterMethod);
                Console.WriteLine("interlaceMethod: " + interlaceMethod);
            }

            float ImageGamma = -1f; //If negative -> No Gamma determined.

            bool FoundPLTE = false;
            while(true)
            {
                CurrenctChunk++;
                
                //Must appear before PLTE if they even appear.
                switch(System.Text.Encoding.ASCII.GetString(Chunks[CurrenctChunk].ChunkType)) 
                {
                case "sRGB":
                    Console.WriteLine("Call sRGB function");
                    break;
                case "cHRM":
                    Console.WriteLine("Call cHRM function");
                    break;
                case "gAMA":
                    Console.WriteLine("Call gAMA function");
                    ImageGamma = gAMA(Chunks[CurrenctChunk]);     
                    break;
                case "sBIT":
                    Console.WriteLine("Call sBIT function");                
                    break;
                case "PLTE":
                    FoundPLTE = true;   
                    break;
                }
                 
                if(FoundPLTE){
                    break;
                }
            }

            // -- PLTE -- //
            
            //Must appear for 3, can appear for 2 & 6
            //Mismaching cases
            if(colorType == 3 && !FoundPLTE){
                return null;
            }

            if(colorType == 0 || colorType == 4) //can't appear for 0 & 4
            {
                if(FoundPLTE){
                    return null;
                }
            }
            
            Color[] ColorPalette = new Color[]{};
            if (FoundPLTE && CompareBytes(Chunks[CurrenctChunk].ChunkType, new byte[] { 80, 76, 84, 69 }))
            {   
                //Checking for errors.
                if(Chunks[CurrenctChunk].ChunkData.Length % 3 != 0){
                    Console.WriteLine("ERROR: Palette has wrong amount of data!");
                    return null;
                }
                if(Math.Pow(2,bitDepth) < Chunks[CurrenctChunk].ChunkData.Length / 3)
                {
                    Console.WriteLine("Error: Palette has too many entries!");
                    return null;
                }

                ColorPalette = new Color[(int)(Chunks[CurrenctChunk].ChunkData.Length / 3)];
                for(int i = 0; i < (int)(Chunks[CurrenctChunk].ChunkData.Length / 3); i++)
                {
                    ColorPalette[i] = new Color(Chunks[CurrenctChunk].ChunkData[i*3],Chunks[CurrenctChunk].ChunkData[i*3 + 1],Chunks[CurrenctChunk].ChunkData[i*3 + 1], 0);
                }
            }

            // -------------------- Ancillary chunks -------------------- //
            // -- bKGD -- //
            int bKGDindex = -1;
            for (int i = 0; i < Chunks.Length; i++)
            {
                if (CompareBytes(Chunks[i].ChunkType, new byte[] { 98, 75, 71, 68 }))
                {
                    bKGDindex = i;
                    break;
                }
            }

            if (bKGDindex > 0 && CompareBytes(Chunks[bKGDindex].ChunkType, new byte[] { 98, 75, 71, 68 }))
            {   
                //TODO: PALAA ASIAAN
                if(colorType == 3) //Contains palette index
                {

                }

                /*

                he bKGD chunk specifies a default background color to present the image against. Note that viewers are not bound to honor this chunk; a viewer can choose to use a different background.

                For color type 3 (indexed color), the bKGD chunk contains:

                Palette index:  1 byte

                The value is the palette index of the color to be used as background.

                For color types 0 and 4 (grayscale, with or without alpha), bKGD contains:

                Gray:  2 bytes, range 0 .. (2^bitdepth)-1

                (For consistency, 2 bytes are used regardless of the image bit depth.) The value is the gray level to be used as background.

                For color types 2 and 6 (truecolor, with or without alpha), bKGD contains:

                Red:   2 bytes, range 0 .. (2^bitdepth)-1
                Green: 2 bytes, range 0 .. (2^bitdepth)-1
                Blue:  2 bytes, range 0 .. (2^bitdepth)-1

                (For consistency, 2 bytes per sample are used regardless of the image bit depth.) This is the RGB color to be used as background.

                When present, the bKGD chunk must precede the first IDAT chunk, and must follow the PLTE chunk, if any. 

                */
            }

            // -- cHRM  -- //
            int cHRMindex = -1;
            for (int i = 0; i < Chunks.Length; i++)
            {
                if (CompareBytes(Chunks[i].ChunkType, new byte[] { 99, 72, 82, 77 }))
                {
                    cHRMindex = i;
                    break;
                }
            }

            if (cHRMindex > 0 && CompareBytes(Chunks[cHRMindex].ChunkType, new byte[] { 99, 72, 82, 77 }))
            {   
                
            }

            Image I = new Image();

            return I;
        }

        public float gAMA(Chunk C)
        {
            return BytesToDecimal(C.ChunkData);
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