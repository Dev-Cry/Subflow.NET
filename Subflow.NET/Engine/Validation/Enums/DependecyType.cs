namespace Ruleflow.NET.Engine.Validation.Enums
{
    /// <summary>
    /// Definuje typy závislostí mezi validačními pravidly
    /// </summary>
    public enum DependencyType
    {
        /// <summary>
        /// Toto pravidlo se spustí pouze tehdy, když všechna závislá pravidla uspějí
        /// </summary>
        RequiresAllSuccess,

        /// <summary>
        /// Toto pravidlo se spustí pouze tehdy, když alespoň jedno závislé pravidlo uspěje
        /// </summary>
        RequiresAnySuccess,

        /// <summary>
        /// Toto pravidlo se spustí pouze tehdy, když všechna závislá pravidla selžou
        /// </summary>
        RequiresAllFailure,

        /// <summary>
        /// Toto pravidlo se spustí pouze tehdy, když alespoň jedno závislé pravidlo selže
        /// </summary>
        RequiresAnyFailure
    }
}