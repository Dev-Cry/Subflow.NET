using Microsoft.Extensions.Logging;
using System;

namespace Subflow.NET.Parser;

/// <summary>
/// Třída pro parsování časového rozsahu z řádku titulků ve formátu SubRip (SRT).
/// Implementuje rozhraní ISubtitleTimeParser.
/// </summary>
/// <param name="logger">Logger pro výpis výstrah a ladicích informací.</param>
public class SubtitleTimeParser(ILogger<SubtitleTimeParser> logger) : ISubtitleTimeParser
{
    // Konstanta určující oddělovač mezi počátečním a koncovým časem ve formátu SRT.
    private const string TimecodeDelimiter = "-->";

    /// <summary>
    /// Pokusí se rozparsovat řetězec reprezentující časový rozsah (např. "00:00:01,000 --> 00:00:04,000").
    /// </summary>
    /// <param name="line">Řetězec s časovým rozsahem.</param>
    /// <param name="startTime">Výstupní parametr - čas začátku.</param>
    /// <param name="endTime">Výstupní parametr - čas konce.</param>
    /// <returns>True, pokud byl časový rozsah úspěšně rozpoznán, jinak false.</returns>
    public bool TryParseTimeRange(string line, out TimeSpan startTime, out TimeSpan endTime)
    {
        // Inicializace výstupních proměnných na nulu
        startTime = endTime = TimeSpan.Zero;

        // Konverze na ReadOnlySpan kvůli výkonu a bezalokacní práci s textem
        ReadOnlySpan<char> lineSpan = line.AsSpan();

        // Rychlá předběžná validace – pokud řetězec neobsahuje základní znaky času, neparsujeme
        if (lineSpan.IndexOf(':') < 0 || lineSpan.IndexOf(',') < 0)
            return false;

        // Vyhledání pozice oddělovače "-->"
        int delimiterIndex = lineSpan.IndexOf(TimecodeDelimiter, StringComparison.Ordinal);
        if (delimiterIndex < 0)
        {
            // Výstraha při chybějícím oddělovači
            logger.LogWarning("Nenalezen delimiter '-->': {Line}", line);
            return false;
        }

        // Rozdělení řádku na dvě části – začátek a konec časového rozsahu
        var startPart = lineSpan.Slice(0, delimiterIndex).Trim();
        var endPart = lineSpan.Slice(delimiterIndex + TimecodeDelimiter.Length).Trim();

        // Pokud jsou obě části neprázdné a dají se přeložit jako platné časy, úspěch
        if (!startPart.IsEmpty && !endPart.IsEmpty &&
            TryParseTime(startPart, out startTime) &&
            TryParseTime(endPart, out endTime))
        {
            return true;
        }

        // Pokud něco selže, vypíšeme varování a vracíme false
        logger.LogWarning("Nepodařilo se rozpoznat časový rozsah: {Line}", line);
        return false;
    }

    /// <summary>
    /// Pokusí se rozparsovat jeden časový údaj ve formátu "hh:mm:ss,fff".
    /// </summary>
    /// <param name="span">Textový úsek reprezentující čas.</param>
    /// <param name="time">Výstupní parametr – výsledný TimeSpan.</param>
    /// <returns>True, pokud se čas podařilo úspěšně přeložit, jinak false.</returns>
    public bool TryParseTime(ReadOnlySpan<char> span, out TimeSpan time)
    {
        // Používáme přesný formát pro parsování s čárkou jako oddělovačem milisekund
        if (TimeSpan.TryParseExact(span, @"hh\:mm\:ss\,fff", null, out time))
        {
            return true;
        }

        // Pokud je zapnuté ladění, vypíšeme podrobnost o chybném formátu
        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("Nevalidní formát času: {Span}", new string(span));
        }

        return false;
    }
}
