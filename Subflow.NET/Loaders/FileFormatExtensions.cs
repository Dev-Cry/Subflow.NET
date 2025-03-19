using Subflow.NET.Loaders.Enums;

/// <summary>
/// Mapování formátů na jejich přípony.
/// </summary>
internal static class FileFormatExtensions
{
    public static readonly Dictionary<FileFormat, string[]> FormatToExtensions = new()
    {
        { FileFormat.SRT, new[] { ".srt" } },
        { FileFormat.VTT, new[] { ".vtt" } },
        { FileFormat.ASS, new[] { ".ass" } },
    };

    /// <summary>
    /// Získá všechny povolené přípony pro danou sadu formátů.
    /// </summary>
    /// <param name="formats">Seznam podporovaných formátů.</param>
    /// <returns>Seznam povolených přípon.</returns>
    public static HashSet<string> GetAllowedExtensions(IEnumerable<FileFormat> formats)
    {
        var extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var format in formats)
        {
            if (FormatToExtensions.TryGetValue(format, out var exts))
            {
                foreach (var ext in exts)
                {
                    extensions.Add(ext);
                }
            }
        }
        return extensions;
    }
}