using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subflow.NET.Data.Model.Subtitle
{
    public class Subtitle
    {
        /// <summary>
        /// Pořadové číslo titulku.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Čas začátku titulku.
        /// </summary>
        public TimeSpan StartTime { get; set; }

        /// <summary>
        /// Čas konce titulku.
        /// </summary>
        public TimeSpan EndTime { get; set; }

        /// <summary>
        /// Text titulku.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Převede titulek na řetězec pro snadné výpisování.
        /// </summary>
        public override string ToString()
        {
            return $"[{Index}] {StartTime} --> {EndTime}\n{Text}";
        }
    }
}
