using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Subflow.NET.Data.Model;
using Subflow.NET.IO.Reader;
using Subflow.NET.Parser;

namespace Subflow.NET.IO.Loader
{
    public class FileLoader : IFileLoader
    {
        private readonly ILogger<FileLoader> _logger;
        private readonly IFileReader _fileReader;
        private readonly ISubtitleParser _subtitleParser;

        public FileLoader(
            ILogger<FileLoader> logger,
            IFileReader fileReader,
            ISubtitleParser subtitleParser)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _fileReader = fileReader ?? throw new ArgumentNullException(nameof(fileReader));
            _subtitleParser = subtitleParser ?? throw new ArgumentNullException(nameof(subtitleParser));
        }
        public async IAsyncEnumerable<ISubtitle> LoadFileAsync(string filePath, int? bufferSize = null, int degreeOfParallelism = 1)
        {
            ValidateFilePath(filePath);

            _logger.LogInformation("Načítám soubor: {FilePath}", filePath);
            _logger.LogInformation("Formát souboru: {FileExtension}", Path.GetExtension(filePath)?.ToLowerInvariant());

            var fileSize = new FileInfo(filePath).Length;
            int effectiveBufferSize = DetermineBufferSize(bufferSize, fileSize);

            // Zpracování s již existující instancí _fileReader
            var processingBlock = new TransformBlock<string, ISubtitle>(
                async line => await _subtitleParser.ParseLineAsync(line.TrimEnd('\r')),
                new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = degreeOfParallelism });

            var outputQueue = new BufferBlock<ISubtitle>();
            processingBlock.LinkTo(outputQueue, new DataflowLinkOptions { PropagateCompletion = true });

            await foreach (var line in _fileReader.ReadFileLinesAsync(effectiveBufferSize))
            {
                await processingBlock.SendAsync(line);
            }

            processingBlock.Complete();

            while (await outputQueue.OutputAvailableAsync())
            {
                var subtitle = await outputQueue.ReceiveAsync();
                if (subtitle != null)
                {
                    yield return subtitle;
                }
            }

            // Flush zbývajících dat z parseru
            var remainingSubtitle = await _subtitleParser.FlushAsync();
            if (remainingSubtitle != null)
                yield return remainingSubtitle;

            _logger.LogInformation("Soubor úspěšně načten asynchronně.");
        }

        private void ValidateFilePath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                _logger.LogWarning("Cesta k souboru je prázdná nebo null.");
                throw new ArgumentException("Cesta k souboru nesmí být prázdná.", nameof(filePath));
            }
            if (!File.Exists(filePath))
            {
                _logger.LogWarning("Soubor '{FilePath}' nebyl nalezen.", filePath);
                throw new FileNotFoundException($"Soubor '{filePath}' nebyl nalezen.");
            }
        }

        private static int DetermineBufferSize(int? userDefinedBufferSize, long fileSize)
        {
            const int DefaultBufferSize = 4096; // Defaultní velikost bufferu (4 KB)
            const int MaxBufferSize = 65536;   // Maximální velikost bufferu (64 KB)
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