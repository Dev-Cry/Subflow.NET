using Ruleflow.NET.Engine.Validation.Core.Base;
using Ruleflow.NET.Engine.Validation.Enums;

using Ruleflow.NET.Engine.Validation.Interfaces;

/// <summary>
/// Implementace validačního pravidla pro podmíněné větvení.
/// </summary>
/// <typeparam name="T">Typ validovaných dat</typeparam>
internal class ConditionalBranchRule<T> : IdentifiableValidationRule<T>, IPrioritizedValidationRule<T>
{
    private readonly IValidationRule<T> _baseRule;
    private readonly Func<T, bool> _condition;
    private readonly IValidationRule<T> _thenRule;
    private readonly List<(Func<T, bool> Condition, IValidationRule<T> Rule)> _elseIfRules;
    private readonly IValidationRule<T>? _elseRule;

    /// <summary>
    /// Inicializuje novou instanci validačního pravidla pro podmíněné větvení.
    /// </summary>
    /// <param name="baseRule">Základní pravidlo</param>
    /// <param name="condition">Podmínka pro vyhodnocení</param>
    /// <param name="thenRule">Pravidlo pro then větev</param>
    /// <param name="elseIfRules">Pravidla pro else-if větve</param>
    /// <param name="elseRule">Pravidlo pro else větev</param>
    public ConditionalBranchRule(
        IValidationRule<T> baseRule,
        Func<T, bool> condition,
        IValidationRule<T> thenRule,
        List<(Func<T, bool> Condition, IValidationRule<T> Rule)> elseIfRules,
        IValidationRule<T>? elseRule)
        : base(GetRuleId(baseRule))
    {
        _baseRule = baseRule ?? throw new ArgumentNullException(nameof(baseRule));
        _condition = condition ?? throw new ArgumentNullException(nameof(condition));
        _thenRule = thenRule ?? throw new ArgumentNullException(nameof(thenRule));
        _elseIfRules = elseIfRules ?? [];
        _elseRule = elseRule;
    }

    /// <inheritdoc />
    public override ValidationSeverity DefaultSeverity => _baseRule.DefaultSeverity;

    /// <inheritdoc />
    public override int Priority => _baseRule is IPrioritizedValidationRule<T> prioritized ? prioritized.Priority : 0;

    public override bool ShouldValidate(T input, ValidationContext context)
    {
        // Vždy vrátí true, protože pravidlo se vyhodnotí vždy, 
        // ale interně rozhodne, která větev se má spustit
        return true;
    }

    /// <inheritdoc />
    public override void Validate(T input)
    {
        if (_condition(input))
        {
            _thenRule.Validate(input);
        }
        else
        {
            foreach (var (condition, rule) in _elseIfRules)
            {
                if (condition(input))
                {
                    rule.Validate(input);
                    return;
                }
            }

            _elseRule?.Validate(input);
        }
    }

    private static string GetRuleId(IValidationRule<T> rule)
    {
        return rule is IIdentifiableValidationRule<T> identifiable
            ? identifiable.RuleId
            : Guid.NewGuid().ToString();
    }
}
}