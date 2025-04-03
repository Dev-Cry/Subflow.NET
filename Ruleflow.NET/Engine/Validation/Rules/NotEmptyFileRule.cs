using Microsoft.Extensions.Logging;
using Ruleflow.NET.Engine.Validation.Core.Base;
using Ruleflow.NET.Engine.Validation.Enums;
using Ruleflow.NET.Engine.Validation.Interfaces;
using System;
using System.IO;

namespace Ruleflow.NET.Engine.Validation.Rules
{
    // Pravidlo: kontrola, že soubor není prázdný
    public class NotEmptyFileRule : BaseValidationRule<FileInfo>
    {
        private readonly ILogger<NotEmptyFileRule> _logger;

        public override ValidationSeverity DefaultSeverity => ValidationSeverity.Warning;

        public NotEmptyFileRule(ILogger<NotEmptyFileRule> logger)
        {
            _logger = logger;
        }

        public override void Validate(FileInfo input)
        {
            if (input.Length == 0)
            {
                _logger.LogWarning("Soubor '{Path}' je prázdný.", input.FullName);
                throw new InvalidDataException($"Soubor '{input.FullName}' je prázdný.");
            }
        }
    }
}