using System;

namespace Ruleflow.NET.Engine.Validation.Attributes
{
    /// <summary>
    /// Označuje metodu, která vyhodí výjimku při neplatném vstupu.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ThrowsOnInvalidInputAttribute : Attribute
    {
        /// <summary>
        /// Dokumentační zpráva popisující chování metody při neplatném vstupu.
        /// </summary>
        public string ValidationMessage { get; }

        /// <summary>
        /// Inicializuje novou instanci atributu.
        /// </summary>
        /// <param name="message">Volitelná zpráva popisující chování metody</param>
        public ThrowsOnInvalidInputAttribute(string message = "Metoda vyžaduje platný vstup a vyhodí výjimku při neplatném vstupu.")
        {
            ValidationMessage = message;
        }
    }

    /// <summary>
    /// Označuje metodu, která vrací objekt s výsledkem validace a nevyhazuje výjimky.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ReturnsValidationResultAttribute : Attribute
    {
        /// <summary>
        /// Dokumentační zpráva popisující chování metody.
        /// </summary>
        public string ValidationMessage { get; }

        /// <summary>
        /// Inicializuje novou instanci atributu.
        /// </summary>
        /// <param name="message">Volitelná zpráva popisující chování metody</param>
        public ReturnsValidationResultAttribute(string message = "Metoda vrací objekt s výsledkem validace a nevyhazuje výjimky.")
        {
            ValidationMessage = message;
        }
    }
}