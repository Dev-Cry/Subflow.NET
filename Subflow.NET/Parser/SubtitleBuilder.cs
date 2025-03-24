using Microsoft.Extensions.Logging;
using Subflow.NET.Data.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Subflow.NET.Parser
{
    public class SubtitleBuilder : ISubtitleBuilder
    {
        private readonly ILogger<ISubtitleParser> _logger;
        private Subtitle? _currentSubtitle = null;

        public SubtitleBuilder(ILogger<ISubtitleParser> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ISubtitle?> ParseLineAsync(string line, ISubtitleTimeParser timeParser)
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
                if (_currentSubtitle != null && timeParser.TryParseTimeRange(line, out var startTime, out var endTime))
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