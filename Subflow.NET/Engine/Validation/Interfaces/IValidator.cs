using Subflow.NET.Engine.Validation.Enums;

namespace Subflow.NET.Engine.Validation.Interfaces
{
    public interface IValidator<T>
    {
        void Validate(T input, ValidationMode mode = ValidationMode.ThrowOnError);
        IValidationResult ValidateWithResult(T input);
    }
}