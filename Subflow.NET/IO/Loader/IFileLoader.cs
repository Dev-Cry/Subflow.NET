using System;
using System.Collections.Generic;
using System.Text;

namespace SubFlow.NET.IO.Loaders
{
    public interface IFileLoader
    {
        /// <summary>
        /// Cesta k souboru.
        /// </summary>
        string FilePath { get; }

        /// <summary>
        /// Kódování souboru.
        /// </summary>
        Encoding FileEncoding { get; }

        /// <summary>
        /// Přípona souboru.
        /// </summary>
        string FileExtension { get; }

        /// <summary>
        /// Indikuje, zda byl soubor úspěšně načten.
        /// </summary>
        bool IsLoaded { get; }

        /// <summary>
        /// Asynchronně načte obsah souboru po řádcích.
        /// </summary>
        /// <param name="filePath">Cesta k souboru.</param>
        /// <param name="bufferSize">Volitelná velikost bufferu v bajtech.</param>
        /// <param name="degreeOfParallelism">Počet paralelních vláken pro zpracování.</param>
        /// <returns>Asynchronní enumerable pro iteraci přes řádky souboru.</returns>
        IAsyncEnumerable<string> LoadFileAsync(string filePath, int? bufferSize = null, int degreeOfParallelism = 4);

        /// <summary>
        /// Nastaví kódování souboru.
        /// </summary>
        /// <param name="encoding">Nové kódování.</param>
        void SetFileEncoding(Encoding encoding);
    }
}