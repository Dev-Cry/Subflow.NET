using Ruleflow.NET.Engine.Models.Rule.Type;
using System.Text;

/// <summary>
/// Představuje konkrétní validační pravidlo v Ruleflow.NET systému,.
/// </summary>
/// <typeparam name="TInput">Typ dat, která budou validována.</typeparam>
public class Rule<TInput>(
    int id,
    RuleType type,
    string? ruleId = null,
    string? name = null,
    string? description = null,
    int priority = 0,
    bool isActive = true,
    DateTimeOffset? timestamp = null)
{
    public int Id { get; } = id;
    public string RuleId { get; } = ruleId ?? Guid.NewGuid().ToString();
    public string? Name { get; } = name;
    public string? Description { get; } = description;
    public int Priority { get; } = priority;
    public bool IsActive { get; } = isActive;
    public DateTimeOffset Timestamp { get; } = timestamp?.ToUniversalTime() ?? DateTimeOffset.UtcNow;

    public int RuleTypeId { get; } = type.Id;
    public RuleType Type { get; } = type ?? throw new ArgumentNullException(nameof(type));

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append($"Rule(Id={Id}, RuleType=\"{Type.Code}\"");
        sb.Append($", RuleId=\"{RuleId}\"");
        if (!string.IsNullOrEmpty(Name))
            sb.Append($", Name=\"{Name}\"");
        if (!string.IsNullOrEmpty(Description))
            sb.Append($", Description=\"{Description}\"");
        sb.Append($", Priority={Priority}");
        sb.Append($", IsActive={IsActive}");
        sb.Append($", Timestamp=\"{Timestamp:o}\"");
        sb.Append(")");
        return sb.ToString();
    }
}
