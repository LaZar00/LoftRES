// This code is based in the source C++ code of "tramboi",
// and converted to C#
// https://sourceforge.net/projects/unveil/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LoftRES.DecryptRES;

namespace LoftRES
{


    class DecryptRES
    {

        public const int _ONE_DIRECTIVE = 1;


        public struct SpriteDesc_t
        {
            public ushort count;
            public ushort height;
            public byte width;
            public byte field_4;
            public byte compressionAlgo;
            public byte field_6;
            public byte[] RAWData;
            public byte[] DecryptedData;
        }

        public struct FontDesc_t
        {
            public byte[] tbl_fontswidths;
            public ushort width;
            public ushort height;
            public byte field_4;
            public byte field_5;
            public byte compressionAlgo;
            public byte field_7;
            //public byte field_8;
            public byte[] RAWData;
            public byte[] DecryptedData;
        }

        public struct Resource_t
        {
            public byte[] magic;
            public byte type;
            public ushort size;
            public SpriteDesc_t spriteDesc;
            public FontDesc_t fontDesc;

            public Resource_t()
            {
                magic = new byte[2];
            }
        }

        public static Resource_t resourceRAW;


        // This is very basic. We will copy the RAWData practically
        // direct as DecryptedData.
        //public static void Extract0(int outputSize)
        //{

        //}

        public static byte[] Reverse(byte[] tmpFrame, int HEIGHT, int WIDTH)
        {
            int w, h, hinv;
            hinv = HEIGHT - 1;

            byte[] tmpRevFrame = new byte[tmpFrame.Length];

            for (h = 0; h < HEIGHT; h++)
            {
                for (w = 0; w < WIDTH; w++)
                {
                    tmpRevFrame[hinv * WIDTH + w] = tmpFrame[h * WIDTH + w];
                }

                hinv--;
            }

            return tmpRevFrame;
        }


        public static void Decompress1(int outputSize)
        {
            int dst = 0;
            int src = 0;
            int FSpriteCount = resourceRAW.spriteDesc.count;
            int FDecodedBytesPerSprite = outputSize;
            int readOneByte = 0;

            int readBytes = 0;
            int writtenBytes = 0;

            while (writtenBytes < FDecodedBytesPerSprite * FSpriteCount)
            {
                sbyte v = (sbyte)resourceRAW.spriteDesc.RAWData[readOneByte++];
                ++readBytes;
                if (v < 0)
                {
                    int count = -v + 1;
                    byte v2 = resourceRAW.spriteDesc.RAWData[readOneByte++];
                    ++readBytes;
                    //count = std::min<int>(count, FDecodedBytesPerSprite*FSpriteCount-writtenBytes);
                    for (int x = count; count > 0; x++)
                        resourceRAW.spriteDesc.DecryptedData[dst++] = v2;
                    dst += count; writtenBytes += count;
                    Debug.Assert(writtenBytes <= FDecodedBytesPerSprite * FSpriteCount);
                }
                else
                {
                    int count = v + 1;
                    //count = std::min<int>(count, FDecodedBytesPerSprite*FSpriteCount-writtenBytes);
                    for (int x = count; x > 0; x++)
                        resourceRAW.spriteDesc.DecryptedData[dst++] = 
                                       resourceRAW.spriteDesc.DecryptedData[src++];
                    readBytes += count;
                    dst += count; writtenBytes += count;
                    Debug.Assert(writtenBytes <= FDecodedBytesPerSprite * FSpriteCount);
                }
            }
            Debug.Assert(writtenBytes == FDecodedBytesPerSprite * FSpriteCount);
            Console.WriteLine("        Decompression done... " + readBytes.ToString() +
                              " -> " + (FSpriteCount * FDecodedBytesPerSprite).ToString() + " bytes");
        }


        //public static void unswizzle(int buffer, int size)
        //{
        //}


