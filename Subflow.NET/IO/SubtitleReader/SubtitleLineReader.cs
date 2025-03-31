using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Subflow.NET.IO.SubtitleReader
{
    /// <summary>
    /// Poskytuje asynchronní čtení řádků ze souboru s titulky.
    /// Optimalizováno pro sekvenční přístup, podporuje zrušení přes CancellationToken.
    /// </summary>
    public class SubtitleLineReader : ISubtitleLineReader
    {
        private readonly string _filePath;
        private readonly Encoding _fileEncoding;


        /// <summary>
        /// Inicializuje nový FileReader pro specifikovaný soubor a kódování.
        /// </summary>
        /// <param name="filePath">Cesta k souboru.</param>
        /// <param name="fileEncoding">Kódování souboru (např. UTF-8, Windows-1250).</param>
        public SubtitleLineReader(string filePath, Encoding fileEncoding)
        {
            _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            _fileEncoding = fileEncoding ?? throw new ArgumentNullException(nameof(fileEncoding));
        }

        /// <summary>
        /// Asynchronně čte soubor s titulky po řádcích a vrací je jako IAsyncEnumerable.
        /// </summary>
        /// <param name="bufferSize">Velikost bufferu v bajtech pro čtení.</param>
        /// <param name="cancellationToken">Token pro zrušení operace čtení.</param>
        /// <returns>Asynchronní enumerátor jednotlivých textových řádků.</returns>
        public async IAsyncEnumerable<string> ReadFileLinesAsync(int bufferSize,[EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            // Moderní způsob inicializace FileStreamu (od .NET 6) pomocí FileStreamOptions
            FileStreamOptions options = new()
            {
                Access = FileAccess.Read, // Soubor otevřen jen pro čtení
                Mode = FileMode.Open, // Soubor musí existovat
                Share = FileShare.Read, // Umožní čtení i z jiných procesů
                BufferSize = bufferSize, // Uživatelsky definovaná velikost bufferu
                Options = FileOptions.Asynchronous | FileOptions.SequentialScan
                // Asynchronous = podpora async I/O
                // SequentialScan = optimalizace pro sekvenční čtení (lepší výkon)
            };

            // Otevře se stream s definovanými volbami
            using var stream = new FileStream(_filePath, options);

            // StreamReader pro čtení textového obsahu ze streamu
            // detectEncodingFromByteOrderMarks = pokusí se detekovat BOM (např. UTF-8 vs UTF-16)
            using var reader = new StreamReader(stream,_fileEncoding,detectEncodingFromByteOrderMarks: true,bufferSize,leaveOpen: false);

            // Smyčka čte řádek po řádku, dokud není konec souboru
            while (!reader.EndOfStream)
            {
                // Umožňuje přerušení čtení zvenku (např. při zrušení požadavku)
                cancellationToken.ThrowIfCancellationRequested();

                // Čte další řádek asynchronně, neblokuje vlákno
                var line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);

                // Vrací čtený řádek, pokud není null (může být na konci souboru)
                if (line is not null)
                {
                    yield return line;
                }
            }
        }
    }
}
