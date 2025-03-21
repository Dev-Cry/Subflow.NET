using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Subflow.NET.IO.Reader
{
    public class FileReader : IFileReader
    {
        private readonly string _filePath;
        private readonly Encoding _fileEncoding;

        public FileReader(string filePath, Encoding fileEncoding)
        {
            _filePath = filePath;
            _fileEncoding = fileEncoding;
        }

        public async IAsyncEnumerable<string> ReadFileLinesAsync(int bufferSize)
        {
            using var stream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: bufferSize, useAsync: true);
            using var reader = new StreamReader(stream, _fileEncoding);

            char[] buffer = new char[bufferSize];
            int bytesRead;

            while ((bytesRead = await reader.ReadBlockAsync(buffer, 0, buffer.Length)) > 0)
            {
                var chunk = new string(buffer, 0, bytesRead);
                foreach (var line in chunk.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    yield return line;
                }
            }
        }
    }
}