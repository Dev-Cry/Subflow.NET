using Subflow.NET.Engine.Validation.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Subflow.NET.Engine.Validation
{
    public class ValidationResult : IValidationResult
    {
        public bool IsValid => !Errors.Any();
        public List<string> Errors { get; } = new();

        public void AddError(string error)
        {
            if (!string.IsNullOrWhiteSpace(error))
                Errors.Add(error);
        }

        public void AddErrors(IEnumerable<string> errors)
        {
            foreach (var error in errors)
                AddError(error);
        }
    }
}