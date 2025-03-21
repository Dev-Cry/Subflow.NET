using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Logging;
using Subflow.NET.Data.Model;

namespace Subflow.NET.IO.Loader
{
    /// <summary>
    /// Třída pro načítání a parsování SRT titulků.
    /// </summary>
    public class FileLoader
    {
        /// <summary>
        /// Cesta k souboru.
        /// </summary>
        public string FilePath { get; private set; }

        /// <summary>
        /// Kódování souboru.
        /// </summary>
        public Encoding FileEncoding { get; private set; } = Encoding.UTF8;

        /// <summary>
        /// Přípona souboru.
        /// </summary>
        public string FileExtension => Path.GetExtension(FilePath)?.ToLowerInvariant();

        /// <summary>
        /// Stav načítání souboru.
        /// </summary>
        public bool IsLoaded { get; private set; }

        /// <summary>
        /// Logger pro zpracování a protokolování chyb.
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Konstruktor pro injektování loggeru.
        /// </summary>
        /// <param name="logger">Instance loggeru.</param>
        public FileLoader(ILogger logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Asynchronní metoda pro načítání souboru a transformaci řádků na typ ISubtitle.
        /// </summary>
        /// <param name="filePath">Cesta k souboru.</param>
        /// <param name="bufferSize">Volitelná velikost bufferu.</param>
        /// <param name="degreeOfParallelism">Počet paralelních vláken pro zpracování.</param>
        /// <returns>Asynchronní posloupnost výsledků typu ISubtitle.</returns>
        public async IAsyncEnumerable<ISubtitle> LoadFileAsync(string filePath, int? bufferSize = null, int degreeOfParallelism = 4)
        {
            // Validace cesty k souboru
            ValidateFilePath(filePath);

            // Nastavení cesty k souboru
            FilePath = filePath;

            // Logování informací o souboru
            Logger.LogInformation("Načítám soubor: {FilePath}", FilePath);
            Logger.LogInformation("Formát souboru: {FileExtension}", FileExtension);

            // Kontrola stavu načítání
            if (IsLoaded)
            {
                Logger.LogInformation("Soubor je již načten.");
                yield break;
            }

            // Získání velikosti souboru
            var fileInfo = new FileInfo(filePath);
            long fileSize = fileInfo.Length;

            // Určení optimální velikosti bufferu
            int effectiveBufferSize = DetermineBufferSize(bufferSize, fileSize);

            // Otevření souboru pro čtení
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: effectiveBufferSize, useAsync: true);
            using var reader = new StreamReader(stream, FileEncoding);

            IsLoaded = true;

            var buffer = new char[effectiveBufferSize];
            var leftover = string.Empty; // Zbývající data z předchozího bloku
            int bytesRead;

            // Vytvoření pipeline pro paralelní zpracování
            var processingBlock = new TransformBlock<string, ISubtitle>(
                async line =>
                {
                    // Transformace řádku na ISubtitle
                    return await ParseLineAsync(line.TrimEnd('\r'));
                },
                new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = degreeOfParallelism // Počet paralelních vláken
                });

            var outputQueue = new BufferBlock<ISubtitle>();

            // Propojení bloků
            processingBlock.LinkTo(outputQueue, new DataflowLinkOptions { PropagateCompletion = true });

            // Čtení a zpracování souboru
            while ((bytesRead = await reader.ReadBlockAsync(buffer, 0, buffer.Length)) > 0)
            {
                var chunk = new string(buffer, 0, bytesRead);
                var lines = (leftover + chunk).Split(new[] { '\n' }, StringSplitOptions.None);

                for (int i = 0; i < lines.Length - 1; i++)
                {
                    await processingBlock.SendAsync(lines[i]); // Odeslání řádku do pipeline
                }

                leftover = lines[^1]; // Poslední část uložíme pro další iteraci
            }

            // Pokud zbývá nějaký obsah v "leftover", odeslat ho do pipeline
            if (!string.IsNullOrEmpty(leftover))
            {
                await processingBlock.SendAsync(leftover);
            }

            // Signalizace konce zpracování
            processingBlock.Complete();

            // Iterace přes zpracované řádky
            while (await outputQueue.OutputAvailableAsync())
            {
                var subtitle = await outputQueue.ReceiveAsync();
                if (subtitle != null)
                {
                    yield return subtitle;
                }
            }

            Logger.LogInformation("Soubor úspěšně načten asynchronně.");
        }

