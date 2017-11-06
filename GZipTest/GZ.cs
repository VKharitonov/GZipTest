using System;
using System.Threading;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace GZipTest
{
    public abstract class GZ
    {
        public const int maxBlock = 40;

        protected object lockerW = new object();
        protected bool _cancelled = false;
        protected bool _success = false;
        protected string inFile, outFile;
        protected static int _threadNum = Environment.ProcessorCount;

        protected int blockSize = 10485760;
        protected QueueController _queueReader = new QueueController();
        protected QueueController _queueWrite = new QueueController();
        protected Thread[] tPool = new Thread[_threadNum];

        public GZ(string input, string output)
        {
            this.inFile = input;
            this.outFile = output;
        }
        public abstract void Launch();
        protected bool isSucces()
        {
            if ((_queueReader.isFin)&&(_queueReader.RetCount()==0)&&(_queueWrite.RetCount()==0))
            {
                _success=true;
            }
            return _success;
        }
    }
    
    }

