using Microsoft.Extensions.Logging;
using Subflow.NET.Engine.Validation.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subflow.NET.Engine.Validation.Rules
{
    // Pravidlo: kontrola, že soubor není prázdný
    public class NotEmptyFileRule : IValidationRule<FileInfo>
    {
        private readonly ILogger<NotEmptyFileRule> _logger;

        public NotEmptyFileRule(ILogger<NotEmptyFileRule> logger)
        {
            _logger = logger;
        }

        public void Validate(FileInfo input)
        {
            if (input.Length == 0)
            {
                _logger.LogWarning("Soubor '{Path}' je prázdný.", input.FullName);
                throw new InvalidDataException($"Soubor '{input.FullName}' je prázdný.");
            }
        }
    }
}
