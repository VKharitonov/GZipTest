using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace GZipTest
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                //Проверяем полученные из командной строки аргументы, если все в порядке - переходим к сжатию/расжатию файла
                Validation.Check(args);
                GZ gzip = new GZ();
                switch (args[0].ToLower())
                {
                    case "compress":
                        gzip.Realize(new Compressor(args[1], args[2]));
                        break;
                    case "decompress":
                        gzip.Realize(new Decompressor(args[1], args[2]));
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error is occured!\n Method: {0}\n Error description: {1}", ex.TargetSite, ex.Message+"\n");
                Console.WriteLine("This description haven`t enough info to solve the problem? Please contact with developer using this e-mail: vladlozovsky@yandex.ru and tell about this error \n");
                Console.WriteLine("Press any key or Enter to close programm");
                Console.Read();
            }

        }
    }
}
