using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Subflow.NET.IO.SubtitleReader
{
    public interface ISubtitleLineReader
    {
        IAsyncEnumerable<string> ReadFileLinesAsync(int bufferSize,[EnumeratorCancellation] CancellationToken cancellationToken = default);

    }
}