        public static void OutputByteImage(ref byte v, ref bool FFinished, ref int FDst, 
                                           ref int FBytesWrittenForCurrentSprite, ref int FDecodedBytesPerSprite,
                                           ref int FCurrentSpriteIndex, ref int FSpriteCount)
        {
            if (FFinished)
                return;

            resourceRAW.spriteDesc.DecryptedData[FDst++] = v;
            ++FBytesWrittenForCurrentSprite;
            if (FBytesWrittenForCurrentSprite == FDecodedBytesPerSprite)
            {
                //unswizzle(FDst - FDecodedBytesPerSprite, FDecodedBytesPerSprite);
                Console.WriteLine("        Sprite " + FCurrentSpriteIndex.ToString() + 
                                  " done", FCurrentSpriteIndex);
                ++FCurrentSpriteIndex;
                FBytesWrittenForCurrentSprite = 0;
                FFinished = (FCurrentSpriteIndex == FSpriteCount);
            }
        }

        public static void OutputByteFont(ref byte v, ref bool FFinished, ref int FDst,
                                          ref int FBytesWrittenForCurrentSprite, ref int FDecodedBytesPerSprite,
                                          ref int FCurrentSpriteIndex, ref int FSpriteCount)
        {
            if (FFinished)
                return;

            resourceRAW.fontDesc.DecryptedData[FDst++] = v;
            ++FBytesWrittenForCurrentSprite;
            if (FBytesWrittenForCurrentSprite == FDecodedBytesPerSprite)
            {
                //unswizzle(FDst - FDecodedBytesPerSprite, FDecodedBytesPerSprite);
                Console.WriteLine("        Font " + FCurrentSpriteIndex.ToString() +
                                  " done", FCurrentSpriteIndex);
                ++FCurrentSpriteIndex;
                FBytesWrittenForCurrentSprite = 0;
                FFinished = (FCurrentSpriteIndex == FSpriteCount);
            }
        }


        public static void Decompress3Image(int outputSize)
        {
            int i;
            int dst = 0;
            bool FFinished = false;
            int readOneByte = 0;
            int FBytesRead = 0;
            int FSpriteCount = resourceRAW.spriteDesc.count;
            int FDecodedBytesPerSprite = outputSize;
            int FBytesWrittenForCurrentSprite = 0;
            int FCurrentSpriteIndex = 0;

            byte[] history = new byte[0x1000];
            for (i = 0; i < 0x1000; i++) history[i] = 0xFE;
            int it = 0xFEE;

            int si = 0;
            while (!FFinished)
            {
                //printf ("si = %04x \n", si);
                si >>= 1;
                if ((si & 0x100) == 0)
                {
                    si = resourceRAW.spriteDesc.RAWData[readOneByte++];
                    si |= 0xFF00;
                }
                if ((si & 1) != 0)
                {
                    byte v = resourceRAW.spriteDesc.RAWData[readOneByte++];
                    OutputByteImage(ref v, ref FFinished, ref dst,
                                    ref FBytesWrittenForCurrentSprite, ref FDecodedBytesPerSprite,
                                    ref FCurrentSpriteIndex, ref FSpriteCount);
                    history[it] = v;
                    it = (it + 1) & 0xFFF;
                }
                else
                {
                    int v1 = resourceRAW.spriteDesc.RAWData[readOneByte++];
                    int v2 = resourceRAW.spriteDesc.RAWData[readOneByte++];

                    v1 |= (v2 & 0xF0) << 4;
                    int di = (v2 & 0x0F) + 2;

                    int cx = 0;
                    do
                    {
                        int bx = (v1 + cx) & 0xFFF;
                        byte v = history[bx];
                        OutputByteImage(ref v, ref FFinished, ref dst,
                                        ref FBytesWrittenForCurrentSprite, ref FDecodedBytesPerSprite,
                                        ref FCurrentSpriteIndex, ref FSpriteCount);
                        history[it] = v;
                        it = (it + 1) & 0xFFF;
                        ++cx;
                    } while (cx <= di);
                }
            }
            
            Console.WriteLine("        Decompression done... " + FBytesRead.ToString() +
                              " -> " + (FSpriteCount * FDecodedBytesPerSprite).ToString() + " bytes");
        }

