using Subflow.NET.Engine.Validation.Enums;
using Subflow.NET.Engine.Validation.Interfaces;

namespace Subflow.NET.Engine.Validation
{
    public abstract class BaseValidationRule<T> : IValidationRule<T>, IBaseValidationRule
    {
        public Type InputType => typeof(T);
        public virtual ValidationSeverity DefaultSeverity => ValidationSeverity.Error;
        public abstract void Validate(T input);
    }
}