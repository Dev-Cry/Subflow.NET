using Ruleflow.NET.Engine.Validation.Core.Context;

namespace Ruleflow.NET.Engine.Validation.Interfaces
{
    /// <summary>
    /// Rozhraní pro validátory, které podporují kontextově závislou validaci
    /// </summary>
    public interface IContextAwareValidator<T>
    {
        /// <summary>
        /// Validuje vstup s použitím daného kontextu
        /// </summary>
        IValidationResult ValidateWithContext(T input, ValidationContext context);
    }
}