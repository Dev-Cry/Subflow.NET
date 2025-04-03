using Ruleflow.NET.Engine.Validation;
using Ruleflow.NET.Engine.Validation.Enums;

namespace Ruleflow.NET.Engine.Validation.Interfaces
{
    public interface IValidationResult
    {
        bool IsValid { get; }
        bool HasCriticalErrors { get; }
        IReadOnlyList<ValidationError> Errors { get; }
        void AddError(ValidationError error);
        void AddError(string message, ValidationSeverity severity = ValidationSeverity.Error, string? code = null, object? context = null);
        void AddErrors(IEnumerable<ValidationError> errors);
        IEnumerable<ValidationError> GetErrorsBySeverity(ValidationSeverity severity);
    }
}