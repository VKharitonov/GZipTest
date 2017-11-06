using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace GZipTest
{
    class Compressor:GZ
    {
        public Compressor(string input, string output)
            : base(input, output)
        {

        }
        public override void Launch()
        {
            try
            {
                Console.WriteLine("Compressing this. Keep patience, please");
//Первый поток будет занят чтением файла. Остальные будут сжимать и записывать в выходной файл поступающие от него блоки
                Thread _reader = new Thread(new ThreadStart(Read));
                _reader.Name = "ReaderThread";
                _reader.Start();
                for (int i = 0; i < _threadNum - 1; i++)
                {
                    tPool[i] = new Thread(new ThreadStart(CompressAndWrite));
                    tPool[i].Name = "Thread " + i;
                    tPool[i].Start();
                }
//Ждем, пока весь файл будет прочитан, после чего создаем еще один поток на сжатие и запись
                _reader.Join();
                Console.WriteLine("All read");
                if (!isSucces())
                {
                    tPool[_threadNum - 1] = new Thread(new ThreadStart(CompressAndWrite));
                    tPool[_threadNum - 1].Name = "Thread AfterRead";
                    tPool[_threadNum - 1].Start();
                    Console.WriteLine("AfterRead created");
                }
//А теперь остается только ждать, пока сжатие будет окончательно завершено. 
                foreach (Thread t in tPool)
                {
                    if (t != null)
                    {
                        t.Join();

                    }
                }
                if (!_cancelled)
                {
                    Console.WriteLine("Compression complete!");
                }
                else
                {
                    Console.WriteLine("Compression failed!");
                }
                Console.WriteLine("Press any key or Enter to close programm");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in thread {0}. \n Error description: {1}", Thread.CurrentThread.Name, ex.Message);
                _cancelled = true;
            }   

        }

        private void CompressAndWrite()
        {
            try
            {
//Если у нас есть что можно записать в файл - блокируем и пишем. Иначе, если есть что сжать - запускается сжатие. Если вообще ничего нет - проверяется, не закончилось ли вообще сжатие.
            while ((!_success)&&(!_cancelled))
            {
                
                if (_queueWrite.RetCount() > 0)
                {
                    lock(lockerW)
                    {
                        Write();
                        Console.WriteLine("...");                     
                        Monitor.PulseAll(lockerW);
                    }
                }
                else if (_queueReader.RetCount() > 0)
                {
                    Compress(Thread.CurrentThread.Name);
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

                using (FileStream _fileToBeCompressed = new FileStream(inFile, FileMode.Open))
                {

                    int bytesRead;
                    byte[] lastBlock;
                    
                    
                    while (_fileToBeCompressed.Position < _fileToBeCompressed.Length && !_cancelled)
                    {
//Чтобы избежать нехватки памяти используется ограничение на максимальное количество блоков в очереди. По умолчанию оно представлено константой, значение которой можно изменить в коде класса GZ. 
//При необходимости можно применить и System.Environment.SystemPageSize или предоставить пользователю определять какое количество оперативной памяти он готов выделить программе
//Но в ТЗ подобной задачи не ставилось, поэтому была использована константа.
                        if(_queueReader.RetCount()<=maxBlock)
                        { 
                          if (_fileToBeCompressed.Length - _fileToBeCompressed.Position <= blockSize)
                              {
                                bytesRead = (int)(_fileToBeCompressed.Length - _fileToBeCompressed.Position);
                              }

                          else
                              {
                                bytesRead = blockSize;
                              }

                        lastBlock = new byte[bytesRead];
                        _fileToBeCompressed.Read(lastBlock, 0, bytesRead);
                        _queueReader.EnqueueForCompressing(lastBlock);
                        
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

        private void Compress(object i)
        {
            try
            {
                
                    blockData _block = _queueReader.Dequeue();

                    if (_block == null)
                        return;

                    using (MemoryStream _memoryStream = new MemoryStream())
                    {
                        using (GZipStream cs = new GZipStream(_memoryStream, CompressionMode.Compress))
                        {

                            cs.Write(_block.Block, 0, _block.Block.Length);
                        }


                        byte[] compressedData = _memoryStream.ToArray();
                        blockData _out = new blockData(_block.ID, compressedData);
                        _queueWrite.EnqueueForWriting(_out);
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
                using (FileStream _fileCompressed = new FileStream(outFile, FileMode.Append))
                {
                    
                        blockData _block = _queueWrite.Dequeue();
                        if (_block == null)
                            return;

                        BitConverter.GetBytes(_block.Block.Length).CopyTo(_block.Block, 4);
                        _fileCompressed.Write(_block.Block, 0, _block.Block.Length);
                    
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
