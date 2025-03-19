using Subflow.NET.Loaders.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Subflow.NET.Loaders.Mapper
{
    /// <summary>
    /// Abstraktní třída pro mapování formátů na jejich přípony.
    /// </summary>
    public abstract class FileFormatMapper
    {
        /// <summary>
        /// Základní mapování formátů na přípony.
        /// </summary>
        protected virtual Dictionary<FileFormat, string[]> FormatToExtensions { get; } = new()
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
        public HashSet<string> GetAllowedExtensions(IEnumerable<FileFormat> formats)
        {
            var extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var format in formats)
            {
                if (GetFormatToExtensions().TryGetValue(format, out var exts))
                {
                    foreach (var ext in exts)
                    {
                        extensions.Add(ext);
                    }
                }
            }
            return extensions;
        }

        /// <summary>
        /// Metoda pro získání mapování formátů na přípony.
        /// Může být přepsána v odvozených třídách pro přidání nebo úpravu mapování.
        /// </summary>
        /// <returns>Mapování formátů na přípony.</returns>
        protected virtual Dictionary<FileFormat, string[]> GetFormatToExtensions()
        {
            return FormatToExtensions;
        }

        /// <summary>
        /// Přidá nový formát a jeho přípony do mapování.
        /// </summary>
        /// <param name="format">Formát souboru.</param>
        /// <param name="extensions">Přípony pro daný formát.</param>
        public void AddFormatMapping(FileFormat format, params string[] extensions)
        {
            if (extensions == null || extensions.Length == 0)
                throw new ArgumentException("Přípony nesmí být prázdné.", nameof(extensions));

            var currentMapping = GetFormatToExtensions();
            if (currentMapping.ContainsKey(format))
            {
                currentMapping[format] = currentMapping[format].Concat(extensions).Distinct().ToArray();
            }
            else
            {
                currentMapping.Add(format, extensions);
            }
        }

        /// <summary>
        /// Odebere formát z mapování.
        /// </summary>
        /// <param name="format">Formát souboru.</param>
        public void RemoveFormatMapping(FileFormat format)
        {
            var currentMapping = GetFormatToExtensions();
            if (currentMapping.ContainsKey(format))
            {
                currentMapping.Remove(format);
            }
        }
    }
}