using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subflow.NET.IO.Loader.Validation
{
    public class Validator<T> : IValidator<T>
    {
        private readonly IEnumerable<IValidationRule<T>> _rules;

        public Validator(IEnumerable<IValidationRule<T>> rules)
        {
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
        }

        public void Validate(T input)
        {
            foreach (var rule in _rules)
            {
                rule.Validate(input);
            }
        }
    }
}
