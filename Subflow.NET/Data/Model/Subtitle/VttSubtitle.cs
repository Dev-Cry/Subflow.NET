using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subflow.NET.Data.Model.Subtitle
{
    public class VttSubtitle : ISubtitle
    {
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
        /// Metadata titulku (volitelné).
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Pozice titulku na obrazovce (např. "line:50%,align:center").
        /// </summary>
        public string Position { get; set; }

        /// <summary>
        /// Převede titulek na řetězec pro snadné výpisování.
        /// </summary>
        public override string ToString()
        {
            var metadataString = Metadata.Count > 0 ? string.Join(", ", Metadata) : "No metadata";
            return $"{StartTime} --> {EndTime} {Position}\n{Text}\nMetadata: {metadataString}";
        }
    }
}
