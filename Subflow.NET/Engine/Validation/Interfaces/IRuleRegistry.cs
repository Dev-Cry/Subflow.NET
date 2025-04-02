namespace Subflow.NET.Engine.Validation.Interfaces
{
    public interface IRuleRegistry
    {
        IValidator<T> CreateValidator<T>();
    }
}
