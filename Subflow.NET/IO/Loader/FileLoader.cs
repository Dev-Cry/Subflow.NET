using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Subflow.NET.Data.Model;
using Subflow.NET.Parser;
using Subflow.NET.IO.Loader.Validation;
using Subflow.NET.IO.SubtitleReader;

namespace Subflow.NET.IO.Loader
{
    public class FileLoader : IFileLoader
    {
        private readonly ILogger<FileLoader> _logger;
        private readonly ISubtitleLineReader _fileReader;
        private readonly ISubtitleParser _subtitleParser;
        private readonly IValidatorFactory _validatorFactory;
        private readonly IBufferSizeDeterminer _bufferSizeDeterminer;

        public FileLoader(
            ILogger<FileLoader> logger,
            ISubtitleLineReader fileReader,
            ISubtitleParser subtitleParser,
            IValidatorFactory validatorFactory,
            IBufferSizeDeterminer bufferSizeDeterminer)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _fileReader = fileReader ?? throw new ArgumentNullException(nameof(fileReader));
            _subtitleParser = subtitleParser ?? throw new ArgumentNullException(nameof(subtitleParser));
            _validatorFactory = validatorFactory ?? throw new ArgumentNullException(nameof(validatorFactory));
            _bufferSizeDeterminer = bufferSizeDeterminer ?? throw new ArgumentNullException(nameof(bufferSizeDeterminer));
        }

        public async IAsyncEnumerable<ISubtitle> LoadFileAsync(string filePath, int? bufferSize = null, int degreeOfParallelism = 8)
        {
            var pathValidator = _validatorFactory.CreatePathValidator();
            pathValidator.Validate(filePath);

            var fileInfo = new FileInfo(filePath);
            var fileValidator = _validatorFactory.CreateFileValidator();
            fileValidator.Validate(fileInfo);

            _logger.LogInformation("Načítám soubor: {FilePath}", filePath);
            _logger.LogInformation("Formát souboru: {FileExtension}", fileInfo.Extension.ToLowerInvariant());

            long fileSize = fileInfo.Length;
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
