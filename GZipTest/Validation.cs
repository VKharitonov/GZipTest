using System;
using System.IO;
using System.Text;


namespace GZipTest
{
    public static class Validation
    {
        public static void Check(string[] args)
        {

            if (args.Length == 0 || args.Length > 3)
            {
                throw new Exception("Please follow this pattern:\n GZipTest.exe compress(decompress) [Source file] [Destination file].");
            }

            if (args[0].ToLower() != "compress" && args[0].ToLower() != "decompress")
            {
                throw new Exception("First argument must be \"compress\" or \"decompress\".");
            }

            if (args[1].Length == 0)
            {
                throw new Exception("No source file name was specified.");
            }

            if (args[2].Length == 0)
            {
                throw new Exception("No destination file name was specified.");
            }

            if (!File.Exists(args[1]))
            {
                throw new Exception("No source file was found.");
            }


            FileInfo _fileIn = new FileInfo(args[1]);
            FileInfo _fileOut = new FileInfo(args[2]);

            if (args[1] == args[2])
            {
                throw new Exception("Source and destination files shall be different.");
            }
            if (_fileOut.Exists)
            {
                throw new Exception("Destination file already exists. Please use another file name.");
            }

            if (args[0] == "compress" && _fileIn.Extension == ".gz")
            {
                throw new Exception("File has already been compressed.");
            }


            if (_fileIn.Extension != ".gz" && args[0] == "decompress")
            {
                throw new Exception("File to be decompressed shall have .gz extension.");
            }

        }
    }
}
