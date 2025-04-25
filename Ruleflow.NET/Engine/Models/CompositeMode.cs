using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruleflow.NET.Engine.Models
{
    /// <summary>
    /// The composition mode for a composite rule.
    /// </summary>
    public enum CompositionMode
    {
        /// <summary>
        /// All child rules must pass for the composite rule to pass.
        /// </summary>
        All,

        /// <summary>
        /// At least one child rule must pass for the composite rule to pass.
        /// </summary>
        Any,

        /// <summary>
        /// The composite rule passes if the specified number of child rules pass.
        /// </summary>
        Threshold
    }
}
