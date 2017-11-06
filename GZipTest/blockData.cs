using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GZipTest
{
    public class blockData
    {
        private int id;
        private byte[] block;
        private byte[] compressedBlock;

        public int ID { get { return id; } }
        public byte[] Block { get { return block; }}
        public byte[] CompressedBlock{ get { return compressedBlock; } }


        public blockData(int id, byte[] _block)
            : this(id, _block, new byte[0])
        {

        }

        public blockData(int id, byte[] block, byte[] compressedblock)
        {
            this.id = id;
            this.block = block;
            this.compressedBlock = compressedblock;
        }
    }
}
