using Microsoft.Extensions.Logging;
using Subflow.NET.Engine.Validation.Enums;
using Subflow.NET.Engine.Validation.Interfaces;
using System;
using System.IO;

namespace Subflow.NET.Engine.Validation.Rules
{
    // Pravidlo: kontrola absolutní cesty
    public class AbsolutePathRule : BaseValidationRule<string>
    {
        private readonly ILogger<AbsolutePathRule> _logger;

        public override ValidationSeverity DefaultSeverity => ValidationSeverity.Warning;

        public AbsolutePathRule(ILogger<AbsolutePathRule> logger)
        {
            _logger = logger;
        }

        public override void Validate(string input)
        {
            if (!Path.IsPathRooted(input))
            {
                _logger.LogWarning("Cesta '{Path}' není absolutní.", input);
                throw new ArgumentException($"Cesta k souboru musí být absolutní. Zadáno: '{input}'", nameof(input));
            }
        }
    }
}