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
        private readonly IFilePathValidator _filePathValidator;
        private readonly IBufferSizeDeterminer _bufferSizeDeterminer;

        public FileLoader(
            ILogger<FileLoader> logger,
            IFileReader fileReader,
            ISubtitleParser subtitleParser,
            IFilePathValidator filePathValidator,
            IBufferSizeDeterminer bufferSizeDeterminer)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _fileReader = fileReader ?? throw new ArgumentNullException(nameof(fileReader));
            _subtitleParser = subtitleParser ?? throw new ArgumentNullException(nameof(subtitleParser));
            _filePathValidator = filePathValidator ?? throw new ArgumentNullException(nameof(filePathValidator));
            _bufferSizeDeterminer = bufferSizeDeterminer ?? throw new ArgumentNullException(nameof(bufferSizeDeterminer));
        }

        public async IAsyncEnumerable<ISubtitle> LoadFileAsync(string filePath, int? bufferSize = null, int degreeOfParallelism = 1)
        {
            _filePathValidator.Validate(filePath);

            _logger.LogInformation("Načítám soubor: {FilePath}", filePath);
            _logger.LogInformation("Formát souboru: {FileExtension}", Path.GetExtension(filePath)?.ToLowerInvariant());

            var fileSize = new FileInfo(filePath).Length;
            int effectiveBufferSize = _bufferSizeDeterminer.Determine(bufferSize, fileSize);

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

            var remainingSubtitle = await _subtitleParser.FlushAsync();
            if (remainingSubtitle != null)
                yield return remainingSubtitle;

            _logger.LogInformation("Soubor úspěšně načten asynchronně.");
        }
    }
}