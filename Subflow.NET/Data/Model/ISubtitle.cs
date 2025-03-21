using System;
using System.Collections.Generic;
using System.Text;

namespace Subflow.NET.Data.Model
{
    /// <summary>
    /// Rozhraní pro model titulku.
    /// </summary>
    public interface ISubtitle : IEquatable<ISubtitle>
    {
        /// <summary>
        /// Pořadové číslo titulku.
        /// </summary>
        int Index { get; set; }

        /// <summary>
        /// Čas začátku titulku.
        /// </summary>
        TimeSpan StartTime { get; set; }

        /// <summary>
        /// Čas konce titulku.
        /// </summary>
        TimeSpan EndTime { get; set; }

        /// <summary>
        /// Text titulku.
        /// </summary>
        List<string> Lines { get; set; }

        /// <summary>
        /// Převede titulek na řetězec pro snadné výpisování.
        /// </summary>
        /// <returns>Řetězec reprezentující titulek.</returns>
        string ToString();
    }
}