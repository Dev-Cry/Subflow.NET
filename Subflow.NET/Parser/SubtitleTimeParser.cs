using Microsoft.Extensions.Logging;
using System;
using System.Text.RegularExpressions;

namespace Subflow.NET.Parser
{
    public class SubtitleTimeParser : ISubtitleTimeParser
    {
        private static readonly string[] _timecodeDelimiters =
            { "-->", "- >", "->", "-- ->", "--->", "—>", "- ->" };

        private static readonly Regex _timeRegex = new(
            @"^(?<Hours>\d{2}):(?<Minutes>\d{2}):(?<Seconds>\d{2}),(?<Milliseconds>\d{3})$",
            RegexOptions.Compiled);

        private readonly ILogger<SubtitleTimeParser> _logger;

        // DI-friendly konstruktor s loggerem (nebo jinou závislostí)
        public SubtitleTimeParser(ILogger<SubtitleTimeParser> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public bool TryParseTimeRange(string line, out TimeSpan startTime, out TimeSpan endTime)
        {
            startTime = endTime = TimeSpan.Zero;

            ReadOnlySpan<char> lineSpan = line.AsSpan();

            // Optimalizovaná předběžná kontrola pomocí jednoho průchodu
            if (lineSpan.IndexOf(':') < 0 || lineSpan.IndexOf(',') < 0)
            {
                return false;
            }

            foreach(var delimiter in _timecodeDelimiters)
    {
                int delimiterIndex = lineSpan.IndexOf(delimiter, StringComparison.OrdinalIgnoreCase);

                if (delimiterIndex >= 0)
                {
                    var startPart = lineSpan.Slice(0, delimiterIndex).Trim();
                    var endPart = lineSpan.Slice(delimiterIndex + delimiter.Length).Trim();

                    if (!startPart.IsEmpty && !endPart.IsEmpty &&
                        TryParseTime(startPart.ToString(), out startTime) &&
                        TryParseTime(endPart.ToString(), out endTime))
                    {
                        return true;
                    }
                }
            }


            _logger.LogWarning("Nepodařilo se rozpoznat časový rozsah: {Line}", line);
            return false;
        }

        public bool TryParseTime(ReadOnlySpan<char> span, out TimeSpan time)
        {
            time = TimeSpan.Zero;

            if (span.Length != 12 || span[2] != ':' || span[5] != ':' || span[8] != ',')
            {
                _logger.LogDebug("Nevalidní formát času: {Span}", new string(span));
                return false;
            }

            if (int.TryParse(span.Slice(0, 2), out int hours) &&
                int.TryParse(span.Slice(3, 2), out int minutes) &&
                int.TryParse(span.Slice(6, 2), out int seconds) &&
                int.TryParse(span.Slice(9, 3), out int milliseconds))
            {
                time = new TimeSpan(0, hours, minutes, seconds, milliseconds);
                return true;
            }

            _logger.LogWarning("Selhalo parsování jednotlivých částí času: {Span}", new string(span));
            return false;
        }
    }
}
