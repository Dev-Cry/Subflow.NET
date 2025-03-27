using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subflow.NET.IO.Loader
{
    public class BufferSizeDeterminer : IBufferSizeDeterminer
    {
        private const int DefaultBufferSize = 4096; // Defaultní velikost bufferu (4 KB)
        private const int MaxBufferSize = 65536;   // Maximální velikost bufferu (64 KB)

        public int Determine(int? userDefinedBufferSize, long fileSize)
        {
            if (userDefinedBufferSize.HasValue)
            {
                return Math.Min(userDefinedBufferSize.Value, MaxBufferSize);
            }
            if (fileSize <= DefaultBufferSize)
            {
                return DefaultBufferSize;
            }
            return MaxBufferSize;
        }
    }
}
