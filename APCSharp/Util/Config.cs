using System;
using System.Collections.Generic;
using System.Text;

namespace APCSharp.Util
{
    /// <summary>
    /// Configuration for the APC parser.
    /// </summary>
    public static class Config
    {
        /// <summary>
        /// Indentation prefix for node tree structure formatting.
        /// </summary>
        public static string Indentation = "    ";
        /// <summary>
        /// Print debug info from each parser.
        /// </summary>
        /// <value>Enabled or disabled</value>
        public static bool Verbose = false;
        /// <summary>
        /// Dictionary of escaped special character mapped to their real values. Override with your own or provide it when using the Parser.CharBut parser combinator.
        /// </summary>
        /// <value>Map of special character mappings</value>
        public static Dictionary<string, char> StandardCharMapping = new Dictionary<string, char>
        {
            ["\\n"] = '\n'
        };
    }
}
