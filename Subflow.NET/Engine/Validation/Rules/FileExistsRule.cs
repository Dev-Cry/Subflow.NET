using Microsoft.Extensions.Logging;
using Ruleflow.NET.Engine.Validation;
using Ruleflow.NET.Engine.Validation.Enums;
using Ruleflow.NET.Engine.Validation.Interfaces;
using System;
using System.IO;

namespace Ruleflow.NET.Engine.Validation.Rules
{
    // Pravidlo: kontrola existence souboru
    public class FileExistsRule : BaseValidationRule<string>
    {
        private readonly ILogger<FileExistsRule> _logger;

        public override ValidationSeverity DefaultSeverity => ValidationSeverity.Error;

        public FileExistsRule(ILogger<FileExistsRule> logger)
        {
            _logger = logger;
        }

        public override void Validate(string input)
        {
            if (!File.Exists(input))
            {
                _logger.LogError("Soubor '{Path}' nebyl nalezen.", input);
                throw new FileNotFoundException($"Soubor '{input}' nebyl nalezen.");
            }
        }
    }
}