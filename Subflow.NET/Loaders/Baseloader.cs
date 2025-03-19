using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Subflow.NET.Loaders.Enums;

namespace SubFlow.NET.Loaders
{
    /// <summary>
    /// Základní abstraktní třída pro všechny loadery.
    /// </summary>
    /// <typeparam name="T">Typ dat, které loader načítá (např. SubtitleFile).</typeparam>
    public abstract class Baseloader<T>(ILogger logger, IOptions<LoaderOptions> options)
    {
        /// <summary>
        /// Logger pro zaznamenávání událostí.
        /// </summary>
        protected ILogger Logger { get; } = logger ?? throw new ArgumentNullException(nameof(logger));

        /// <summary>
        /// Konfigurace loaderu.
        /// </summary>
        protected LoaderOptions Options { get; } = options?.Value ?? throw new ArgumentNullException(nameof(options));

        /// <summary>
        /// Načte data ze souboru.
        /// </summary>
        /// <param name="filePath">Cesta k souboru.</param>
        /// <returns>Vrátí načtená data typu T.</returns>
        public async Task<T> LoadAsync(string filePath)
        {
            // Normalizace cesty
            if (Options.NormalizePath)
            {
                filePath = Path.GetFullPath(filePath);
            }

            // Validace cesty k souboru
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Cesta k souboru nesmí být prázdná.", nameof(filePath));

            // Kontrola povolených přípon
            var extension = Path.GetExtension(filePath);
            if (!Options.AllowedExtensions.Contains(extension))
            {
                Logger.LogError("Soubor '{FilePath}' má nepovolenou příponu '{Extension}'. Povolené přípony jsou: {AllowedExtensions}.",
                    filePath, extension, string.Join(", ", Options.AllowedExtensions));
                throw new InvalidOperationException($"Soubor '{filePath}' má nepovolenou příponu.");
            }

            // Ověření síťové cesty
            if (!Options.AllowNetworkPaths && IsNetworkPath(filePath))
            {
                Logger.LogError("Síťové cesty nejsou povoleny: {FilePath}", filePath);
                throw new InvalidOperationException("Síťové cesty nejsou povoleny.");
            }

            // Mechanismus opakování
            for (int attempt = 1; attempt <= Options.MaxRetryAttempts; attempt++)
            {
                try
                {
                    Logger.LogInformation("Načítání souboru: {FilePath} (Pokus {Attempt}/{MaxAttempts})",
                        filePath, attempt, Options.MaxRetryAttempts);

                    // Kontrola existence souboru
                    if (!File.Exists(filePath))
                    {
                        Logger.LogError("Soubor '{FilePath}' nebyl nalezen.", filePath);
                        throw new FileNotFoundException($"Soubor '{filePath}' nebyl nalezen.", filePath);
                    }

                    // Ověření přístupových práv
                    if (!HasReadAccessToPath(filePath))
                    {
                        Logger.LogError("Nelze číst soubor '{FilePath}' kvůli nedostatečným oprávněním.", filePath);
                        throw new UnauthorizedAccessException($"Nedostatečná oprávnění pro čtení souboru '{filePath}'.");
                    }

                    // Ověření velikosti souboru
                    var fileInfo = new FileInfo(filePath);
                    if (fileInfo.Length > Options.MaxFileSize)
                    {
                        Logger.LogError("Soubor '{FilePath}' je příliš velký. Maximální povolená velikost je {MaxFileSize} bajtů.",
                            filePath, Options.MaxFileSize);
                        throw new InvalidOperationException($"Soubor '{filePath}' je příliš velký.");
                    }

                    // Kontrola integrity souboru
                    if (Options.CheckFileIntegrity && !VerifyFileIntegrity(filePath, Options.ExpectedHash))
                    {
                        Logger.LogError("Integrita souboru '{FilePath}' se neshoduje s očekávaným hashem.", filePath);
                        throw new InvalidOperationException($"Integrita souboru '{filePath}' není v pořádku.");
                    }

                    // Čtení obsahu souboru
                    Encoding encoding = Options.AutoDetectEncoding ? DetectEncoding(filePath) : Options.Encoding;
                    var content = await File.ReadAllTextAsync(filePath, encoding);

                    Logger.LogInformation("Soubor úspěšně načten. Velikost: {ContentSize} znaků.", content.Length);

                    // Validace obsahu
                    if (Options.ValidateContent && string.IsNullOrWhiteSpace(content))
                    {
                        Logger.LogWarning("Obsah souboru '{FilePath}' je prázdný nebo obsahuje pouze bílé znaky.", filePath);
                        throw new InvalidOperationException("Obsah souboru je neplatný.");
                    }

                    return await LoadFromStringAsync(content);
                }
                catch (Exception ex) when (attempt < Options.MaxRetryAttempts && !Options.IgnoreErrors)
                {
                    Logger.LogWarning(ex, "Chyba při načítání souboru '{FilePath}' (Pokus {Attempt}/{MaxAttempts}). Opakuji za {Delay}.",
                        filePath, attempt, Options.MaxRetryAttempts, Options.RetryDelay);

                    await Task.Delay(Options.RetryDelay);
                }
            }

            Logger.LogError("Načítání souboru '{FilePath}' selhalo po {MaxAttempts} pokusech.", filePath, Options.MaxRetryAttempts);
            throw new InvalidOperationException($"Načítání souboru '{filePath}' selhalo.");
        }

