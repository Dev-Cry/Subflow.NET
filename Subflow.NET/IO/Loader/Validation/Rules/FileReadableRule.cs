using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subflow.NET.IO.Loader.Validation.Rules
{
    // Pravidlo: ověření, že soubor lze otevřít pro čtení
    public class FileReadableRule : IValidationRule<FileInfo>
    {
        private readonly ILogger<FileReadableRule> _logger;

        public FileReadableRule(ILogger<FileReadableRule> logger)
        {
            _logger = logger;
        }

        public void Validate(FileInfo input)
        {
            try
            {
                using var stream = input.OpenRead();
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "Soubor '{Path}' nelze otevřít pro čtení.", input.FullName);
                throw new IOException($"Soubor '{input.FullName}' nelze otevřít pro čtení: {ex.Message}", ex);
            }
        }
    }
}