using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;

namespace Subflow.NET.IO.Loader.Validation.Rules
{
    public class EncodingValidatorRule : IValidationRule<FileInfo>
    {
        private readonly ILogger<EncodingValidatorRule> _logger;

        public EncodingValidatorRule(ILogger<EncodingValidatorRule> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Validate(FileInfo input)
        {
            if (!input.Exists)
                throw new FileNotFoundException("Soubor neexistuje.", input.FullName);

            using var stream = File.OpenRead(input.FullName);
            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);

            reader.Peek(); // Nutné pro inicializaci CurrentEncoding
            var encoding = reader.CurrentEncoding;

            _logger.LogInformation("Detekováno kódování: {EncodingName}", encoding.EncodingName);

            if (encoding != Encoding.UTF8 && encoding != Encoding.Unicode)
            {
                throw new InvalidOperationException($"Nepodporované kódování: {encoding.EncodingName}. Povolené jsou pouze UTF-8 a UTF-16.");
            }
        }
    }
}
