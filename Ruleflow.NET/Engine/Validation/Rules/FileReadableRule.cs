using Microsoft.Extensions.Logging;
using Ruleflow.NET.Engine.Validation.Core.Base;
using Ruleflow.NET.Engine.Validation.Enums;
using Ruleflow.NET.Engine.Validation.Interfaces;
using System;
using System.IO;

namespace Ruleflow.NET.Engine.Validation.Rules
{
    // Pravidlo: ověření, že soubor lze otevřít pro čtení
    public class FileReadableRule : BaseValidationRule<FileInfo>
    {
        private readonly ILogger<FileReadableRule> _logger;

        public override ValidationSeverity DefaultSeverity => ValidationSeverity.Error;

        public FileReadableRule(ILogger<FileReadableRule> logger)
        {
            _logger = logger;
        }

        public override void Validate(FileInfo input)
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