using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subflow.NET.IO.Reader
{
    public interface IFileReader
    {
        IAsyncEnumerable<string> ReadFileLinesAsync(int bufferSize);
    }
}