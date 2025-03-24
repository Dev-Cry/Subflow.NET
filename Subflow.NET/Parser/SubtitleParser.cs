using Microsoft.Extensions.Logging;
using Subflow.NET.Data.Model;
using System;
using System.Threading.Tasks;

namespace Subflow.NET.Parser
{
    public class SubtitleParser : ISubtitleParser
    {
        private readonly ILogger<SubtitleParser> _logger;
        private readonly ISubtitleTimeParser _timeParser;
        private readonly ISubtitleBuilder _subtitleBuilder;

        public SubtitleParser(ILogger<SubtitleParser> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _timeParser = new SubtitleTimeParser();
            _subtitleBuilder = new SubtitleBuilder(_logger);
        }

        public async Task<ISubtitle?> ParseLineAsync(string line)
        {
            return await _subtitleBuilder.ParseLineAsync(line, _timeParser);
        }

        public Task<ISubtitle?> FlushAsync()
        {
            return _subtitleBuilder.FlushAsync();
        }
    }
}