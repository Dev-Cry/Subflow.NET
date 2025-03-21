using System;
using System.Text;

namespace Subflow.NET.Data.Model.Subtitle
{
    /// <summary>
    /// Rozhraní pro model titulku.
    /// </summary>
    public interface ISubtitle
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
        string Text { get; set; }

        /// <summary>
        /// Převede titulek na řetězec pro snadné výpisování.
        /// </summary>
        /// <returns>Řetězec reprezentující titulek.</returns>
        string ToString();
    }
}