        public static void Decompress3Font(int outputSize)
        {
            int dst = 0;
            bool FFinished = false;
            int readOneByte = 0;
            int FBytesRead = 0;
            int FSpriteCount = 1;
            int FDecodedBytesPerSprite = outputSize;
            int FBytesWrittenForCurrentSprite = 0;
            int FCurrentSpriteIndex = 0;

            byte[] history = new byte[0x1000];
            Array.Fill(history, (byte)0xFE);
            //for (i = 0; i < 0x1000; i++) history[i] = 0xFE;
            int it = 0xFEE;

            int si = 0;
            while (!FFinished)
            {
                //printf ("si = %04x \n", si);
                si >>= 1;
                if ((si & 0x100) == 0)
                {
                    si = resourceRAW.fontDesc.RAWData[readOneByte++];
                    si |= 0xFF00;
                }
                if ((si & 1) != 0)
                {
                    byte v = resourceRAW.fontDesc.RAWData[readOneByte++];
                    OutputByteFont(ref v, ref FFinished, ref dst,
                                   ref FBytesWrittenForCurrentSprite, ref FDecodedBytesPerSprite,
                                   ref FCurrentSpriteIndex, ref FSpriteCount);
                    history[it] = v;
                    it = (it + 1) & 0xFFF;
                }
                else
                {
                    int v1 = resourceRAW.fontDesc.RAWData[readOneByte++];
                    int v2 = resourceRAW.fontDesc.RAWData[readOneByte++];

                    v1 |= (v2 & 0xF0) << 4;
                    int di = (v2 & 0x0F) + 2;

                    int cx = 0;
                    do
                    {
                        int bx = (v1 + cx) & 0xFFF;
                        byte v = history[bx];
                        OutputByteFont(ref v, ref FFinished, ref dst,
                                       ref FBytesWrittenForCurrentSprite, ref FDecodedBytesPerSprite,
                                       ref FCurrentSpriteIndex, ref FSpriteCount);
                        history[it] = v;
                        it = (it + 1) & 0xFFF;
                        ++cx;
                    } while (cx <= di);
                }
            }

            Console.WriteLine("        Decompression done... " + FBytesRead.ToString() +
                              " -> " + (FSpriteCount * FDecodedBytesPerSprite).ToString() + " bytes");
        }


        public static void SetStandardPAL(ref byte[] palette)
        {
            int i;

            for (i = 0; i < 256; i++)
            {
                palette[i * 3] = (byte)i;
                palette[(i * 3) + 1] = (byte)i;
                palette[(i * 3) + 2] = (byte)i;
            }
        }


