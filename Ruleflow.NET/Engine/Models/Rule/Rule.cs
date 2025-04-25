using Ruleflow.NET.Engine.Models.Rule;

using System.Text;

/// <summary>
/// Představuje konkrétní validační pravidlo v Ruleflow.NET systému,.
/// </summary>
/// <typeparam name="TInput">Typ dat, která budou validována.</typeparam>
public class Rule<TInput>
{
    public int Id { get; }
    public string RuleId { get; }
    public string? Name { get; }
    public string? Description { get; }
    public int Priority { get; }
    public bool IsActive { get; }
    public DateTimeOffset Timestamp { get; }

    // --- silná vazba na RuleType:
    public int RuleTypeId { get; }      // FK pro ORM
    public RuleType Type { get; }       // navigační vlastnost

    public Rule(
        int id,
        RuleType type,
        string? ruleId = null,
        string? name = null,
        string? description = null,
        int priority = 0,
        bool isActive = true,
        DateTimeOffset? timestamp = null)
    {
        Id = id;
        Type = type ?? throw new ArgumentNullException(nameof(type));
        RuleTypeId = type.Id;

        RuleId = ruleId ?? Guid.NewGuid().ToString();
        Name = name;
        Description = description;
        Priority = priority;
        IsActive = isActive;
        Timestamp = timestamp?.ToUniversalTime() ?? DateTimeOffset.UtcNow;
    }

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
