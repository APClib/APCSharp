using System;
using System.Collections.Generic;
using System.Text;

namespace APCSharp.Util
{
    /// <summary>
    /// Configuration for the APC parser
    /// </summary>
    public static class Config
    {
        /// <summary>
        /// Indentation prefix for node tree structure formatting.
        /// </summary>
        public static string Indentation = "    ";
        /// <summary>
        /// Print debug info from each parser Function.
        /// </summary>
        public static bool DebugInfo = false;
    }
}
