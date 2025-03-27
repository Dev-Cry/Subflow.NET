using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subflow.NET.IO.Loader
{
    public interface IBufferSizeDeterminer
    {
        int Determine(int? userDefinedBufferSize, long fileSize);
    }
}
