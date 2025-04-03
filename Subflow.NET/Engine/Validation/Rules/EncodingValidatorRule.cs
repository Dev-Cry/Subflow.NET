using Microsoft.Extensions.Logging;
using Ruleflow.NET.Engine.Validation;
using Ruleflow.NET.Engine.Validation.Enums;
using Subflow.NET.Engine.Validation.Interfaces;
using System;
using System.IO;
using System.Text;

namespace Ruleflow.NET.Engine.Validation.Rules
{
    public class EncodingValidatorRule : BaseValidationRule<FileInfo>
    {
        private readonly ILogger<EncodingValidatorRule> _logger;

        public override ValidationSeverity DefaultSeverity => ValidationSeverity.Error;

        public EncodingValidatorRule(ILogger<EncodingValidatorRule> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override void Validate(FileInfo input)
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