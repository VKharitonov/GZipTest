using System;
using System.Threading;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace GZipTest
{
    class GZ
    {
        public void Realize(IGZ intgz)
        {
            intgz.Realize();
        }
    }
    
    
    interface IGZ
    {
        void Realize();
    }

    class Compressor : IGZ
    {
        private string inName;
        private string outName;
        static int tNum = Environment.ProcessorCount;
        static byte[][] block = new byte[tNum][];
        static byte[][] cBlock = new byte[tNum][];
        static int blockSize = 10485760;

        public Compressor(string InName, string OutName)
        {
            inName = InName;
            outName = OutName;
        }
        public void Realize()
        {
            try
            {

                FileStream inFs = new FileStream(inName, FileMode.Open);
                FileStream outFs = new FileStream(outName, FileMode.Append);

                int _blockSize;
                Thread[] tArr;
                Console.WriteLine("Compression started:");
                while (inFs.Position < inFs.Length)
                {
                    Console.WriteLine("completed " + inFs.Position + " from " + inFs.Length);
                    tArr = new Thread[tNum];
                    for (int blockCount = 0; blockCount < tNum && inFs.Position < inFs.Length; blockCount++)
                    {
                        //Читаем блок файла и сжимаем его в потоке. Количество потоков зависит от количества процессоров.
                        if (inFs.Length - inFs.Position > blockSize)
                        {
                            _blockSize = blockSize;
                        }
                        else
                        {
                            _blockSize = (int)(inFs.Length - inFs.Position);
                        }

                        block[blockCount] = new byte[_blockSize];
                        inFs.Read(block[blockCount], 0, _blockSize);

                        tArr[blockCount] = new Thread(CompressionBlock);
                        tArr[blockCount].Start(blockCount);
                    }
                    foreach (Thread t in tArr)
                    {
                        //Ждем, пока все потоки сожмут свои части, чтобы избежать записи блоков в неверном порядке.
                        if (t != null)
                        {
                            t.Join();
                        }
                    }

                    for (int bCount = 0; (bCount < tNum) && (tArr[bCount] != null); )
                    {
                        //Записываем блоки, добавив данные о их размере
                        BitConverter.GetBytes(cBlock[bCount].Length).CopyTo(cBlock[bCount], 4);
                        outFs.Write(cBlock[bCount], 0, cBlock[bCount].Length);
                        bCount++;

                    }
                }
                outFs.Close();
                inFs.Close();
                Console.WriteLine("Compression completed. Press any key or Enter to exit");
                Console.Read();
            }
            catch(Exception ex)
            {
                Console.WriteLine("ERROR:" + ex.Message);
                Console.WriteLine("Press any key or enter to exit");
                Console.Read();
            }

        }
        public static void CompressionBlock(object num)
        {
            using (MemoryStream output = new MemoryStream(block[(int)num].Length))
            {
                using (GZipStream compress = new GZipStream(output, CompressionMode.Compress))
                {
                    compress.Write(block[(int)num], 0, block[(int)num].Length);
                }
                cBlock[(int)num] = output.ToArray();
            }
        }

        }

    class Decompressor:IGZ
    {
        private string inName;
        private string outName;
        static int tNum = Environment.ProcessorCount;
        static byte[][] block = new byte[tNum][];
        static byte[][] cBlock = new byte[tNum][];

         public Decompressor(string InName, string OutName)
        {
            inName = InName;
            outName = OutName;
        }
        public void Realize()
         {
            try
            {
                FileStream inFs = new FileStream(inName, FileMode.Open);
                FileStream outFs = new FileStream(outName, FileMode.Append);
                int _blockSize;
                int compressedBlockLength;
                Thread[] tArr;
                Console.WriteLine("Decompressing started");
                byte[] buffer = new byte[8];


                while (inFs.Position < inFs.Length)
                {
                    tArr = new Thread[tNum];
                    for (int blockCount = 0;
                         (blockCount < tNum) && (inFs.Position < inFs.Length);
                         blockCount++)
                    {
                        //Определяем размеры заархивированного блока и передаем его на распаковку.
                        inFs.Read(buffer, 0, 8);
                        compressedBlockLength = BitConverter.ToInt32(buffer, 4);
                        cBlock[blockCount] = new byte[compressedBlockLength];
                        buffer.CopyTo(cBlock[blockCount], 0);

                        inFs.Read(cBlock[blockCount], 8, compressedBlockLength - 8);
                        _blockSize = BitConverter.ToInt32(cBlock[blockCount], compressedBlockLength - 4);
                        block[blockCount] = new byte[_blockSize];

                        tArr[blockCount] = new Thread(DecompressionBlock);
                        tArr[blockCount].Start(blockCount);
                        
                       

                    }
                    foreach (Thread t in tArr)
                    {
                        if (t != null)
                        {
                            t.Join();
                        }
                    }
                    for (int portionCount = 0; (portionCount < tNum) && (tArr[portionCount] != null); )
                    {

                            outFs.Write(block[portionCount], 0, block[portionCount].Length);
                            portionCount++;
                        
                    }
                }

                outFs.Close();
                inFs.Close();
                Console.WriteLine("Decompression completed. Press any key or Enter to exit");
                Console.Read();
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR:" + ex.Message);
                Console.WriteLine("Press any key or enter to exit");
                Console.Read();
            }
        }

        public static void DecompressionBlock(object num)
        {
            using (MemoryStream input = new MemoryStream(cBlock[(int)num]))
            {

                using (GZipStream ds = new GZipStream(input, CompressionMode.Decompress))
                {
                    ds.Read(block[(int)num], 0, block[(int)num].Length);
                }
                
            }
        }
    }

    }

