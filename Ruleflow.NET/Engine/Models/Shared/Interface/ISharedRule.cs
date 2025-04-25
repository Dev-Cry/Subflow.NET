using Ruleflow.NET.Engine.Models.Rule.Context.Interface;
using Ruleflow.NET.Engine.Models.Rule.Interface;

namespace Ruleflow.NET.Engine.Models.Shared.Interface
{
    /// <summary>
    /// Rozhraní pro sdílené pravidlo s kontextem.
    /// </summary>
    /// <typeparam name="TInput">Typ validovaných dat.</typeparam>
    public interface ISharedRule<TInput>
    {
        /// <summary>
        /// Identifikátor sdíleného pravidla.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Pravidlo, které je sdíleno.
        /// </summary>
        IRule<TInput> Rule { get; }

        /// <summary>
        /// Kontext, ve kterém je pravidlo sdíleno.
        /// </summary>
        IRuleContext Context { get; }

        /// <summary>
        /// Čas sdílení pravidla.
        /// </summary>
        DateTimeOffset SharedAt { get; }
    }
}