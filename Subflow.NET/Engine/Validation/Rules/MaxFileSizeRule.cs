using Microsoft.Extensions.Logging;
using Subflow.NET.Engine.Validation.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subflow.NET.Engine.Validation.Rules
{
    // Pravidlo: kontrola maximální velikosti souboru
    public class MaxFileSizeRule : IValidationRule<FileInfo>
    {
        private readonly ILogger<MaxFileSizeRule> _logger;
        private readonly long _maxBytes;

        public MaxFileSizeRule(ILogger<MaxFileSizeRule> logger, long maxBytes = 100 * 1024 * 1024)
        {
            _logger = logger;
            _maxBytes = maxBytes;
        }

        public void Validate(FileInfo input)
        {
            if (input.Length > _maxBytes)
            {
                _logger.LogWarning("Soubor '{Path}' překračuje maximální velikost {Max} B.", input.FullName, _maxBytes);
                throw new InvalidOperationException($"Soubor '{input.FullName}' je příliš velký (max. {_maxBytes / 1024 / 1024} MB).");
            }
        }
    }
}
