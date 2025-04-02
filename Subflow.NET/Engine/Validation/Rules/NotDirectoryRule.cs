using Microsoft.Extensions.Logging;
using Subflow.NET.Engine.Validation.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subflow.NET.Engine.Validation.Rules
{
    // Pravidlo: kontrola, že cesta nevede na adresář
    public class NotDirectoryRule : IValidationRule<FileInfo>
    {
        private readonly ILogger<NotDirectoryRule> _logger;

        public NotDirectoryRule(ILogger<NotDirectoryRule> logger)
        {
            _logger = logger;
        }

        public void Validate(FileInfo input)
        {
            if ((input.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
            {
                _logger.LogWarning("Cesta '{Path}' odkazuje na adresář.", input.FullName);
                throw new ArgumentException($"Cesta '{input.FullName}' odkazuje na adresář.");
            }
        }
    }
}
