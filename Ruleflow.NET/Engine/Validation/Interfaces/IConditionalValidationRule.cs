using Ruleflow.NET.Engine.Validation.Core.Context;

namespace Ruleflow.NET.Engine.Validation.Interfaces
{
    /// <summary>
    /// Rozhraní pro validační pravidla, která se vyhodnocují podmíněně
    /// </summary>
    public interface IConditionalValidationRule<T> : IValidationRule<T>
    {
        /// <summary>
        /// Určuje, zda by se pravidlo mělo vyhodnotit pro daný vstup a kontext
        /// </summary>
        bool ShouldValidate(T input, ValidationContext context);
    }
}