        /// <summary>
        /// Načte data z řetězce.
        /// </summary>
        /// <param name="content">Obsah souboru jako řetězec.</param>
        /// <returns>Vrátí načtená data typu T.</returns>
        public abstract Task<T> LoadFromStringAsync(string content);

        /// <summary>
        /// Zkontroluje, zda má aplikace oprávnění ke čtení souboru.
        /// </summary>
        /// <param name="filePath">Cesta k souboru.</param>
        /// <returns>Vrací true, pokud má oprávnění ke čtení.</returns>
        private bool HasReadAccessToPath(string filePath)
        {
            try
            {
                using (FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    return true;
                }
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
            catch (IOException)
            {
                return false;
            }
        }

        /// <summary>
        /// Detekuje kódování souboru.
        /// </summary>
        /// <param name="filePath">Cesta k souboru.</param>
        /// <returns>Detekované kódování.</returns>
        private Encoding DetectEncoding(string filePath)
        {
            using (var reader = new StreamReader(filePath, detectEncodingFromByteOrderMarks: true))
            {
                reader.Peek(); // Nutné pro detekci kódování
                return reader.CurrentEncoding;
            }
        }

        /// <summary>
        /// Ověří integritu souboru pomocí hash funkce.
        /// </summary>
        /// <param name="filePath">Cesta k souboru.</param>
        /// <param name="expectedHash">Očekávaný hash souboru.</param>
        /// <returns>Vrací true, pokud je integrita v pořádku.</returns>
        private bool VerifyFileIntegrity(string filePath, string? expectedHash)
        {
            if (string.IsNullOrEmpty(expectedHash))
                return true;

            using var sha256 = SHA256.Create();
            using var stream = File.OpenRead(filePath);
            var hash = BitConverter.ToString(sha256.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();

            return hash == expectedHash.ToLowerInvariant();
        }

        /// <summary>
        /// Zjistí, zda je cesta síťová.
        /// </summary>
        /// <param name="filePath">Cesta k souboru.</param>
        /// <returns>Vrací true, pokud je cesta síťová.</returns>
        private bool IsNetworkPath(string filePath)
        {
            return filePath.StartsWith(@"\\", StringComparison.Ordinal);
        }
    }

    /// <summary>
    /// Konfigurace loaderu.
    /// </summary>
    public class LoaderOptions
    {
        /// <summary>
        /// Kódování použité pro čtení souborů.
        /// </summary>
        public Encoding Encoding { get; set; } = Encoding.UTF8;

        /// <summary>
        /// Maximální velikost souboru v bajtech.
        /// </summary>
        public long MaxFileSize { get; set; } = 10 * 1024 * 1024; // Default: 10 MB

        /// <summary>
        /// Povolit načítání souborů ze síťových cest.
        /// </summary>
        public bool AllowNetworkPaths { get; set; } = false;

        /// <summary>
        /// Časový limit pro načítání souboru.
        /// </summary>
        public TimeSpan? Timeout { get; set; }

        /// <summary>
        /// Automaticky detekovat kódování souboru.
        /// </summary>
        public bool AutoDetectEncoding { get; set; } = false;

        /// <summary>
        /// Povolit mezipaměť pro zlepšení výkonu.
        /// </summary>
        public bool EnableCaching { get; set; } = true;

        /// <summary>
        /// Validovat obsah souboru (např. prázdnost).
        /// </summary>
        public bool ValidateContent { get; set; } = true;

        /// <summary>
        /// Normalizovat cestu k souboru.
        /// </summary>
        public bool NormalizePath { get; set; } = true;

        /// <summary>
        /// Kontrolovat integritu souboru pomocí hash.
        /// </summary>
        public bool CheckFileIntegrity { get; set; } = false;
        public string? ExpectedHash { get; set; } = null;

        /// <summary>
        /// Ignorovat chyby při načítání souboru.
        /// </summary>
        public bool IgnoreErrors { get; set; } = false;

        /// <summary>
        /// Maximální počet pokusů o načtení souboru.
        /// </summary>
        public int MaxRetryAttempts { get; set; } = 3;

        /// <summary>
        /// Prodleva mezi pokusy o načtení souboru.
        /// </summary>
        public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(1);

        /// <summary>
        /// Dekomprimovat komprimované soubory (např. .zip, .gz).
        /// </summary>
        public bool DecompressFiles { get; set; } = false;

        /// <summary>
        /// Seznam podporovaných formátů.
        /// </summary>
        public HashSet<FileFormat> AllowedFormats { get; set; } = new()
        {
        FileFormat.SRT, FileFormat.VTT, FileFormat.ASS // Defaultní podporované formáty
        };

        /// <summary>
        /// Seznam povolených přípon generovaný z podporovaných formátů.
        /// </summary>
        public HashSet<string> AllowedExtensions => FileFormatExtensions.GetAllowedExtensions(AllowedFormats);
    }
}