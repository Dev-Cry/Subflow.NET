namespace Ruleflow.NET.Engine.Validation
{
    public class ValidationException : Exception
    {
        public ValidationError ValidationError { get; }

        public ValidationException(string message, ValidationError validationError)
            : base(message)
        {
            ValidationError = validationError;
        }

        public ValidationException(string message, ValidationError validationError, Exception innerException)
            : base(message, innerException)
        {
            ValidationError = validationError;
        }
    }
}