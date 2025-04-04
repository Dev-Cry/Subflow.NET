﻿using Microsoft.Extensions.Logging;
using Ruleflow.NET.Engine.Validation.Core.Base;
using Ruleflow.NET.Engine.Validation.Enums;
using Ruleflow.NET.Engine.Validation.Interfaces;
using System;

namespace Ruleflow.NET.Engine.Validation.Rules
{
    // Pravidlo: kontrola prázdné cesty
    public class NotEmptyPathRule : BaseValidationRule<string>
    {
        private readonly ILogger<NotEmptyPathRule> _logger;

        public override ValidationSeverity DefaultSeverity => ValidationSeverity.Critical;

        public NotEmptyPathRule(ILogger<NotEmptyPathRule> logger)
        {
            _logger = logger;
        }

        public override void Validate(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                _logger.LogCritical("Cesta k souboru je prázdná nebo null.");
                throw new ArgumentException("Cesta k souboru nesmí být prázdná.", nameof(input));
            }
        }
    }
}