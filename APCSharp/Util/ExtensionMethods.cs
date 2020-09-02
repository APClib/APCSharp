using System;
using System.Text.RegularExpressions;

namespace APCSharp.Util
{
    internal static class ExtensionMethods
    {
        internal static string ReplaceAll(this string s, char p, string r) => ReplaceAll(s, p.ToString(), r);
        internal static string ReplaceAll(this string s, string p, string r) => Regex.Replace(s, p, r, RegexOptions.Compiled | RegexOptions.Multiline);

        /// <summary>
        /// Convert input character or string to Human Readable Text.
        /// </summary>
        /// <param name="got">input</param>
        /// <returns>readable input</returns>
        internal static string ValueToHRT(this string s) => s.ReplaceAll('\n', "\\n").ReplaceAll('\r', "\\r").ReplaceAll('\t', "\\t");

        internal static string ArrayToString<T>(this T[] a)
        {
            string r = "[";
            if (a.Length == 0) return r + "]";
            foreach (T e in a) r += e + ", ";
            return r.Remove(r.Length - 2) + "]";
        }
    }
}
