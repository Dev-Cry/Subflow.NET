using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subflow.NET.IO.Loader.Validation.Rules
{
    // Pravidlo: kontrola absolutní cesty
    public class AbsolutePathRule : IValidationRule<string>
    {
        private readonly ILogger<AbsolutePathRule> _logger;

        public AbsolutePathRule(ILogger<AbsolutePathRule> logger)
        {
            _logger = logger;
        }

        public void Validate(string input)
        {
            if (!Path.IsPathRooted(input))
            {
                _logger.LogWarning("Cesta '{Path}' není absolutní.", input);
                throw new ArgumentException($"Cesta k souboru musí být absolutní. Zadáno: '{input}'", nameof(input));
            }
        }
    }
}
