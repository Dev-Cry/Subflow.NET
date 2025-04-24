using Ruleflow.NET.Engine.Validation.Core.Results;
using Ruleflow.NET.Engine.Validation.Enums;
using System;
using System.Collections.Generic;

namespace Ruleflow.NET.Engine.Validation.Interfaces
{
    public interface IValidationResult
    {
        bool IsValid { get; }
        bool HasCriticalErrors { get; }
        IReadOnlyList<ValidationError> Errors { get; }

        void AddError(ValidationError error);
        void AddError(string message, ValidationSeverity severity = ValidationSeverity.Error, string? code = null, object? context = null);
        void AddErrors(IEnumerable<ValidationError> errors);
        IEnumerable<ValidationError> GetErrorsBySeverity(ValidationSeverity severity);

        /// <summary>
        /// Vyhodí výjimku, pokud výsledek validace obsahuje chyby. Jinak neudělá nic.
        /// </summary>
        /// <exception cref="AggregateException">Vyhozeno, pokud výsledek obsahuje chyby</exception>
        void ThrowIfInvalid();

        /// <summary>
        /// Provede zadanou akci, pokud je výsledek validace platný.
        /// </summary>
        /// <param name="action">Akce k provedení</param>
        /// <returns>Tentýž validační výsledek pro řetězení volání</returns>
        IValidationResult OnSuccess(Action action);

        /// <summary>
        /// Provede zadanou akci s validovaným objektem, pokud je výsledek validace platný.
        /// </summary>
        /// <typeparam name="TInput">Typ validovaného objektu</typeparam>
        /// <param name="input">Validovaný objekt</param>
        /// <param name="action">Akce k provedení s objektem</param>
        /// <returns>Tentýž validační výsledek pro řetězení volání</returns>
        IValidationResult OnSuccess<TInput>(TInput input, Action<TInput> action);

        /// <summary>
        /// Provede zadanou akci s chybami, pokud validace selhala.
        /// </summary>
        /// <param name="action">Akce pro zpracování chyb</param>
        /// <returns>Tentýž validační výsledek pro řetězení volání</returns>
        IValidationResult OnFailure(Action<IReadOnlyList<ValidationError>> action);
    }
}