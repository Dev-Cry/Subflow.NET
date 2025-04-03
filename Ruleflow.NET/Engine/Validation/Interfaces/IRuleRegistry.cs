namespace Ruleflow.NET.Engine.Validation.Interfaces
{
    public interface IRuleRegistry
    {
        IValidator<T> CreateValidator<T>();
    }
}
