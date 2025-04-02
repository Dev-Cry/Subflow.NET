using Subflow.NET.Engine.Validation.Enums;
using Subflow.NET.Engine.Validation.Interfaces;
using System;

namespace Subflow.NET.Engine.Validation
{
    public abstract class BaseValidationRule<T> : IValidationRule<T>, IBaseValidationRule, IPrioritizedValidationRule<T>
    {
        public Type InputType => typeof(T);
        public virtual ValidationSeverity DefaultSeverity => ValidationSeverity.Error;

        /// <summary>
        /// Priorita pravidla určuje pořadí vyhodnocování (vyšší priorita = dřívější vyhodnocení)
        /// </summary>
        public virtual int Priority => 0;

        public abstract void Validate(T input);
    }
}