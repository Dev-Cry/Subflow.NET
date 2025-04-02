using Subflow.NET.Engine.Validation.Interfaces;
using System;

namespace Subflow.NET.Engine.Validation
{
    public abstract class BaseValidationRule<T> : IValidationRule<T>, IBaseValidationRule
    {
        public Type InputType => typeof(T);
        public abstract void Validate(T input);
    }
}