using Microsoft.Extensions.Logging;
using Subflow.NET.Engine.Validation.Enums;
using Subflow.NET.Engine.Validation.Interfaces;
using System;
using System.IO;

namespace Subflow.NET.Engine.Validation.Rules
{
    // Pravidlo: kontrola, že cesta nevede na adresář
    public class NotDirectoryRule : BaseValidationRule<FileInfo>
    {
        private readonly ILogger<NotDirectoryRule> _logger;

        public override ValidationSeverity DefaultSeverity => ValidationSeverity.Error;

        public NotDirectoryRule(ILogger<NotDirectoryRule> logger)
        {
            _logger = logger;
        }

        public override void Validate(FileInfo input)
        {
            if ((input.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
            {
                _logger.LogWarning("Cesta '{Path}' odkazuje na adresář.", input.FullName);
                throw new ArgumentException($"Cesta '{input.FullName}' odkazuje na adresář.");
            }
        }
    }
}