using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subflow.NET.IO.Loader
{
    public class FilePathValidator
    {
        private readonly ILogger<FilePathValidator> _logger;

        public FilePathValidator(ILogger<FilePathValidator> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Validate(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                _logger.LogWarning("Cesta k souboru je prázdná nebo null.");
                throw new ArgumentException("Cesta k souboru nesmí být prázdná.", nameof(filePath));
            }
            if (!File.Exists(filePath))
            {
                _logger.LogWarning("Soubor '{FilePath}' nebyl nalezen.", filePath);
                throw new FileNotFoundException($"Soubor '{filePath}' nebyl nalezen.");
            }
        }
    }
}
