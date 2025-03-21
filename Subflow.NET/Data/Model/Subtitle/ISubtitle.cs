using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subflow.NET.Data.Model.Subtitle
{
    public interface ISubtitle
    {
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
    }
}
