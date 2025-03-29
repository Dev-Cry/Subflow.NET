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

            // Rychlá předběžná kontrola, zda řádek vypadá jako časový rozsah
            if (!line.Contains(":") || !line.Contains(","))
            {
                return false;
            }

            foreach (var delimiter in _timecodeDelimiters)
            {
                if (line.Contains(delimiter, StringComparison.OrdinalIgnoreCase))
                {
                    var parts = line.Split(new[] { delimiter }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 2 &&
                        TryParseTime(parts[0].Trim(), out startTime) &&
                        TryParseTime(parts[1].Trim(), out endTime))
                    {
                        return true;
                    }
                }
            }

            _logger.LogWarning("Nepodařilo se rozpoznat časový rozsah: {Line}", line);
            return false;
        }

        public bool TryParseTime(string timeString, out TimeSpan time)
        {
            time = TimeSpan.Zero;
            var match = _timeRegex.Match(timeString);

            if (!match.Success)
            {
                _logger.LogDebug("Regex match selhal: {TimeString}", timeString);
                return false;
            }

            if (int.TryParse(match.Groups["Hours"].Value, out int hours) &&
                int.TryParse(match.Groups["Minutes"].Value, out int minutes) &&
                int.TryParse(match.Groups["Seconds"].Value, out int seconds) &&
                int.TryParse(match.Groups["Milliseconds"].Value, out int milliseconds))
            {
                time = new TimeSpan(0, hours, minutes, seconds, milliseconds);
                return true;
            }

            _logger.LogWarning("Selhalo parsování jednotlivých částí času: {TimeString}", timeString);
            return false;
        }
    }
}
