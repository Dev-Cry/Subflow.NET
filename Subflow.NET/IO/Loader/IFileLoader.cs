using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Subflow.NET.Data.Model;

namespace Subflow.NET.IO.Loader
{
    public interface IFileLoader
    {
        IAsyncEnumerable<ISubtitle?> LoadFileAsync(string filePath, int? bufferSize = null, int degreeOfParallelism = 8);
    }
} 