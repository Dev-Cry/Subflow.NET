using Microsoft.Extensions.Logging;
using Subflow.NET.Data.Model;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Subflow.NET.Parser
{
public class SubtitleParser : ISubtitleParser
    {
        private readonly ILogger<SubtitleParser> _logger;
        private static readonly string[] _timecodeDelimiters = { "-->", "- >", "->", "-- ->", "--->", "—>", "- ->" };
        private static readonly Regex _timeRegex = new(@"^(?<Hours>\d{2}):(?<Minutes>\d{2}):(?<Seconds>\d{2}),(?<Milliseconds>\d{3})$", RegexOptions.Compiled);
        private Subtitle? _currentSubtitle = null;

        public SubtitleParser(ILogger<SubtitleParser> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<ISubtitle?> ParseLineAsync(string line)
            {
                try
                {
                    await Task.Yield(); // Zajistí skutečnou asynchronní implementaci

                    // Zkontroluj prázdný nebo whitespace řádky
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        if (_currentSubtitle != null)
                        {
                            if (_currentSubtitle.StartTime == TimeSpan.Zero || _currentSubtitle.EndTime == TimeSpan.Zero)
                            {
                                _logger.LogWarning("Dokončen titulek bez platného časového rozsahu: {Index}", _currentSubtitle.Index);
                                _currentSubtitle = null;
                                return null;
                            }

                            var completedSubtitle = _currentSubtitle;
                            _currentSubtitle = null;
                            return completedSubtitle;
                        }
                        return null;
                    }

                    // Pokus o parsování indexu titulku
                    if (int.TryParse(line.Trim(), out int index))
                    {
                        if (index <= 0)
                        {
                            _logger.LogWarning("Titulek obsahuje neplatný index ({Index})", index);
                            return null;
                        }

                        _currentSubtitle = new Subtitle(index, TimeSpan.Zero, TimeSpan.Zero, new List<string>());
                        return null;
                    }

                    // Pokus o parsování časového rozsahu
                    if (_currentSubtitle != null && TryParseTimeRange(line, out var startTime, out var endTime))
                    {
                        if (startTime > endTime)
                        {
                            _logger.LogWarning("Titulek {Index} má přehozený časový rozsah: začátek {StartTime}, konec {EndTime}. Proběhne automatická korekce.",
                                _currentSubtitle.Index, startTime, endTime);

                            // Automatická korekce času
                            (startTime, endTime) = (endTime, startTime);
                        }

                        _currentSubtitle.StartTime = startTime;
                        _currentSubtitle.EndTime = endTime;
                        return null;
                    }

                    // Přidání textu titulku
                    if (_currentSubtitle != null)
                    {
                        _currentSubtitle.Lines.Add(line);
                        return null;
                    }

                    // Neočekávaný stav - text řádku bez inicializace titulku
                    _logger.LogWarning("Ignorován neočekávaný řádek (bez inicializace titulku): '{Line}'", line);
                    return null;
                }
                catch (FormatException ex)
                {
                    _logger.LogError(ex, "Chyba při parsování formátu v řádku '{Line}'", line);
                    _currentSubtitle = null;
                    return null;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Neočekávaná chyba při parsování řádku '{Line}'", line);
                    _currentSubtitle = null;
                    return null;
                }
            }

        private bool TryParseTimeRange(string line, out TimeSpan startTime, out TimeSpan endTime)
        {
            startTime = endTime = TimeSpan.Zero;

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

            return false;
        }

        private bool TryParseTime(string timeString, out TimeSpan time)
        {
            time = TimeSpan.Zero;
            var match = _timeRegex.Match(timeString);

            if (!match.Success)
                return false;

            if (int.TryParse(match.Groups["Hours"].Value, out int hours) &&
                int.TryParse(match.Groups["Minutes"].Value, out int minutes) &&
                int.TryParse(match.Groups["Seconds"].Value, out int seconds) &&
                int.TryParse(match.Groups["Milliseconds"].Value, out int milliseconds))
            {
                time = new TimeSpan(0, hours, minutes, seconds, milliseconds);
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