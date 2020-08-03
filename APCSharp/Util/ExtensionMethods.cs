using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace APCSharp.Util
{
    internal static class ExtensionMethods
    {
        internal static string ReplaceAll(this string s, char p, string r) => ReplaceAll(s, p.ToString(), r);
        internal static string ReplaceAll(this string s, string p, string r) => Regex.Replace(s, p, r, RegexOptions.Compiled | RegexOptions.Multiline);
    }
}
