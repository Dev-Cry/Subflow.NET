using Ruleflow.NET.Engine.Validation.Enums;
using Ruleflow.NET.Engine.Validation.Interfaces;
using System;

namespace Ruleflow.NET.Engine.Validation.Core.Base
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