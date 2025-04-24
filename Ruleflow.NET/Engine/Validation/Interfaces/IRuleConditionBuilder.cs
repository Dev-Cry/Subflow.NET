using Ruleflow.NET.Engine.Validation.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruleflow.NET.Engine.Validation.Interfaces
{
    /// <summary>
    /// Rozhraní pro definici podmíněných validačních pravidel pomocí if/then konstrukce.
    /// </summary>
    /// <typeparam name="T">Typ validovaných dat</typeparam>
    public interface IRuleConditionBuilder<T>
    {
        /// <summary>
        /// Definuje validační pravidlo, které se provede, když je podmínka splněna.
        /// </summary>
        /// <param name="configureRule">Akce pro konfiguraci pravidla</param>
        /// <returns>Builder pro možnost definice else větve</returns>
        IRuleConditionElseBuilder<T> Then(Action<ValidationRuleBuilder<T>> configureRule);

        /// <summary>
        /// Vytvoří validační pravidlo z aktuální konfigurace.
        /// </summary>
        /// <returns>Validační pravidlo</returns>
        IValidationRule<T> Build();
    }
}
