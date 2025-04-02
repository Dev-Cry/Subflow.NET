using Microsoft.Extensions.Logging;
using Subflow.NET.Engine.Validation.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subflow.NET.Engine.Validation.Rules
{
    // Pravidlo: kontrola prázdné cesty
    public class NotEmptyPathRule : IValidationRule<string>
    {
        private readonly ILogger<NotEmptyPathRule> _logger;

        public NotEmptyPathRule(ILogger<NotEmptyPathRule> logger)
        {
            _logger = logger;
        }

        public void Validate(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                _logger.LogWarning("Cesta k souboru je prázdná nebo null.");
                throw new ArgumentException("Cesta k souboru nesmí být prázdná.", nameof(input));
            }
        }
    }
}
