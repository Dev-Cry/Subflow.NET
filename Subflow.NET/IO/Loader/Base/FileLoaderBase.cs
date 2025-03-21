using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Logging;

namespace Subflow.NET.IO.Loader.Base
{
    public abstract class FileLoaderBase<TResult>
    {
        // Vlastnosti
        public string FilePath { get; private set; } // Cesta k souboru
        public Encoding FileEncoding { get; private set; } = Encoding.UTF8; // Kódování souboru
        public string FileExtension => GetFileExtension(FilePath); // Přípona souboru
        public bool IsLoaded { get; private set; } // Stav načítání

        // Logger
        protected ILogger Logger { get; }

        /// <summary>
        /// Konstruktor pro injektování loggeru.
        /// </summary>
        /// <param name="logger">Instance loggeru.</param>
        protected FileLoaderBase(ILogger logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Asynchronní metoda pro načítání souboru a transformaci řádků na typ TResult.
        /// </summary>
        /// <param name="filePath">Cesta k souboru.</param>
        /// <param name="bufferSize">Volitelná velikost bufferu.</param>
        /// <param name="degreeOfParallelism">Počet paralelních vláken pro zpracování.</param>
        /// <returns>Asynchronní posloupnost výsledků typu TResult.</returns>
        public virtual async IAsyncEnumerable<TResult> LoadFileAsync(string filePath, int? bufferSize = null, int degreeOfParallelism = 4)
        {
            // Validace cesty
            ValidateFilePath(filePath);

            // Nastavení FilePath
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

            // Určení velikosti bufferu
            int effectiveBufferSize = DetermineBufferSize(bufferSize, fileSize);

            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: effectiveBufferSize, useAsync: true);
            using var reader = new StreamReader(stream, FileEncoding);

            IsLoaded = true;

            var buffer = new char[effectiveBufferSize];
            var leftover = string.Empty; // Zbývající data z předchozího bloku
            int bytesRead;

            // Vytvoření pipeline pro paralelní zpracování
            var processingBlock = new TransformBlock<string, TResult>(
                async line =>
                {
                    // Transformace řádku na TResult pomocí abstraktní metody ParseLine
                    return await ParseLineAsync(line.TrimEnd('\r'));
                },
                new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = degreeOfParallelism // Počet paralelních vláken
                });

            var outputQueue = new BufferBlock<TResult>();

            // Propojení bloků
            processingBlock.LinkTo(outputQueue, new DataflowLinkOptions { PropagateCompletion = true });

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
                yield return await outputQueue.ReceiveAsync();
            }

            Logger.LogInformation("Soubor úspěšně načten asynchronně.");
        }

        /// <summary>
        /// Abstraktní metoda pro transformaci řádku na výsledný typ TResult.
        /// Tuto metodu musí implementovat konkrétní potomek.
        /// </summary>
        /// <param name="line">Řádek ze souboru.</param>
        /// <returns>Výsledek typu TResult.</returns>
        protected abstract Task<TResult> ParseLineAsync(string line);

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
        protected void ValidateFilePath(string filePath)
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
        protected string GetFileExtension(string filePath)
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
    }
}