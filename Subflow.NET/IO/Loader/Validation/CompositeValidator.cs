using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subflow.NET.IO.Loader.Validation
{
    // Kompozitní validátor, který kombinuje více validátorů
    public class CompositeValidator<T> : IValidator<T>
    {
        private readonly IEnumerable<IValidator<T>> _validators;

        public CompositeValidator(IEnumerable<IValidator<T>> validators)
        {
            _validators = validators ?? throw new ArgumentNullException(nameof(validators));
        }

        public void Validate(T input)
        {
            foreach (var validator in _validators)
            {
                validator.Validate(input);
            }
        }
    }
}
