using Microsoft.Extensions.Logging;
using System;

namespace Subflow.NET.Parser;

public class SubtitleTimeParser(ILogger<SubtitleTimeParser> logger) : ISubtitleTimeParser
{
    private const string TimecodeDelimiter = "-->";

    public bool TryParseTimeRange(string line, out TimeSpan startTime, out TimeSpan endTime)
    {
        startTime = endTime = TimeSpan.Zero;

        ReadOnlySpan<char> lineSpan = line.AsSpan();

        if (lineSpan.IndexOf(':') < 0 || lineSpan.IndexOf(',') < 0)
            return false;

        int delimiterIndex = lineSpan.IndexOf(TimecodeDelimiter, StringComparison.Ordinal);
        if (delimiterIndex < 0)
        {
            logger.LogWarning("Nenalezen delimiter '-->': {Line}", line);
            return false;
        }

        var startPart = lineSpan.Slice(0, delimiterIndex).Trim();
        var endPart = lineSpan.Slice(delimiterIndex + TimecodeDelimiter.Length).Trim();

        if (!startPart.IsEmpty && !endPart.IsEmpty &&
            TryParseTime(startPart, out startTime) &&
            TryParseTime(endPart, out endTime))
        {
            return true;
        }

        logger.LogWarning("Nepodařilo se rozpoznat časový rozsah: {Line}", line);
        return false;
    }

    public bool TryParseTime(ReadOnlySpan<char> span, out TimeSpan time)
    {
        if (TimeSpan.TryParseExact(span, @"hh\:mm\:ss\,fff", null, out time))
        {
            return true;
        }

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("Nevalidní formát času: {Span}", new string(span));
        }

        return false;
    }
}
