using Microsoft.Extensions.Logging;
using Subflow.NET.IO.Loader.Validation.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subflow.NET.IO.Loader.Validation
{
    public class ValidatorFactory : IValidatorFactory
    {
        private readonly ILoggerFactory _loggerFactory;

        public ValidatorFactory(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public IValidator<string> CreatePathValidator()
        {
            return new CompositeValidator<string>(new IValidator<string>[]
            {
            new Validator<string>(new IValidationRule<string>[]
            {
                new NotEmptyPathRule(_loggerFactory.CreateLogger<NotEmptyPathRule>()),
                new AbsolutePathRule(_loggerFactory.CreateLogger<AbsolutePathRule>()),
                new PathLengthRule(_loggerFactory.CreateLogger<PathLengthRule>(), 260),
                new FileExistsRule(_loggerFactory.CreateLogger<FileExistsRule>())
            })
            });
        }

        public IValidator<FileInfo> CreateFileValidator()
        {
            return new CompositeValidator<FileInfo>(new IValidator<FileInfo>[]
            {
            new Validator<FileInfo>(new IValidationRule<FileInfo>[]
            {
                new NotDirectoryRule(_loggerFactory.CreateLogger<NotDirectoryRule>()),
                new NotEmptyFileRule(_loggerFactory.CreateLogger<NotEmptyFileRule>()),
                new ExtensionAllowedRule(_loggerFactory.CreateLogger<ExtensionAllowedRule>(), new[] { ".srt", ".vtt" }),
                new MaxFileSizeRule(_loggerFactory.CreateLogger<MaxFileSizeRule>(), 100 * 1024 * 1024),
                new FileReadableRule(_loggerFactory.CreateLogger<FileReadableRule>())
            })
            });
        }
    }
}
