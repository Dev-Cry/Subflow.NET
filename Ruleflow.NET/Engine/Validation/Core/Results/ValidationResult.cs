using Ruleflow.NET.Engine.Validation.Core.Exceptions;
using Ruleflow.NET.Engine.Validation.Enums;
using Ruleflow.NET.Engine.Validation.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ruleflow.NET.Engine.Validation.Core.Results
{
    /// <summary>
    /// Reprezentuje výsledek validace se seznamem chyb a pomocnými metodami.
    /// </summary>
    public class ValidationResult : IValidationResult
    {
        private readonly List<ValidationError> _errors = new();

        /// <summary>
        /// Určuje, zda validace byla úspěšná (neobsahuje chyby s úrovní Error nebo vyšší).
        /// </summary>
        public bool IsValid => !_errors.Any(e => e.Severity >= ValidationSeverity.Error);

        /// <summary>
        /// Určuje, zda validace obsahuje kritické chyby.
        /// </summary>
        public bool HasCriticalErrors => _errors.Any(e => e.Severity == ValidationSeverity.Critical);

        /// <summary>
        /// Seznam všech chyb zjištěných během validace.
        /// </summary>
        public IReadOnlyList<ValidationError> Errors => _errors.AsReadOnly();

        /// <summary>
        /// Přidá chybu do výsledku validace.
        /// </summary>
        /// <param name="error">Chyba k přidání</param>
        /// <exception cref="ArgumentNullException">Vyhozeno, když je error null</exception>
        public void AddError(ValidationError error)
        {
            if (error == null) throw new ArgumentNullException(nameof(error));
            _errors.Add(error);
        }

        /// <summary>
        /// Přidá chybu do výsledku validace s danými parametry.
        /// </summary>
        /// <param name="message">Chybová zpráva</param>
        /// <param name="severity">Závažnost chyby</param>
        /// <param name="code">Volitelný kód chyby</param>
        /// <param name="context">Volitelný kontext chyby</param>
        public void AddError(string message, ValidationSeverity severity = ValidationSeverity.Error, string? code = null, object? context = null)
        {
            AddError(new ValidationError(message, severity, code, context));
        }

        /// <summary>
        /// Přidá kolekci chyb do výsledku validace.
        /// </summary>
        /// <param name="errors">Chyby k přidání</param>
        public void AddErrors(IEnumerable<ValidationError> errors)
        {
            foreach (var error in errors)
            {
                AddError(error);
            }
        }

        /// <summary>
        /// Vrátí chyby se zadanou úrovní závažnosti.
        /// </summary>
        /// <param name="severity">Úroveň závažnosti</param>
        /// <returns>Chyby se zadanou úrovní závažnosti</returns>
        public IEnumerable<ValidationError> GetErrorsBySeverity(ValidationSeverity severity)
        {
            return _errors.Where(e => e.Severity == severity);
        }

        /// <summary>
        /// Vyhodí výjimku, pokud výsledek validace obsahuje chyby. Jinak neudělá nic.
        /// </summary>
        /// <exception cref="AggregateException">Vyhozeno, pokud výsledek obsahuje chyby</exception>
        public void ThrowIfInvalid()
        {
            if (!IsValid)
            {
                if (HasCriticalErrors)
                {
                    var criticalErrors = GetErrorsBySeverity(ValidationSeverity.Critical).ToList();
                    throw new AggregateException("Validace selhala s kritickými chybami",
                        criticalErrors.Select(e => new ValidationException(e.Message, e)));
                }

                var errors = Errors.Where(e => e.Severity >= ValidationSeverity.Error).ToList();
                throw new AggregateException("Validace selhala",
                    errors.Select(e => new ValidationException(e.Message, e)));
            }
        }

        /// <summary>
        /// Provede zadanou akci, pokud je výsledek validace platný.
        /// </summary>
        /// <param name="action">Akce k provedení</param>
        /// <returns>Tentýž validační výsledek pro řetězení volání</returns>
        public ValidationResult OnSuccess(Action action)
        {
            if (IsValid)
            {
                action();
            }
            return this;
        }

        /// <summary>
        /// Provede zadanou akci s validovaným objektem, pokud je výsledek validace platný.
        /// </summary>
        /// <typeparam name="TInput">Typ validovaného objektu</typeparam>
        /// <param name="input">Validovaný objekt</param>
        /// <param name="action">Akce k provedení s objektem</param>
        /// <returns>Tentýž validační výsledek pro řetězení volání</returns>
        public ValidationResult OnSuccess<TInput>(TInput input, Action<TInput> action)
        {
            if (IsValid)
            {
                action(input);
            }
            return this;
        }

        /// <summary>
        /// Provede zadanou akci s chybami, pokud validace selhala.
        /// </summary>
        /// <param name="action">Akce pro zpracování chyb</param>
        /// <returns>Tentýž validační výsledek pro řetězení volání</returns>
        public ValidationResult OnFailure(Action<IReadOnlyList<ValidationError>> action)
        {
            if (!IsValid)
            {
                action(Errors);
            }
            return this;
        }
    }
}