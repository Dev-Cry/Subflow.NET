using Subflow.NET.Engine.Validation.Enums;

namespace Subflow.NET.Engine.Validation
{
    public class ValidationError
    {
        public string Message { get; }
        public ValidationSeverity Severity { get; }
        public string? Code { get; }
        public object? Context { get; }

        public ValidationError(string message, ValidationSeverity severity = ValidationSeverity.Error, string? code = null, object? context = null)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
            Severity = severity;
            Code = code;
            Context = context;
        }

        public override string ToString() => $"[{Severity}] {(Code != null ? $"({Code}) " : "")}{Message}";
    }
}