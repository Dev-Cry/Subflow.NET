using Microsoft.Extensions.Logging;
using Subflow.NET.Engine.Validation.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subflow.NET.Engine.Validation.Rules
{
    // Pravidlo: kontrola délky cesty
    public class PathLengthRule : IValidationRule<string>
    {
        private readonly ILogger<PathLengthRule> _logger;
        private readonly int _maxLength;

        public PathLengthRule(ILogger<PathLengthRule> logger, int maxLength = 260)
        {
            _logger = logger;
            _maxLength = maxLength;
        }

        public void Validate(string input)
        {
            if (input.Length > _maxLength)
            {
                _logger.LogWarning("Cesta k souboru je příliš dlouhá. Délka: {Length}, Max: {Max}", input.Length, _maxLength);
                throw new PathTooLongException($"Cesta k souboru nesmí přesáhnout {_maxLength} znaků.");
            }
        }
    }
}
