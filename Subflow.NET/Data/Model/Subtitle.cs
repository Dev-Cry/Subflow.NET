using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subflow.NET.Data.Model
{
    public class Subtitle : ISubtitle, IEquatable<ISubtitle>
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
        public List<string> Lines { get; set; }

        /// <summary>
        /// Konstruktor pro vytvoření nového titulku.
        /// </summary>
        /// <param name="index">Pořadové číslo titulku.</param>
        /// <param name="startTime">Čas začátku titulku.</param>
        /// <param name="endTime">Čas konce titulku.</param>
        /// <param name="lines">Text titulku.</param>
        public Subtitle(int index, TimeSpan startTime, TimeSpan endTime, List<string> lines)
        {
            Index = index;
            StartTime = startTime;
            EndTime = endTime;
            Lines = lines ?? new List<string>();
        }

        /// <summary>
        /// Převede titulek na řetězec pro snadné výpisování.
        /// </summary>
        /// <returns>Řetězec reprezentující titulek.</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine(Index.ToString());
            sb.Append(StartTime.ToString(@"hh\:mm\:ss\,fff"));
            sb.Append(" --> ");
            sb.Append(EndTime.ToString(@"hh\:mm\:ss\,fff"));
            sb.AppendLine();
            foreach (var line in Lines)
            {
                sb.AppendLine(line);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Porovnává dvě instance titulků.
        /// </summary>
        /// <param name="other">Druhá instance titulku.</param>
        /// <returns>True pokud jsou instance rovny, jinak false.</returns>
        public bool Equals(ISubtitle other)
        {
            if (other == null) return false;
            return Index == other.Index &&
                   StartTime == other.StartTime &&
                   EndTime == other.EndTime &&
                   Lines.SequenceEqual(other.Lines);
        }

        /// <summary>
        /// Vrátí hash kód titulku.
        /// </summary>
        /// <returns>Hash kód titulku.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + Index.GetHashCode();
                hash = hash * 23 + StartTime.GetHashCode();
                hash = hash * 23 + EndTime.GetHashCode();
                hash = hash * 23 + Lines.GetHashCode();
                return hash;
            }
        }
    }
}