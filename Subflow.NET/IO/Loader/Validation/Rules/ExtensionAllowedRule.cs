using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subflow.NET.IO.Loader.Validation.Rules
{
    // Pravidlo: kontrola podporované přípony
    public class ExtensionAllowedRule : IValidationRule<FileInfo>
    {
        private readonly ILogger<ExtensionAllowedRule> _logger;
        private readonly string[] _allowedExtensions;

        public ExtensionAllowedRule(ILogger<ExtensionAllowedRule> logger, string[]? allowedExtensions = null)
        {
            _logger = logger;
            _allowedExtensions = allowedExtensions ?? new[] { ".srt" };
        }

        public void Validate(FileInfo input)
        {
            if (!_allowedExtensions.Contains(input.Extension, StringComparer.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Soubor '{Path}' má nepodporovanou příponu.", input.FullName);
                throw new NotSupportedException($"Soubor s příponou '{input.Extension}' není podporován.");
            }
        }
    }
}
