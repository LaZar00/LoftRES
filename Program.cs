using System.Globalization;
using System.Numerics;
using System.Text;
using System.Xml.Linq;
using System.Diagnostics;

namespace LoftRES
{
    class Program
    {

        public const int DEFAULT_PALETTE = 12;

        public struct RES
        {
            public int num_entries;
            public int[] offsets_entries;
            public List<byte[]> list_entries;
        }

        public static RES res = new();
        public static string[] fileRESTXT = [];


        static void PrintHelp()
        {
            Console.WriteLine("Ravenloft: Strahd's Possesions, Ravenloft: Stone Prophet");
            Console.WriteLine("and Menzoberranzan... RES Exporter/Generator v1.1");
            Console.WriteLine("=========================================================");
            Console.WriteLine("");
            Console.WriteLine("Usage:");
            Console.WriteLine("");
            Console.WriteLine("  loftres <option> <filename>");
            Console.WriteLine("");
            Console.WriteLine("Options:");
            Console.WriteLine("  -e/-E: extract RES?? files and RES.TXT");
            Console.WriteLine("         C:\\loftres -e RES0");
            Console.WriteLine("  -g/-G: generate RES??_NEW from RES.TXT and files");
            Console.WriteLine("         C:\\loftres -g RESTEST");
            Console.WriteLine("  -d/-D: decrypt extracted .RAW file into .TGA");
            Console.WriteLine("  -y/-Y: decrypt extracted .RAW file into .TGA AND decrypted .DEC file");
            Console.WriteLine("         C:\\loftres -d 0000.RAW");
            Console.WriteLine("  -d/-D/-y/-Y: decrypt extracted .RAW file with PALETTE (MAX: 14)");
            Console.WriteLine("         C:\\loftres -d 0000.RAW:12");
            Console.WriteLine("  -r/-R: convert .TGA file into .RAW (uncompressed) compatible file");
            Console.WriteLine("");
        }


        static void ReadRES(string filename)
        {
            byte[] resInput, tmpEntry;
            int i, size;

            // Let's read the binary RES?? file
            resInput = File.ReadAllBytes(filename);

            // Let's put this in the table
            MemoryStream fileRES = new(resInput);
            BinaryReader readRES = new(fileRES);

            res.num_entries = readRES.ReadInt32();

            res.offsets_entries = new int[res.num_entries];

            for (i = 0; i < res.num_entries; i++)
            {
                res.offsets_entries[i] = readRES.ReadInt32();
            }

            res.list_entries = [];

            for (i = 0; i < res.num_entries; i++)
            {
                if (i < res.num_entries - 1)
                {
                    size = res.offsets_entries[i + 1] - res.offsets_entries[i];
                }
                else
                {
                    size = (int)readRES.BaseStream.Length - res.offsets_entries[i];
                }

                tmpEntry = readRES.ReadBytes(size);
                res.list_entries.Add(tmpEntry);
            }

            fileRES.Close();
        }


        static void ExtractRES()
        {
            int i;

            try
            {
                FileStream fileRESTXT = File.Open("RES.TXT", FileMode.Create);
                StreamWriter writeRESTXT = new(fileRESTXT, Encoding.GetEncoding(1252));

                for (i = 0; i < res.num_entries; i++)
                {
                    writeRESTXT.WriteLine(i.ToString("0000") + ".RAW");

                    File.WriteAllBytes(i.ToString("0000") + ".RAW", [.. res.list_entries[i].ToArray()]);
                }

                writeRESTXT.Close();
            }
            catch (Exception e)
            {
                Console.Write(e.ToString());
            }
        }


        static void PrepareRES()
        {
            byte[] fileRES;
            int i;

            // Let's read the RES.TXT file
            fileRESTXT = File.ReadAllLines("RES.TXT");
            res.list_entries = [];

            foreach (string line in fileRESTXT)
            {
                fileRES = File.ReadAllBytes(line);

                res.list_entries.Add(fileRES);
            }

            res.num_entries = res.list_entries.Count;
            res.offsets_entries = new int[res.num_entries];

            res.offsets_entries[0] = 4 + (res.num_entries * 4);     //  4 for num_entires in header

            for (i = 1; i < res.num_entries; i++)
            {
                res.offsets_entries[i] = res.offsets_entries[i - 1] + res.list_entries[i - 1].Length;
            }

        }

