using Microsoft.Extensions.Logging;
using Ruleflow.NET.Engine.Validation;
using Ruleflow.NET.Engine.Validation.Enums;
using Subflow.NET.Engine.Validation.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ruleflow.NET.Engine.Validation.Rules
{
    // Pravidlo: kontrola podporované přípony
    public class ExtensionAllowedRule : BaseValidationRule<FileInfo>
    {
        private readonly ILogger<ExtensionAllowedRule> _logger;
        private readonly string[] _allowedExtensions;

        public override ValidationSeverity DefaultSeverity => ValidationSeverity.Error;

        public ExtensionAllowedRule(ILogger<ExtensionAllowedRule> logger, string[]? allowedExtensions = null)
        {
            _logger = logger;
            _allowedExtensions = allowedExtensions ?? new[] { ".srt" };
        }

        public override void Validate(FileInfo input)
        {
            if (!_allowedExtensions.Contains(input.Extension, StringComparer.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Soubor '{Path}' má nepodporovanou příponu.", input.FullName);
                throw new NotSupportedException($"Soubor s příponou '{input.Extension}' není podporován.");
            }
        }
    }
}