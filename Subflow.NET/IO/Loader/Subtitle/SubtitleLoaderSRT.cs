using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Subflow.NET.Data.Model.Subtitle;
using Subflow.NET.IO.Loader.Base;

namespace Subflow.NET.IO.Loader.Subtitle
{
    public class SubtitleLoaderSRT : FileLoaderBase<SrtSubtitle>
    {
        // Regex pro extrakci časového intervalu
        private static readonly Regex TimeRegex = new Regex(@"(\d+):(\d{2}):(\d{2}),(\d{3})\s*-->\s*(\d+):(\d{2}):(\d{2}),(\d{3})");

        // Stavové proměnné pro parsování
        private SrtSubtitle _currentSubtitle;
        private bool _isParsingText;

        public SubtitleLoaderSRT(ILogger logger) : base(logger)
        {
        }

        /// <summary>
        /// Parsuje jeden řádek `.srt` souboru a vytváří objekt SrtSubtitle.
        /// </summary>
        protected override async Task<SrtSubtitle> ParseLineAsync(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                // Prázdný řádek signalizuje konec titulku
                return null;
            }

            if (int.TryParse(line, out int index))
            {
                // Nový titulek - pořadové číslo
                _currentSubtitle = new SrtSubtitle { Index = index };
                return null; // Pořadové číslo samo o sobě není kompletní titulek
            }

            var timeMatch = TimeRegex.Match(line);
            if (timeMatch.Success)
            {
                // Časový interval
                var startTime = ParseTime(timeMatch.Groups[1].Value, timeMatch.Groups[2].Value, timeMatch.Groups[3].Value, timeMatch.Groups[4].Value);
                var endTime = ParseTime(timeMatch.Groups[5].Value, timeMatch.Groups[6].Value, timeMatch.Groups[7].Value, timeMatch.Groups[8].Value);

                _currentSubtitle.StartTime = startTime;
                _currentSubtitle.EndTime = endTime;
                _isParsingText = true;
                return null; // Časový interval samo o sobě není kompletní titulek
            }

            if (_isParsingText)
            {
                // Text titulku
                _currentSubtitle.Text += line + Environment.NewLine;
                return null; // Text může být víceřádkový
            }

            throw new FormatException($"Neplatný formát řádku: {line}");
        }

        /// <summary>
        /// Pomocná metoda pro parsování času.
        /// </summary>
        private static TimeSpan ParseTime(string hours, string minutes, string seconds, string milliseconds)
        {
            return new TimeSpan(
                int.Parse(hours),
                int.Parse(minutes),
                int.Parse(seconds),
                int.Parse(milliseconds)
            );
        }
    }
}