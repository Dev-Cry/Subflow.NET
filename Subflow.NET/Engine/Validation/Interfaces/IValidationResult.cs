using System.Collections.Generic;

namespace Subflow.NET.Engine.Validation.Interfaces
{
    public interface IValidationResult
    {
        bool IsValid { get; }
        List<string> Errors { get; }
        void AddError(string error);
        void AddErrors(IEnumerable<string> errors);
    }
}