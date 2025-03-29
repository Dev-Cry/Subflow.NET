using System;

namespace Subflow.NET.Parser
{
    public interface ISubtitleTimeParser
    {
        bool TryParseTimeRange(string line, out TimeSpan startTime, out TimeSpan endTime);
        bool TryParseTime(ReadOnlySpan<char> span, out TimeSpan time);
    }
}