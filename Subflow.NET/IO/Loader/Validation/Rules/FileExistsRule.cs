using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subflow.NET.IO.Loader.Validation.Rules
{
    // Pravidlo: kontrola existence souboru
    public class FileExistsRule : IValidationRule<string>
    {
        private readonly ILogger<FileExistsRule> _logger;

        public FileExistsRule(ILogger<FileExistsRule> logger)
        {
            _logger = logger;
        }

        public void Validate(string input)
        {
            if (!File.Exists(input))
            {
                _logger.LogWarning("Soubor '{Path}' nebyl nalezen.", input);
                throw new FileNotFoundException($"Soubor '{input}' nebyl nalezen.");
            }
        }
    }
}