        /// <summary>
        /// Parsování řádku na objekt ISubtitle.
        /// </summary>
        /// <param name="line">Řádek ze souboru.</param>
        /// <returns>Objekt ISubtitle.</returns>
        private async Task<ISubtitle> ParseLineAsync(string line)
        {
            var subtitle = new Subtitle(0, TimeSpan.Zero, TimeSpan.Zero, new List<string>());
            if (int.TryParse(line, out int index))
            {
                subtitle.Index = index;
            }
            else if (TryParseTimeRange(line, out var startTime, out var endTime))
            {
                subtitle.StartTime = startTime;
                subtitle.EndTime = endTime;
            }
            else if (!string.IsNullOrWhiteSpace(line))
            {
                // Přidáte text titulku do kolekce
                subtitle.Lines.Add(line);
            }
            else
            {
                // Pokud je to prázdný řádek, ukončíme aktuální titulek a připravíme se na další
                if (subtitle.Lines.Count > 0)
                {
                    return subtitle;
                }
            }
            return null; // Pokud není validní řádek, vrátíme null
        }

        /// <summary>
        /// Určuje optimální velikost bufferu na základě velikosti souboru a volitelného uživatelského nastavení.
        /// </summary>
        /// <param name="userDefinedBufferSize">Volitelná velikost bufferu zadávaná uživatelem.</param>
        /// <param name="fileSize">Velikost souboru v bajtech.</param>
        /// <returns>Optimální velikost bufferu.</returns>
        private static int DetermineBufferSize(int? userDefinedBufferSize, long fileSize)
        {
            const int DefaultBufferSize = 4096; // Defaultní velikost bufferu (4 KB)
            const int MaxBufferSize = 65536;   // Maximální velikost bufferu (64 KB)

            if (userDefinedBufferSize.HasValue)
            {
                return Math.Min(userDefinedBufferSize.Value, MaxBufferSize);
            }

            if (fileSize <= DefaultBufferSize)
            {
                return DefaultBufferSize;
            }

            return MaxBufferSize;
        }

        /// <summary>
        /// Validuje cestu k souboru.
        /// </summary>
        /// <param name="filePath">Cesta k souboru.</param>
        private void ValidateFilePath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                Logger.LogWarning("Cesta k souboru je prázdná nebo null.");
                throw new ArgumentException("Cesta k souboru nesmí být prázdná.", nameof(filePath));
            }

            if (!File.Exists(filePath))
            {
                Logger.LogWarning("Soubor '{FilePath}' nebyl nalezen.", filePath);
                throw new FileNotFoundException($"Soubor '{filePath}' nebyl nalezen.");
            }
        }

        /// <summary>
        /// Pomocná metoda pro získání přípony souboru.
        /// </summary>
        /// <param name="filePath">Cesta k souboru.</param>
        /// <returns>Přípona souboru (např. ".srt").</returns>
        private string GetFileExtension(string filePath)
        {
            return Path.GetExtension(filePath)?.ToLowerInvariant();
        }

        /// <summary>
        /// Nastaví kódování souboru.
        /// </summary>
        /// <param name="encoding">Nové kódování.</param>
        public void SetFileEncoding(Encoding encoding)
        {
            FileEncoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
            Logger.LogInformation("Kódování souboru bylo změněno na: {Encoding}", encoding.EncodingName);
        }

        /// <summary>
        /// Rozpoznávání časového intervalu v SRT formátu.
        /// </summary>
        /// <param name="timeRange">Časový interval ve formátu SRT.</param>
        /// <param name="startTime">Začátek časového intervalu.</param>
        /// <param name="endTime">Konec časového intervalu.</param>
        /// <returns>True pokud byl časový interval úspěšně rozpoznán.</returns>
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

        /// <summary>
        /// Rozpoznávání času v SRT formátu.
        /// </summary>
        /// <param name="timeString">Čas ve formátu SRT.</param>
        /// <param name="time">Výsledný čas.</param>
        /// <returns>True pokud byl čas úspěšně rozpoznán.</returns>
        private bool TryParseTime(string timeString, out TimeSpan time)
        {
            time = TimeSpan.Zero;

            var parts = timeString.Split(':');
            if (parts.Length != 3)
            {
                return false;
            }

            if (int.TryParse(parts[0], out int hours) &&
                int.TryParse(parts[1], out int minutes) &&
                TryParseMilliseconds(parts[2], out int milliseconds))
            {
                time = new TimeSpan(hours, minutes, 0, 0, milliseconds);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Rozpoznávání milisekund v SRT formátu.
        /// </summary>
        /// <param name="timeString">Čas ve formátu SRT.</param>
        /// <param name="milliseconds">Výsledné milisekundy.</param>
        /// <returns>True pokud byly milisekundy úspěšně rozpoznány.</returns>
        private bool TryParseMilliseconds(string timeString, out int milliseconds)
        {
            milliseconds = 0;

            var parts = timeString.Split(',');
            if (parts.Length != 2)
            {
                return false;
            }

            if (int.TryParse(parts[0], out int seconds) && int.TryParse(parts[1], out int millisecs))
            {
                milliseconds = seconds * 1000 + millisecs;
                return true;
            }

            return false;
        }
    }
}