using Microsoft.Extensions.Logging;
using Subflow.NET.Data.Model;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Subflow.NET.Parser
{
    public class SubtitleParser(ILogger logger) : ISubtitleParser
    {
        private readonly ILogger _logger = logger;

        // Zjednodušený příklad stavu v parseru:
        private Subtitle? _currentSubtitle = null;

        public Task<ISubtitle?> ParseLineAsync(string line)
        {
            if (int.TryParse(line, out int index))
            {
                _currentSubtitle = new Subtitle(index, TimeSpan.Zero, TimeSpan.Zero, new List<string>());
                return Task.FromResult<ISubtitle?>(null);
            }
            else if (_currentSubtitle != null && TryParseTimeRange(line, out var startTime, out var endTime))
            {
                _currentSubtitle.StartTime = startTime;
                _currentSubtitle.EndTime = endTime;
                return Task.FromResult<ISubtitle?>(null);
            }
            else if (_currentSubtitle != null && !string.IsNullOrWhiteSpace(line))
            {
                _currentSubtitle.Lines.Add(line);
                return Task.FromResult<ISubtitle?>(null);
            }
            else if (string.IsNullOrWhiteSpace(line) && _currentSubtitle != null)
            {
                var finishedSubtitle = _currentSubtitle;
                _currentSubtitle = null;
                return Task.FromResult<ISubtitle?>(finishedSubtitle);
            }

            return Task.FromResult<ISubtitle?>(null);
        }

        private bool TryParseTimeRange(string timeRange, out TimeSpan startTime, out TimeSpan endTime)
        {
            startTime = TimeSpan.Zero;
            endTime = TimeSpan.Zero;
            var parts = timeRange.Split(new[] { "-->" }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
            {
                return false;
            }
            if (TryParseTime(parts[0], out startTime) && TryParseTime(parts[1], out endTime))
            {
                return true;
            }
            return false;
        }

        private bool TryParseTime(string timeString, out TimeSpan time)
        {
            time = TimeSpan.Zero;
            var match = Regex.Match(timeString, @"(\d{2}):(\d{2}):(\d{2}),(\d{3})");
            if (!match.Success || match.Groups.Count != 5)
            {
                return false;
            }

            if (int.TryParse(match.Groups[1].Value, out int hours) &&
                int.TryParse(match.Groups[2].Value, out int minutes) &&
                int.TryParse(match.Groups[3].Value, out int seconds) &&
                int.TryParse(match.Groups[4].Value, out int milliseconds))
            {
                time = new TimeSpan(hours, minutes, seconds, 0, milliseconds);
                return true;
            }
            return false;
        }

        public Task<ISubtitle?> FlushAsync()
        {
            if (_currentSubtitle != null)
            {
                var subtitle = _currentSubtitle;
                _currentSubtitle = null;
                return Task.FromResult<ISubtitle?>(subtitle);
            }
            return Task.FromResult<ISubtitle?>(null);
        }

    }
}