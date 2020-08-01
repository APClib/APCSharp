using System;
using System.Collections.Generic;
using System.Text;

namespace APCSharp
{
    internal static class Error
    {
        internal static ParserException Invalid(string what, ParserException innerExcepton, string input) => Invalid(what, innerExcepton, PResult.Failed(input));
        internal static ParserException Invalid(string what, ParserException innerExcepton, PResult result) => SyntaxError($"Invalid {what}; " + innerExcepton.Message, result);
        internal static ParserException Unexpected(string what, char got, char expected, string input) => Unexpected(what, got.ToString(), expected.ToString(), PResult.Failed(input));
        internal static ParserException Unexpected(string what, char got, char expected, PResult result) => Unexpected(what, got.ToString(), expected.ToString(), result);
        internal static ParserException Unexpected(string what, string got, string expected, string input) => Unexpected(what, got, expected, PResult.Failed(input));
        internal static ParserException Unexpected(string what, string got, string expected, PResult result) => SyntaxError($"Unexpected {what} '{got}', expected '{expected}' in \"{result.Remaining}\"", result);
        internal static ParserException SyntaxError(string message, string input) => SyntaxError(message, PResult.Failed(input));
        internal static ParserException SyntaxError(string message, PResult result) => new ParserException($"Syntax Error ({SharedData.LineColumn}): " + message + '.', result);
    }
}
