﻿using Ruleflow.NET.Engine.Validation.Core.Context;
using Ruleflow.NET.Engine.Validation.Interfaces;

namespace Ruleflow.NET.Engine.Validation.Core.Base
{
    /// <summary>
    /// Základní třída pro pravidla, která se vyhodnocují podmíněně na základě vstupních dat
    /// </summary>
    public abstract class ConditionalValidationRule<T> : IdentifiableValidationRule<T>, IConditionalValidationRule<T>
    {
        protected ConditionalValidationRule(string ruleId)
            : base(ruleId)
        {
        }

        /// <summary>
        /// Určuje, zda by se pravidlo mělo vyhodnotit pro daný vstup a kontext
        /// </summary>
        public abstract bool ShouldValidate(T input, ValidationContext context);
    }
}