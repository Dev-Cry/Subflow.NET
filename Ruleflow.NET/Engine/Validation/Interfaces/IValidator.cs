using Ruleflow.NET.Engine.Validation.Enums;
using System;

namespace Ruleflow.NET.Engine.Validation.Interfaces
{
    /// <summary>
    /// Rozhraní pro validátory, které poskytují metody pro validaci vstupních dat.
    /// </summary>
    /// <typeparam name="T">Typ vstupních dat, která budou validována</typeparam>
    public interface IValidator<T>
    {
        /// <summary>
        /// Validuje vstupní data a vrátí objekt s výsledky validace.
        /// Použijte tuto metodu, když potřebujete sbírat více chyb nebo pokračovat v zpracování i při neplatném vstupu.
        /// </summary>
        /// <param name="input">Vstupní data k validaci</param>
        /// <returns>Validační výsledek obsahující všechny nalezené chyby</returns>
        IValidationResult CollectValidationResults(T input);

        /// <summary>
        /// Validuje vstupní data a vyhodí výjimku, pokud validace selže.
        /// Použijte tuto metodu, když potřebujete okamžitě přerušit zpracování při neplatném vstupu.
        /// </summary>
        /// <param name="input">Vstupní data k validaci</param>
        /// <exception cref="Ruleflow.NET.Engine.Validation.Core.Exceptions.ValidationException">Vyhozeno při selhání validace</exception>
        void ValidateOrThrow(T input);

        /// <summary>
        /// Validuje vstup podle zadaného režimu.
        /// </summary>
        /// <remarks>
        /// Pro jasnější záměr použijte místo této metody 
        /// <see cref="ValidateOrThrow"/> nebo <see cref="CollectValidationResults"/>.
        /// </remarks>
        /// <param name="input">Vstupní data k validaci</param>
        /// <param name="mode">Režim validace určující chování při selhání</param>
        void Validate(T input, ValidationMode mode = ValidationMode.ThrowOnError);
    }
}