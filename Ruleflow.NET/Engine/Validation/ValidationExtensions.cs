using Ruleflow.NET.Engine.Validation.Core.Results;
using Ruleflow.NET.Engine.Validation.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ruleflow.NET.Engine.Validation
{
    /// <summary>
    /// Poskytuje rozšiřující metody pro práci s validátory a výsledky validace.
    /// </summary>
    public static class ValidationExtensions
    {
        /// <summary>
        /// Validuje vstup a vrací logickou hodnotu indikující platnost.
        /// </summary>
        /// <typeparam name="T">Typ validovaných dat</typeparam>
        /// <param name="validator">Validátor</param>
        /// <param name="input">Vstupní data k validaci</param>
        /// <returns>True, pokud je vstup platný; jinak false</returns>
        public static bool IsValid<T>(this IValidator<T> validator, T input)
        {
            return validator.CollectValidationResults(input).IsValid;
        }

        /// <summary>
        /// Validuje vstup a vrací první chybu, nebo null, pokud je vstup platný.
        /// </summary>
        /// <typeparam name="T">Typ validovaných dat</typeparam>
        /// <param name="validator">Validátor</param>
        /// <param name="input">Vstupní data k validaci</param>
        /// <returns>První chyba, nebo null, pokud je vstup platný</returns>
        public static ValidationError? GetFirstError<T>(this IValidator<T> validator, T input)
        {
            var result = validator.CollectValidationResults(input);
            return result.IsValid ? null : result.Errors.FirstOrDefault();
        }

        /// <summary>
        /// Validuje vstup a vyvolá zadanou akci podle výsledku validace.
        /// </summary>
        /// <typeparam name="T">Typ validovaných dat</typeparam>
        /// <param name="validator">Validátor</param>
        /// <param name="input">Vstupní data k validaci</param>
        /// <param name="onSuccess">Akce, která se provede při úspěšné validaci</param>
        /// <param name="onFailure">Akce, která se provede při selhání validace</param>
        public static void ValidateAndExecute<T>(
            this IValidator<T> validator,
            T input,
            Action onSuccess,
            Action<IReadOnlyList<ValidationError>> onFailure)
        {
            var result = validator.CollectValidationResults(input);
            if (result.IsValid)
            {
                onSuccess();
            }
            else
            {
                onFailure(result.Errors);
            }
        }

        /// <summary>
        /// Validuje vstup a vyvolá zadanou akci při úspěšné validaci.
        /// </summary>
        /// <typeparam name="T">Typ validovaných dat</typeparam>
        /// <param name="validator">Validátor</param>
        /// <param name="input">Vstupní data k validaci</param>
        /// <param name="onSuccess">Akce, která se provede při úspěšné validaci s validovaným objektem</param>
        /// <returns>Výsledek validace pro další zpracování</returns>
        public static IValidationResult ValidateAndProcess<T>(
            this IValidator<T> validator,
            T input,
            Action<T> onSuccess)
        {
            var result = validator.CollectValidationResults(input);
            return result.OnSuccess(input, onSuccess);
        }
    }
}