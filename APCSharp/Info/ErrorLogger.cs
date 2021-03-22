using System;
using APCSharp.Parser;
using APCSharp.Util;

namespace APCSharp.Info
{
    public class ErrorLogger<TParserBuilder, TCombiner, TPResult, TNode>
        where TParserBuilder : IParserBuilder<TParserBuilder, TCombiner, TNode>
        where TCombiner : Combiner
        where TPResult : PResult
        where TNode : struct, IConvertible
    {
        internal string Invalid(string what, string innerException) => SyntaxError($"Invalid {what}; " + innerException);
        internal string Unexpected(dynamic got, params IParserBuilder<TParserBuilder, TCombiner, TNode>[] parsers) => SyntaxError($"Unexpected '{((got == null ? "NULL" : got).ToString() as string).ValueToHRT()}' (expected {ToExpected(parsers)})");
        internal string SyntaxError(string message) => $"Syntax Error ({Parser.Data.SharedData.LineColumn}): " + message + '.';
        private string ToExpected(IParserBuilder<TParserBuilder, TCombiner, TNode>[] parsers) 
        {
            string result = string.Empty;
            for (int i = 0; i < parsers.Length; i++)
            {
                result += parsers[i].GetMatchString();
                if (i < parsers.Length - 2) result += ", ";
                else if (i < parsers.Length - 1) result += " or ";
            }
            return result.ValueToHRT();
        }
    }
}