        static void GenerateRES(string filename)
        {
            int i;

            MemoryStream memRES = new();
            BinaryWriter writeRES = new(memRES);

            writeRES.Write(res.num_entries);

            for (i = 0; i < res.num_entries; i++)
            {
                writeRES.Write(res.offsets_entries[i]);
            }

            for (i = 0; i < res.num_entries; i++)
            {
                writeRES.Write(res.list_entries[i]);
            }

            memRES.Close();

            File.WriteAllBytes(filename, [.. memRES.ToArray()]);

        }


        static void Main(string[] args)
        {

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            if (args.Length == 2)
            {
                switch (args[0].ToLower())
                {
                    case "-d":
                    case "-y":
                        string[] argsInput = args[1].Split(':');

                        if (File.Exists(argsInput[0]))
                        {
                            if (Path.GetExtension(argsInput[0]).ToUpper() == ".RAW")
                            {
                                if (argsInput.Length > 1)
                                {
                                    if (Int32.TryParse(argsInput[1], out int i_numpal))
                                    {
                                            if (args[0].ToLower() == "-d")
                                                DecryptRES.ConvertDecryptRESFile(argsInput[0], i_numpal, false);
                                            else
                                                DecryptRES.ConvertDecryptRESFile(argsInput[0], i_numpal, true);                                            
                                    }
                                    else
                                    {
                                        Console.WriteLine("The palette set is not correct.");
                                    }
                                }
                                else
                                {
                                    if (args[0].ToLower() == "-d")
                                        DecryptRES.ConvertDecryptRESFile(argsInput[0], DEFAULT_PALETTE, false);
                                    else
                                        DecryptRES.ConvertDecryptRESFile(argsInput[0], DEFAULT_PALETTE, true);
                                }

                                break;
                            }
                            else
                            {
                                Console.WriteLine("The file must have .RAW extension.\n");
                                PrintHelp();
                            }
                        }
                        else
                        {
                            Console.WriteLine("The file " + args[1] + " does not exist.\n");
                            PrintHelp();
                        }
                        break;

                    case "-r":
                        if (File.Exists(args[1]) && Path.GetExtension(args[1]).ToUpper() == ".TGA")
                        {
                            if (File.Exists(Path.GetFileNameWithoutExtension(args[1]) + ".RAW"))
                                DecryptRES.ConvertTGA2UndecryptRES(args[1]);
                            else
                            {
                                Console.WriteLine("You need also the original " + Path.GetFileNameWithoutExtension(args[1]) + ".RAW file.\n");
                                PrintHelp();
                            }
                        }
                        else
                        {
                            Console.WriteLine("The file " + args[1] + " does not exist.\n");
                            PrintHelp();
                        }

                        break;

                    case "-e":
                        if (Path.GetFileNameWithoutExtension(args[1]).ToUpper()[0] == 'R' &&
                            Path.GetFileNameWithoutExtension(args[1]).ToUpper()[1] == 'E' &&
                            Path.GetFileNameWithoutExtension(args[1]).ToUpper()[2] == 'S')
                        {
                            if (File.Exists(args[1]))
                            {
                                ReadRES(Path.GetFileNameWithoutExtension(args[1]));
                                ExtractRES();
                            }
                            else
                            {
                                Console.WriteLine("The file " + args[1] + " does not exist.\n");
                                PrintHelp();
                            }
                        }
                        else
                        {
                            Console.WriteLine("The file " + args[1] + " does not seems a resource RES?? file.\n");
                            PrintHelp();
                        }

                        break;

                    case "-g":
                        if (File.Exists("RES.TXT"))
                        {
                            PrepareRES();
                            GenerateRES(Path.GetFileNameWithoutExtension(args[1]) + "_NEW");
                        }
                        else
                        {
                            Console.WriteLine("You must have a RES.TXT file to do the resource RES file generation.\n");
                            PrintHelp();
                            return;
                        }
                        break;

                    default:
                        PrintHelp();
                        break;
                }
            }
            else
            {
                PrintHelp();
                return;
            }
        }
    }
}
