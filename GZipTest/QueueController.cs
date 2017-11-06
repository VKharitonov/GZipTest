using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace GZipTest
{
   
    public class QueueController
    {
        private object locker = new object();
        Queue<blockData> queue = new Queue<blockData>();
        public bool isFin = false;
        private int blockId = 0;
//Здесь присваивается id новым блокам. Поскольку читается файл для сжатия одним потоком, то проблем с нарушением последовательности здесь не возникает.
        public void EnqueueForCompressing(byte[] block)
        {
            lock (locker)
            {
                if (isFin)
                    throw new InvalidOperationException("Queue already stopped");

                blockData _block = new blockData(blockId, block);
                queue.Enqueue(_block);
                blockId++;
                Monitor.PulseAll(locker);
            }
        }

        public void EnqueueForWriting(blockData _block)
        {
            //А этот метод применяется, когда важна последовательность. Перед записью в выходной файл, в основном.
            int id = _block.ID;
            lock (locker)
            {
                if (isFin)
                    throw new InvalidOperationException("Adding new elements in stopped queue is impossible");

                while (id != blockId)
                {
                    Monitor.Wait(locker);
                }

                queue.Enqueue(_block);
                blockId++;
                Monitor.PulseAll(locker);
            }
        }

        public blockData Dequeue()
        {
            lock (locker)
            {
                
                if (queue.Count == 0)
                    return null;
                //Console.WriteLine("Elements: " + (queue.Count - 1));
                return queue.Dequeue();

            }
        }
       public int RetCount()
        {
           lock(locker)
           {
               return queue.Count;
           }
        }
        public void Stop()
        {
            lock (locker)
            {
                isFin = true;
                Monitor.PulseAll(locker);
            }
        }
    }
}
