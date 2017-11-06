using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace GZipTest
{
    class Decompressor:GZ
    {
        int blockCounter = 0;
        public Decompressor(string input, string output)
            : base(input, output)
        {
        }
            public override void Launch()
        {
                try
                {
                    Console.WriteLine("Decompressing this. Keep patience, please");
            Thread _reader = new Thread(new ThreadStart(Read));
            _reader.Name = "ReaderThread";
            _reader.Start();
            for (int i = 0; i<_threadNum-1;i++)
            {
                tPool[i] = new Thread(new ThreadStart(DecompressAndWrite));
                tPool[i].Name = "Thread " + i;
                tPool[i].Start();
            }

            _reader.Join();
            Console.WriteLine("All read");
            if (!isSucces())
            {
                tPool[_threadNum - 1] = new Thread(new ThreadStart(DecompressAndWrite));
                tPool[_threadNum - 1].Name = "Thread AfterRead";
                tPool[_threadNum - 1].Start();
                Console.WriteLine("AfterRead created");
            }
            foreach (Thread t in tPool)
            {
                if (t!=null)
                {
                    t.Join();
                    
                }
            }

            if (!_cancelled)
                {
                    Console.WriteLine("Decompression complete!");
                }
                else
                {
                    Console.WriteLine("Decompression failed!");
                }
            Console.WriteLine("Press any key or Enter to close programm");
            }
            
        catch (Exception ex)
            {
                Console.WriteLine("Error in thread {0}. \n Error description: {1}", Thread.CurrentThread.Name, ex.Message);
                _cancelled = true;
            }   
        }

        private void DecompressAndWrite()
        {
            try
            {

                while ((!_success) && (!_cancelled))
                {

                    if (_queueWrite.RetCount() > 0)
                    {
                        lock (lockerW)
                        {
                            Write();
                            Console.WriteLine("...");
                            Monitor.PulseAll(lockerW);
                        }
                    }
                    else if ((_queueReader.RetCount() > 0) && (_queueWrite.RetCount() < maxBlock))
                    {
                        Decompress(Thread.CurrentThread.Name);
                    }
                    else
                    {
                        isSucces();
                    }

                }
                Console.WriteLine(Thread.CurrentThread.Name + "finished");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in thread {0}. \n Error description: {1}", Thread.CurrentThread.Name, ex.Message);
                _cancelled = true;
            }   
        }

        private void Read()
        {
            try
            {
                using (FileStream _compressedFile = new FileStream(inFile, FileMode.Open))
                {
                    while (_compressedFile.Position < _compressedFile.Length)
                    {
                        //Чтобы избежать нехватки памяти используется ограничение на максимальное количество блоков в очереди. По умолчанию оно представлено константой, значение которой можно изменить в коде класса GZ. 
                        //При необходимости можно применить и System.Environment.SystemPageSize или предоставить пользователю определять какое количество оперативной памяти он готов выделить программе
                        //Но в ТЗ подобной задачи не ставилось, поэтому была использована константа.
                        if (_queueReader.RetCount() <= maxBlock)
                        {
                            byte[] lengthOfBlock = new byte[8];
                            _compressedFile.Read(lengthOfBlock, 0, lengthOfBlock.Length);
                            int blockLength = BitConverter.ToInt32(lengthOfBlock, 4);
                            byte[] compressedData = new byte[blockLength];
                            lengthOfBlock.CopyTo(compressedData, 0);

                            _compressedFile.Read(compressedData, 8, blockLength - 8);
                            int _dataSize = BitConverter.ToInt32(compressedData, blockLength - 4);
                            byte[] lastBlock = new byte[_dataSize];

                            blockData _block = new blockData(blockCounter, lastBlock, compressedData);
                            _queueReader.EnqueueForWriting(_block);
                            blockCounter++;

                        }

                    }
                    _queueReader.Stop();
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine("Error in thread {0}. \n Error description: {1}", Thread.CurrentThread.Name, ex.Message);
                _cancelled = true;
            }   
        }

        private void Decompress(object i)
        {
            try
            {
                
                    blockData _block = _queueReader.Dequeue();
                    if (_block == null)
                        return;

                    using (MemoryStream ms = new MemoryStream(_block.CompressedBlock))
                    {
                        using (GZipStream _gz = new GZipStream(ms, CompressionMode.Decompress))
                        {
                            _gz.Read(_block.Block, 0, _block.Block.Length);
                            byte[] decompressedData = new byte[_block.Block.Length];
                            _block.Block.CopyTo(decompressedData, 0);
                            blockData block = new blockData(_block.ID, decompressedData);
                            _queueWrite.EnqueueForWriting(block);
                        }
                    }
                
            }

            catch (Exception ex)
            {
                Console.WriteLine("Error in thread {0}. \n Error description: {1}", Thread.CurrentThread.Name, ex.Message);
                _cancelled = true;
            }   
        }

        private void Write()
        {
            try
            {
                using (FileStream _decompressedFile = new FileStream(outFile, FileMode.Append))
                {

                        blockData _block = _queueWrite.Dequeue();
                        if (_block == null)
                            return;

                        _decompressedFile.Write(_block.Block, 0, _block.Block.Length);
                    
                }
            }

            
            catch (Exception ex)
            {
                Console.WriteLine("Error in thread {0}. \n Error description: {1}", Thread.CurrentThread.Name, ex.Message);
                _cancelled = true;
            }   
        }
    }
}
