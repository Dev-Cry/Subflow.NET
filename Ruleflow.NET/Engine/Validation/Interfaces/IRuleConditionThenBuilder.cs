using Ruleflow.NET.Engine.Validation.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruleflow.NET.Engine.Validation.Interfaces
{
    /// <summary>
    /// Rozhraní pro definici then větve v else-if konstrukci.
    /// </summary>
    /// <typeparam name="T">Typ validovaných dat</typeparam>
    public interface IRuleConditionThenBuilder<T>
    {
        /// <summary>
        /// Definuje validační pravidlo, které se provede, když je podmínka splněna.
        /// </summary>
        /// <param name="configureRule">Akce pro konfiguraci pravidla</param>
        /// <returns>Builder pro možnost definice další else větve</returns>
        IRuleConditionElseBuilder<T> Then(Action<ValidationRuleBuilder<T>> configureRule);
    }

}
