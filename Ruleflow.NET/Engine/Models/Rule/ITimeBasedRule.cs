using Ruleflow.NET.Engine.Models.Evaluation;
using Ruleflow.NET.Engine.Models.Rule.Context;
using Ruleflow.NET.Engine.Models.Rule.Interface;

namespace Ruleflow.NET.Engine.Models.Rule
{
    /// <summary>
    /// Rozhraní pro časové pravidlo, které vyhodnocuje podmínky na základě času.
    /// </summary>
    /// <typeparam name="TInput">Typ validovaných dat.</typeparam>
    public interface ITimeBasedRule<TInput> : IRule<TInput>
    {
        /// <summary>
        /// Vyhodnotí, zda časová podmínka platí pro zadaný čas.
        /// </summary>
        /// <param name="input">Vstupní data pro validaci.</param>
        /// <param name="evaluationTime">Čas, pro který se podmínka vyhodnocuje.</param>
        /// <param name="context">Kontext vyhodnocení pravidla.</param>
        /// <returns>True pokud časová podmínka platí, jinak false.</returns>
        bool EvaluateTimeCondition(TInput input, DateTimeOffset evaluationTime, RuleContext context);

        /// <summary>
        /// Indikuje, zda se má pro vyhodnocení použít aktuální čas.
        /// Pokud je false, očekává se, že čas bude dodán v kontextu.
        /// </summary>
        bool UseCurrentTime { get; }
    }
}