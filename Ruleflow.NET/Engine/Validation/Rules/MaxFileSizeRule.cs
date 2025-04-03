using Microsoft.Extensions.Logging;
using Ruleflow.NET.Engine.Validation.Core.Base;
using Ruleflow.NET.Engine.Validation.Enums;
using Ruleflow.NET.Engine.Validation.Interfaces;
using System;

namespace Ruleflow.NET.Engine.Validation.Rules
{
    // Pravidlo: kontrola maximální velikosti souboru
    public class MaxFileSizeRule : BaseValidationRule<FileInfo>
    {
        private readonly ILogger<MaxFileSizeRule> _logger;
        private readonly long _maxBytes;

        public override ValidationSeverity DefaultSeverity => ValidationSeverity.Warning;

        public MaxFileSizeRule(ILogger<MaxFileSizeRule> logger, long maxBytes = 100 * 1024 * 1024)
        {
            _logger = logger;
            _maxBytes = maxBytes;
        }

        public override void Validate(FileInfo input)
        {
            if (input.Length > _maxBytes)
            {
                _logger.LogWarning("Soubor '{Path}' překračuje maximální velikost {Max} B.", input.FullName, _maxBytes);
                throw new InvalidOperationException($"Soubor '{input.FullName}' je příliš velký (max. {_maxBytes / 1024 / 1024} MB).");
            }
        }
    }
}