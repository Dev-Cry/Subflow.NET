using Ruleflow.NET.Engine.Validation.Enums;
using Ruleflow.NET.Engine.Validation.Interfaces;

namespace Ruleflow.NET.Engine.Validation
{
    public class ValidationResult : IValidationResult
    {
        private readonly List<ValidationError> _errors = new();

        public bool IsValid => !_errors.Any(e => e.Severity >= ValidationSeverity.Error);
        public bool HasCriticalErrors => _errors.Any(e => e.Severity == ValidationSeverity.Critical);
        public IReadOnlyList<ValidationError> Errors => _errors.AsReadOnly();

        public void AddError(ValidationError error)
        {
            if (error == null) throw new ArgumentNullException(nameof(error));
            _errors.Add(error);
        }

        public void AddError(string message, ValidationSeverity severity = ValidationSeverity.Error, string? code = null, object? context = null)
        {
            AddError(new ValidationError(message, severity, code, context));
        }

        public void AddErrors(IEnumerable<ValidationError> errors)
        {
            foreach (var error in errors)
            {
                AddError(error);
            }
        }

        public IEnumerable<ValidationError> GetErrorsBySeverity(ValidationSeverity severity)
        {
            return _errors.Where(e => e.Severity == severity);
        }
    }
}