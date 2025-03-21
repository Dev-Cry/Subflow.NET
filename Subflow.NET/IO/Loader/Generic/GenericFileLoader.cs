using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Subflow.NET.IO.Loader.Base;

namespace Subflow.NET.IO.Loader.Generic
{
    public class GenericFileLoader<TResult> : FileLoaderBase<TResult>
    {
        // Delegát pro zpracování jednoho řádku
        private readonly Func<string, Task<TResult>> _lineParser;

        /// <summary>
        /// Konstruktor pro vytvoření obecného file loaderu.
        /// </summary>
        /// <param name="logger">Instance loggeru.</param>
        /// <param name="lineParser">Funkce pro parsování jednoho řádku na TResult.</param>
        public GenericFileLoader(ILogger logger, Func<string, Task<TResult>> lineParser) : base(logger)
        {
            _lineParser = lineParser ?? throw new ArgumentNullException(nameof(lineParser));
        }

        /// <summary>
        /// Implementace abstraktní metody ParseLineAsync.
        /// </summary>
        /// <param name="line">Řádek ze souboru.</param>
        /// <returns>Výsledek typu TResult po zpracování řádku.</returns>
        protected override async Task<TResult> ParseLineAsync(string line)
        {
            return await _lineParser(line);
        }
    }
}