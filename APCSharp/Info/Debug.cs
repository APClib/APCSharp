using System;
using APCSharp.Parser.Data;
using APCSharp.Util;

namespace APCSharp.Info
{
    /// <summary>
    /// Static class for debug handling.
    /// </summary>
    internal static class Debug
    {
        /// <summary>
        /// Print the debug message if DebugInfo is enabled in Config.
        /// </summary>
        /// <param name="message"></param>
        public static void Print(string message)
        {
            if (Config.Verbose) Console.WriteLine($"Debug Info ({SharedData.LineColumn}): " + message.ValueToHRT());
        }
    }
}