        public static void LoadColors(ref byte[] palette, int i_numpal)
        {
            int i_numcolors;

            try
            {
                if (File.Exists("COLORS"))
                {
                    FileStream filePAL = new("COLORS", FileMode.Open);
                    BinaryReader readerPAL = new(filePAL);

                    i_numcolors = readerPAL.ReadInt32();

                    if (i_numpal > i_numcolors)
                    {
                        SetStandardPAL(ref palette);
                        Console.WriteLine("\nThe number of palettes in COLORS file is less than the one you set.\nWe will use a standard palette.");
                    }
                    else
                    {
                        readerPAL.BaseStream.Position = 1 + (i_numpal * 768);

                        palette = readerPAL.ReadBytes(768);
                    }

                    filePAL.Close();
                }
                else
                {
                    SetStandardPAL(ref palette);
                    Console.WriteLine("COLORS file does not exists. We will use a standard palette.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
        }


        public static void WriteOneByte(BinaryWriter writerTGA, int inputByte)
        {
            writerTGA.Write((byte)inputByte);
        }


        public static void SaveRESTGAImage(string filename, int i_numpal)
        {
            byte[] palette = new byte[768];
            int i, j, w, h, rbase;

            LoadColors(ref palette, i_numpal);

            try
            {
                MemoryStream memTGA = new();
                BinaryWriter writerTGA = new(memTGA);

                WriteOneByte(writerTGA, 0);
                WriteOneByte(writerTGA, 1);
                WriteOneByte(writerTGA, 1);

                // Palette
                WriteOneByte(writerTGA, 0);
                WriteOneByte(writerTGA, 0);
                WriteOneByte(writerTGA, 0);
                WriteOneByte(writerTGA, 1);
                WriteOneByte(writerTGA, 24);

                WriteOneByte(writerTGA, 0);
                WriteOneByte(writerTGA, 0);
                WriteOneByte(writerTGA, 0);
                WriteOneByte(writerTGA, 0);

                if (_ONE_DIRECTIVE == 1)
                {
                    writerTGA.Write(
                        (ushort)(resourceRAW.spriteDesc.width * 8));
                    writerTGA.Write(
                        (ushort)(resourceRAW.spriteDesc.height * resourceRAW.spriteDesc.count));
                }
                else
                {
                    writerTGA.Write(
                        (ushort)(resourceRAW.spriteDesc.height * resourceRAW.spriteDesc.count));
                    writerTGA.Write(
                        (ushort)(resourceRAW.spriteDesc.width * 8));
                }

                WriteOneByte(writerTGA, 8);
                WriteOneByte(writerTGA, 0);

                if (_ONE_DIRECTIVE == 0)
                {
                    for (i = 0; i < 256; i++)
                    {
                        WriteOneByte(writerTGA, (i * 7) & 0xFF);
                        WriteOneByte(writerTGA, (i * 13) & 0xFF);
                        WriteOneByte(writerTGA, (i * 19) & 0xFF);
                    }
                }
                else
                {
                    for (i = 0; i < 256; i++)
                    {
                        WriteOneByte(writerTGA, (palette[(i * 3) + 2] * 255) / 0x3F);
                        WriteOneByte(writerTGA, (palette[(i * 3) + 1] * 255) / 0x3F);
                        WriteOneByte(writerTGA, (palette[(i * 3)] * 255) / 0x3F);
                    }
                }

                w = resourceRAW.spriteDesc.width * 8;
                h = resourceRAW.spriteDesc.height;
                for (i = 0; i < resourceRAW.spriteDesc.count; ++i)
                {
                    rbase = (resourceRAW.spriteDesc.count - i - 1) * w * h;
                    for (j = 0; j < h; ++j)
                    {
                        writerTGA.Write(resourceRAW.spriteDesc.DecryptedData, rbase + (h - j - 1) * w, w);
                    }
                }

                writerTGA.Close();

                File.WriteAllBytes(filename, memTGA.ToArray());
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
        }


        public static void SaveRESTGAFont(string filename, int i_numpal)
        {
            byte[] palette = new byte[768];
            int i, j, w, h, iCounter = 0;

            LoadColors(ref palette, i_numpal);

            try
            {
                MemoryStream memTGA = new();
                BinaryWriter writerTGA = new(memTGA);

                WriteOneByte(writerTGA, 0);
                WriteOneByte(writerTGA, 1);
                WriteOneByte(writerTGA, 1);

                // Palette
                WriteOneByte(writerTGA, 0);
                WriteOneByte(writerTGA, 0);
                WriteOneByte(writerTGA, 0);
                WriteOneByte(writerTGA, 1);
                WriteOneByte(writerTGA, 24);

                WriteOneByte(writerTGA, 0);
                WriteOneByte(writerTGA, 0);
                WriteOneByte(writerTGA, 0);
                WriteOneByte(writerTGA, 0);

                if (_ONE_DIRECTIVE == 1)
                {
                    if (resourceRAW.fontDesc.width == 16)
                    {
                        writerTGA.Write(
                            (ushort)16);       // width
                        writerTGA.Write(
                            (ushort)((resourceRAW.fontDesc.width * resourceRAW.fontDesc.height) / 2));
                    }
                    else
                    {
                        writerTGA.Write(
                            (ushort)8);       // width
                        writerTGA.Write(
                            (ushort)(resourceRAW.fontDesc.width * resourceRAW.fontDesc.height));
                    }
                }
                else
                {
                    writerTGA.Write(
                        (ushort)(resourceRAW.fontDesc.height));
                    writerTGA.Write(
                        (ushort)(resourceRAW.fontDesc.width * 8));
                }

                WriteOneByte(writerTGA, 8);
                WriteOneByte(writerTGA, 0);

                if (_ONE_DIRECTIVE == 0)
                {
                    for (i = 0; i < 256; i++)
                    {
                        WriteOneByte(writerTGA, (i * 7) & 0xFF);
                        WriteOneByte(writerTGA, (i * 13) & 0xFF);
                        WriteOneByte(writerTGA, (i * 19) & 0xFF);
                    }
                }
                else
                {
                    for (i = 0; i < 256; i++)
                    {
                        WriteOneByte(writerTGA, (palette[(i * 3) + 2] * 255) / 0x3F);
                        WriteOneByte(writerTGA, (palette[(i * 3) + 1] * 255) / 0x3F);
                        WriteOneByte(writerTGA, (palette[(i * 3)] * 255) / 0x3F);
                    }
                }

                w = resourceRAW.fontDesc.width * resourceRAW.fontDesc.height;

                if (resourceRAW.fontDesc.width == 16)
                {
                    w /= 2;
                    h = 16;
                }                    
                else
                {                    
                    h = 8;
                }                    

                resourceRAW.fontDesc.DecryptedData = Reverse(resourceRAW.fontDesc.DecryptedData,
                                                             w,
                                                             h);


                for (j = 0; j < w; j++)
                {
                    for (i = 0; i < h; i++)
                    {
                        writerTGA.Write(resourceRAW.fontDesc.DecryptedData[iCounter++]);
                    }
                }

                writerTGA.Close();

                File.WriteAllBytes(filename, memTGA.ToArray());
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
        }


        public static void SaveRESDecryptedImage(string filename)
        {
            MemoryStream memDec = new();
            BinaryWriter writerDec = new(memDec);

            writerDec.Write(resourceRAW.magic, 0, resourceRAW.magic.Length);
            writerDec.Write(resourceRAW.type);
            writerDec.Write(resourceRAW.size);

            writerDec.Write(resourceRAW.spriteDesc.count);
            writerDec.Write(resourceRAW.spriteDesc.height);
            writerDec.Write(resourceRAW.spriteDesc.width);
            writerDec.Write(resourceRAW.spriteDesc.field_4);
            writerDec.Write((byte)0);
            writerDec.Write(resourceRAW.spriteDesc.field_6);

            writerDec.Write(resourceRAW.spriteDesc.DecryptedData);

            memDec.Close();

            File.WriteAllBytes(filename, memDec.ToArray());
        }

        public static void SaveRESDecryptedFont(string filename)
        {
            MemoryStream memDec = new();
            BinaryWriter writerDec = new(memDec);

            writerDec.Write(resourceRAW.magic, 0, resourceRAW.magic.Length);
            writerDec.Write(resourceRAW.type);
            writerDec.Write(resourceRAW.size);

            writerDec.Write(resourceRAW.fontDesc.tbl_fontswidths);
            writerDec.Write(resourceRAW.fontDesc.height);
            writerDec.Write(resourceRAW.fontDesc.width);
            writerDec.Write(resourceRAW.fontDesc.field_4);
            writerDec.Write(resourceRAW.fontDesc.field_5);
            writerDec.Write((byte)0);
            writerDec.Write(resourceRAW.fontDesc.field_7);

            writerDec.Write(resourceRAW.fontDesc.DecryptedData);

            memDec.Close();

            File.WriteAllBytes(filename, memDec.ToArray());
        }


        public static void ConvertTGA2UndecryptRESImage(string filename, byte[] fileTGA)
        {
            int i, j, k;

            try
            {
                // Work with TGA data
                MemoryStream memTGA = new(fileTGA);
                BinaryReader readerTGA = new(memTGA);

                readerTGA.BaseStream.Position = 0xC;    // width
                resourceRAW.spriteDesc.width = (byte)(readerTGA.ReadUInt16() / 8);
                resourceRAW.spriteDesc.count = (ushort)(readerTGA.ReadUInt16() / resourceRAW.spriteDesc.height);

                // Now the image.
                // The image is inverted when inside the game as a resource.
                // So, if we have:                    12 14 14 12
                //                                    14 12 12 14
                //                                    14 14 14 14
                //
                // The data in resource file is:      14 14 14 14
                //                                    14 12 12 14
                //                                    12 14 14 12

                resourceRAW.spriteDesc.DecryptedData = new byte[resourceRAW.spriteDesc.width *
                                                                resourceRAW.spriteDesc.height *
                                                                resourceRAW.spriteDesc.count * 8];

                // Now let's put the data. It begins normally at 0x2CA
                int imgWidth = resourceRAW.spriteDesc.width * 8;
                k = 0;

                for (i = (resourceRAW.spriteDesc.count * resourceRAW.spriteDesc.height) - 1; i >= 0; i--)
                {
                    readerTGA.BaseStream.Position = 0x312 + (i * imgWidth);

                    for (j = 0; j < imgWidth; j++)
                    {
                        resourceRAW.spriteDesc.DecryptedData[(k * imgWidth) + j] =
                            readerTGA.ReadByte();
                    }

                    k++;
                }

                memTGA.Close();

                // Now save the imported TGA as undecrypted resource (the game can use this)
                //SaveRESDecryptedImage(Path.GetFileNameWithoutExtension(filename) + "_NEW.RAW");

            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
        }


        public static void ConvertTGA2UndecryptRESFont(string filename, byte[] fileTGA)
        {
            int i, j, w, h, iCounter = 0;

            try
            {
                // Work with TGA data
                MemoryStream memTGA = new(fileTGA);
                BinaryReader readerTGA = new(memTGA);

                readerTGA.BaseStream.Position = 0xC;    // width

                // Now the image.
                // The image is inverted when inside the game as a resource.
                // So, if we have:                    12 14 14 12
                //                                    14 12 12 14
                //                                    14 14 14 14
                //
                // The data in resource file is:      14 14 14 14
                //                                    14 12 12 14
                //                                    12 14 14 12

                resourceRAW.fontDesc.DecryptedData = new byte[resourceRAW.fontDesc.width *
                                                              resourceRAW.fontDesc.height * 8];

                // Now let's put the data. It begins normally at 0x2CA
                w = resourceRAW.fontDesc.width * resourceRAW.fontDesc.height;
                h = 8;

                readerTGA.BaseStream.Position = 0x312;

                for (j = 0; j < w; j++)
                {
                    for (i = 0; i < h; i++)
                    {
                        resourceRAW.fontDesc.DecryptedData[iCounter++] = 
                            readerTGA.ReadByte();
                    }
                }

                resourceRAW.fontDesc.DecryptedData = Reverse(resourceRAW.fontDesc.DecryptedData,
                                                             w,
                                                             h);

                memTGA.Close();

                // Now save the imported TGA as undecrypted resource (the game can use this)
                //SaveRESDecryptedFont(Path.GetFileNameWithoutExtension(filename) + "_NEW.RAW");

            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
        }


        public static void ConvertTGA2UndecryptRES(string filename)
        {
            byte[] fileTGA, fileRAW;

            int i, j, k;

            try
            {
                // First read all the file
                fileTGA = File.ReadAllBytes(filename);
                fileRAW = File.ReadAllBytes(Path.GetFileNameWithoutExtension(filename) + ".RAW");

                // Work with .RAW file data
                MemoryStream memRAW = new(fileRAW);
                BinaryReader readerRAW = new(memRAW);

                // Let's create the resource header
                resourceRAW.magic = "EH"u8.ToArray();
                readerRAW.BaseStream.Position = 0x2;
                resourceRAW.type = readerRAW.ReadByte();
                resourceRAW.size = readerRAW.ReadUInt16();

                if (resourceRAW.type == 2)
                {
                    // Let's create the font header
                    resourceRAW.fontDesc.tbl_fontswidths = new byte[128];
                    resourceRAW.fontDesc.tbl_fontswidths = readerRAW.ReadBytes(128);
                    resourceRAW.fontDesc.height = readerRAW.ReadUInt16();
                    resourceRAW.fontDesc.width = readerRAW.ReadUInt16();
                    resourceRAW.fontDesc.field_4 = readerRAW.ReadByte();
                    resourceRAW.fontDesc.field_5 = readerRAW.ReadByte();
                    resourceRAW.fontDesc.compressionAlgo = 0;
                    resourceRAW.fontDesc.field_7 = readerRAW.ReadByte();

                    // Enough of RAW file
                    memRAW.Close();

                    ConvertTGA2UndecryptRESFont(filename, fileTGA);

                    // Now save the imported TGA as undecrypted resource (the game can use this)
                    SaveRESDecryptedFont(Path.GetFileNameWithoutExtension(filename) + "_NEW.RAW");
                }
                else
                {
                    // Let's create the sprite header
                    readerRAW.BaseStream.Position = 0x7;
                    resourceRAW.spriteDesc.height = readerRAW.ReadUInt16(); ;
                    readerRAW.ReadByte();
                    resourceRAW.spriteDesc.field_4 = readerRAW.ReadByte();
                    resourceRAW.spriteDesc.compressionAlgo = 0;
                    readerRAW.ReadByte();
                    resourceRAW.spriteDesc.field_6 = readerRAW.ReadByte();

                    // Enough of RAW file
                    memRAW.Close();

                    ConvertTGA2UndecryptRESImage(filename, fileTGA);

                    // Now save the imported TGA as undecrypted resource (the game can use this)
                    SaveRESDecryptedImage(Path.GetFileNameWithoutExtension(filename) + "_NEW.RAW");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine ("Exception: " + e.Message);
            }
        }


        public static void ExtractRESImage(BinaryReader readerRES, int i_numres, 
                                           int i_numpal, bool extractdecrypt)
        {
            int outputSize;

            Debug.Assert(resourceRAW.size == 8);

            // Read header of sprite
            resourceRAW.spriteDesc.count = readerRES.ReadUInt16();
            resourceRAW.spriteDesc.height = readerRES.ReadUInt16();
            resourceRAW.spriteDesc.width = readerRES.ReadByte();
            resourceRAW.spriteDesc.field_4 = readerRES.ReadByte();
            resourceRAW.spriteDesc.compressionAlgo = readerRES.ReadByte();
            resourceRAW.spriteDesc.field_6 = readerRES.ReadByte();

            // Read RAW Data
            resourceRAW.spriteDesc.RAWData = readerRES.ReadBytes(1000000);

            Console.WriteLine("    count           : " + resourceRAW.spriteDesc.count.ToString());
            Console.WriteLine("    height          : " + resourceRAW.spriteDesc.height.ToString());
            Console.WriteLine("    width           : " + resourceRAW.spriteDesc.width.ToString() +
                              "(*8) = " + (resourceRAW.spriteDesc.width * 8).ToString());
            Console.WriteLine("    field_4         : " + resourceRAW.spriteDesc.field_4.ToString());
            Console.WriteLine("    compressionAlgo : " + resourceRAW.spriteDesc.compressionAlgo.ToString());
            Console.WriteLine("    field_6         : " + resourceRAW.spriteDesc.field_6.ToString());

            outputSize = resourceRAW.spriteDesc.width * resourceRAW.spriteDesc.height * 8;
            Console.WriteLine("\n    output size     : " + outputSize.ToString());

            resourceRAW.spriteDesc.DecryptedData = new byte[outputSize * resourceRAW.spriteDesc.count];

            switch (resourceRAW.spriteDesc.compressionAlgo)
            {
                case 0:
                    {

                        Array.Copy(resourceRAW.spriteDesc.RAWData,
                                   resourceRAW.spriteDesc.DecryptedData,
                                   resourceRAW.spriteDesc.RAWData.Length);

                        //Extract0(outputSize);
                        //Console.WriteLine("Exporting " + i_numres.ToString("0000") + ".TGA");

                        // Export image
                        SaveRESTGAImage(i_numres.ToString("0000") + ".TGA", i_numpal);
                        break;

                    }
                case 1:
                    {

                        Decompress1(outputSize);
                        Console.WriteLine("Exporting " + i_numres.ToString("0000") + ".TGA");

                        //dumpTga(filename, spriteDesc, output);
                        break;

                    }
                case 3:
                    {

                        Decompress3Image(outputSize);
                        Console.WriteLine("Exporting " + i_numres.ToString("0000") + ".TGA");

                        // Export image
                        SaveRESTGAImage(i_numres.ToString("0000") + ".TGA", i_numpal);

                        // Export decrypted
                        if (extractdecrypt)
                            SaveRESDecryptedImage(i_numres.ToString("0000") + ".DEC");
                        break;

                    }
                default:
                    {
                        Console.WriteLine("Unsupported sprite compression method!");
                        break;
                    }
            }
        }

        public static void ExtractRESFont(BinaryReader readerRES, int i_numres, int i_numpal, bool extractdecrypt)
        {
            int outputSize;

            //Debug.Assert(resourceRAW.size == 88);

            // Read header of sprite
            resourceRAW.fontDesc.tbl_fontswidths = readerRES.ReadBytes(128);
            resourceRAW.fontDesc.height = readerRES.ReadUInt16();
            resourceRAW.fontDesc.width = readerRES.ReadUInt16();
            resourceRAW.fontDesc.field_4 = readerRES.ReadByte();
            resourceRAW.fontDesc.field_5 = readerRES.ReadByte();
            resourceRAW.fontDesc.compressionAlgo = readerRES.ReadByte();
            resourceRAW.fontDesc.field_7 = readerRES.ReadByte();
            //resourceRAW.fontDesc.field_8 = readerRES.ReadByte();

            // Read RAW Data
            resourceRAW.fontDesc.RAWData = readerRES.ReadBytes(1000000);

            Console.WriteLine("    table_fonts_w   : " + resourceRAW.fontDesc.tbl_fontswidths.Length.ToString());
            Console.WriteLine("    height          : " + resourceRAW.fontDesc.height.ToString());
            Console.WriteLine("    width           : " + resourceRAW.fontDesc.width.ToString() +
                              "(*8) = " + (resourceRAW.fontDesc.width * 8).ToString());
            Console.WriteLine("    field_4         : " + resourceRAW.fontDesc.field_4.ToString());
            Console.WriteLine("    field_5         : " + resourceRAW.fontDesc.field_5.ToString());
            Console.WriteLine("    compressionAlgo : " + resourceRAW.fontDesc.compressionAlgo.ToString());
            Console.WriteLine("    field_7         : " + resourceRAW.fontDesc.field_7.ToString());

            outputSize = resourceRAW.fontDesc.width * resourceRAW.fontDesc.height * 8;
            Console.WriteLine("\n    output size     : " + outputSize.ToString());

            resourceRAW.fontDesc.DecryptedData = new byte[outputSize];

            switch (resourceRAW.fontDesc.compressionAlgo)
            {
                case 0:
                    {

                        Array.Copy(resourceRAW.fontDesc.RAWData,
                                   resourceRAW.fontDesc.DecryptedData,
                                   resourceRAW.fontDesc.RAWData.Length);

                        // Export font
                        SaveRESTGAFont(i_numres.ToString("0000") + ".TGA", i_numpal);
                        break;

                    }
                case 1:
                    {

                        Decompress1(outputSize);
                        Console.WriteLine("Exporting " + i_numres.ToString("0000") + ".TGA");

                        //dumpTga(filename, spriteDesc, output);
                        break;

                    }
                case 3:
                    {

                        Decompress3Font(outputSize);
                        Console.WriteLine("Exporting " + i_numres.ToString("0000") + ".TGA");

                        // Export font
                        SaveRESTGAFont(i_numres.ToString("0000") + ".TGA", i_numpal);

                        // Export decrypted
                        if (extractdecrypt)
                        {
                            SaveRESDecryptedFont(i_numres.ToString("0000") + ".DEC");
                        }

                        break;

                    }
                default:
                    {
                        Console.WriteLine("Unsupported sprite compression method!");
                        break;
                    }
            }
        }


        public static void ConvertDecryptRESNum(int i_numres, int i_numpal, bool extractdecrypt)
        {
            try
            {
                // Prepare memory reader
                MemoryStream memRES = new([.. Program.res.list_entries[i_numres]]);
                BinaryReader readerRES = new(memRES);

                // Read header of resource
                resourceRAW.magic = readerRES.ReadBytes(2);
                resourceRAW.type = readerRES.ReadByte();
                resourceRAW.size = readerRES.ReadUInt16();

                switch (resourceRAW.type)
                {
                    case 1:
                        ExtractRESImage(readerRES, i_numres, i_numpal, extractdecrypt);
                        break;

                    case 2:         // Font
                        ExtractRESFont(readerRES, i_numres, i_numpal, extractdecrypt);
                        break;

                    default:
                        break;
                }

                memRES.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }

        }


        public static void ConvertDecryptRESFile(string filename, int i_numpal, bool extractdecrypt)
        {
            byte[] fileInput;

            try
            {
                int i_numres = Int32.Parse(Path.GetFileNameWithoutExtension(filename));

                // Load file
                fileInput = File.ReadAllBytes(filename);

                // Prepare memory reader
                MemoryStream memRES = new(fileInput);
                BinaryReader readerRES = new(memRES);

                // Read header of resource
                resourceRAW.magic = readerRES.ReadBytes(2);
                resourceRAW.type = readerRES.ReadByte();
                resourceRAW.size = readerRES.ReadUInt16();

                if (resourceRAW.magic[0] == 'E' && resourceRAW.magic[1] == 'H')
                {
                    switch (resourceRAW.type)
                    {
                        case 1:         // Image normally
                            ExtractRESImage(readerRES, i_numres, i_numpal, extractdecrypt);
                            break;

                        case 2:         // Font
                            ExtractRESFont(readerRES, i_numres, i_numpal, extractdecrypt);
                            break;

                        default:
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("The input file for decrypt is not an exported resource.");
                }

                memRES.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
        }

    }
}
