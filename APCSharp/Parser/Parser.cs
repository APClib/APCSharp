﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using APCSharp.Info;
using APCSharp.Parser.Data;
using APCSharp.Util;

namespace APCSharp.Parser
{
    /// <summary>
    /// Shared abstract interface for parsers.
    /// </summary>
    public abstract class AParser<TParserBuilder, TCombiner, TPResult, TNode>
        where TParserBuilder : IParserBuilder<TParserBuilder, TCombiner, TNode>
        where TCombiner : Combiner
        where TPResult : PResult
        where TNode : struct, IConvertible
    {
        protected static readonly ErrorLogger<TParserBuilder, TCombiner, TPResult, TNode> Error = new ErrorLogger<TParserBuilder, TCombiner, TPResult, TNode>();
        internal string Type { get; set; }
        internal dynamic SpecificValue { get; set; }
        internal Func<StreamReader, TPResult> Func;

        protected AParser(Func<StreamReader, TPResult> func) { Func = func; }
        protected AParser(string type, Func<StreamReader, TPResult> func) : this(func) { Type = type; }
        protected AParser(string type) { Type = type; }
        
        /// <summary>
        /// Parse text from stream.
        /// </summary>
        /// <param name="sourceStream">Source input stream</param>
        /// <param name="encoding">Stream encoding</param>
        /// <returns></returns>
        public virtual TPResult Run(Stream sourceStream, Encoding encoding)
        {
            SharedData.LineColumn.Reset();
            return Func(new StreamReader(sourceStream, encoding));
        }
        /// <summary>
        /// Parse unicode text from stream.
        /// </summary>
        /// <param name="sourceStream">Source input stream</param>
        /// <returns></returns>
        public virtual TPResult Run(Stream sourceStream) => Run(sourceStream, Encoding.UTF8);
        /// <summary>
        /// Parse text.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="encoding">Text encoding</param>
        /// <returns></returns>
        public virtual TPResult Run(string input, Encoding encoding) => Run(new MemoryStream(encoding.GetBytes(input ?? "")), encoding);
        /// <summary>
        /// Parse unicode text.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public virtual TPResult Run(string input) => Run(input, Encoding.UTF8);

        
        #region Shared utility methods
        public string GetMatchString()
        {
            if (SpecificValue != null && !string.IsNullOrEmpty(Type)) return $"{Type} '{SpecificValue.ToString()}'";
            if (SpecificValue != null) return $"'{SpecificValue.ToString()}'";
            if (!string.IsNullOrEmpty(Type)) return Type;
            return string.Empty;
        }
        protected static void ProcessChar(char c)
        {
            Data.SharedData.LineColumn.NextColumn();
            switch (c)
            {
                case '\n':
                    Data.SharedData.LineColumn.NextLine();
                    break;
            }
        }

        #endregion
    }








    /// <summary>
    /// Default text parser enough for most parsers.
    /// </summary>
    public class Parser : AParser<ParserBuilder, Combiner, PResult, NodeType>
    {
        public Parser(Func<StreamReader, PResult> func) : base(func) { }
        public Parser(string type, Func<StreamReader, PResult> func) : base(type, func) {}
        public Parser(string type, dynamic specificValue, Func<StreamReader, PResult> func) : base(type, func) { SpecificValue = specificValue; }
        public Parser(string type, dynamic specificValue) : base(type) { SpecificValue = specificValue; }
        public Parser(string type) : base(type) {}




        #region Preset Parsers
        
        /// <summary>
        /// Generate parser just in time. Allows for recursive calls and prevents stack overflows. 
        /// </summary>
        /// <param name="parser">Parser to make lazy</param>
        /// <returns></returns>
        public static ParserBuilder Lazy(ParserBuilder parser) => new ParserBuilder(s => parser.Func(s));

        /// <summary>
        /// Accepts character if in range of n and m and set a custom named NodeType.
        /// </summary>
        /// <param name="n">Lower character bound</param>
        /// <param name="m">Upper character bound></param>
        /// <param name="charType">Named type of matched character</param>
        /// <returns>Parser for any character between n and m</returns>
        public static ParserBuilder InRange(char n, char m, NodeType charType)
        {
            ParserBuilder parser = new ParserBuilder("character in range " + n + " to " + m);
            parser.Func = s =>
            {
                if (s.Peek() == -1) return PResult.EndOfInput(Error, parser);
                char c = (char) s.Peek();
                if (c >= n && c <= m)
                {
                    Debug.Print("Parsed char '" + c + "'");
                    ProcessChar(c);
                    return PResult.Succeeded(new Node(charType, ((char)s.Read()).ToString()), s);
                }
                return PResult.Failed(Error.Unexpected(c, parser), c, s);
                
            };
            return parser;
        }

        public static ParserBuilder SequenceOf(params ParserBuilder[] parsers)
        {
            if (parsers.Length == 1) return parsers[0];
            int i = 1;
            ParserBuilder last = parsers[i - 1].FollowedBy(parsers[i]).ListToString();
            for (i++; i < parsers.Length; i++) last = last.FollowedBy(parsers[i]).ListToString();
            return last;
        }

        public static ParserBuilder AnyOf(params Parser[] parsers) => AnyOf(parsers.ArrayCast<ParserBuilder, Parser>());
        public static ParserBuilder AnyOf(params ParserBuilder[] parsers)
        {
            if (parsers.Length == 1) return parsers[0];
            int i = 1;
            ParserBuilder last = parsers[i - 1].Or(parsers[i]);
            for (i++; i < parsers.Length; i++) last = last.Or(parsers[i]);
            return last;
        }


        /// <summary>
        /// Accepts character if in range of n and m.
        /// </summary>
        /// <param name="n">Lower character bound</param>
        /// <param name="m">Upper character bound></param>
        /// <returns>Parser for any character between n and m</returns>
        public static ParserBuilder InRange(char n, char m) => InRange(n, m, NodeType.Char);
        /// <summary>
        /// Only match a specific character.
        /// </summary>
        /// <param name="m">character to match</param>
        /// <returns>Single character parser</returns>
        public static ParserBuilder Char(char m)
        {
            ParserBuilder parser = new ParserBuilder("character", m);
            parser.Func = s =>
            {
                if (s.Peek() == -1) return PResult.EndOfInput(Error, parser);
                char c = (char) s.Peek();
                if (c == m)
                {
                    Debug.Print("Parsed char '" + m + "'");
                    ProcessChar(c);
                    return PResult.Succeeded(new Node(NodeType.Char, ((char)s.Read()).ToString()), s);
                }
                return PResult.Failed(Error.Unexpected(c, parser), c, s);
            };
            return parser;
        }
        /// <summary>
        /// Match a string
        /// </summary>
        /// <param name="s">String to match</param>
        /// <returns></returns>
        public static ParserBuilder String(string s)
        {
            Debug.Print("Looking for string '" + s + "'");
            ParserBuilder[] parsers = new ParserBuilder[s.Length];
            for (int i = 0; i < s.Length; i++) parsers[i] = Char(s[i]);
            ParserBuilder parser = SequenceOf(parsers);
            parser.Type = "string";
            parser.SpecificValue = s;
            return parser;
        }
        /// <summary>
        /// Matching all characters but a number of given ones. Special characters are taken from the Config.StandardCharMapping dictionary
        /// and are escaped using backslash '\'. All other escaped characters that are not in the escaped character mapping are invalid and causes a parse failure.
        /// </summary>
        /// <param name="nonAllowedChars">Non-allowed characters</param>
        /// <returns></returns>
        public static ParserBuilder CharBut(params char[] nonAllowedChars) => CharBut('\\', Config.StandardCharMapping, nonAllowedChars);

        /// <summary>
        /// Matching all characters but a number of given ones.
        /// If an escaped character is a special character, e.g \n, add that to the escaped character mapping dictionary like "\\n" = '\n'.
        /// All other escaped characters that are not in the escaped character mapping are invalid and causes a parse failure.
        /// </summary>
        /// <param name="escapeChar">Prefix to escape special characters</param>
        /// <param name="escapedCharMapping">Escaped character mapping</param>
        /// <param name="nonAllowedChars">Non-allowed characters</param>
        /// <returns>Single char parser</returns>
        public static ParserBuilder CharBut(char? escapeChar, Dictionary<string, char> escapedCharMapping, params char[] nonAllowedChars)
        {
            ParserBuilder parser = new ParserBuilder("characters but", nonAllowedChars.ArrayToString());
            parser.Func = s =>
            {
                if (s.Peek() == -1) return PResult.EndOfInput(Error, parser);
                char c = (char) s.Peek();
                if (escapeChar == c)
                {
                    s.Read(); // Consume escape character
                    if (s.Peek() == -1) return PResult.EndOfInput(Error, parser); // Peek the escaped character
                    string escaped = c.ToString() + (char)s.Read(); // Read the escaped character and concatenate it with the escape character, e.g "\" + 'n' = "\n"

                    if (escapedCharMapping.ContainsKey(escaped))
                    {
                        Debug.Print("Parsed char '" + escaped + "'");
                        ProcessChar(c);
                        return PResult.Succeeded(new Node(NodeType.Char, escapedCharMapping[escaped].ToString()), s);
                    }
                    return PResult.Failed(Error.Unexpected(escaped, parser), escaped, s);
                }
                if (nonAllowedChars.Contains(c)) return PResult.Failed(Error.Unexpected(c, parser), c, s);
                
                Debug.Print("Parsed char '" + c + "'");
                ProcessChar(c);
                return PResult.Succeeded(new Node(NodeType.Char, ((char)s.Read()).ToString()), s);
            };
            return parser;
        }

        public static ParserBuilder InfoBinder(string type, ParserBuilder parser) => InfoBinder(type, null, parser);
        public static ParserBuilder InfoBinder(string type, string specificValue, ParserBuilder parser)
        {
            parser.Type = type;
            parser.SpecificValue = specificValue;
            return parser;
        }


        /// <summary>
        /// A letter from A-z. Does not support unicode.
        /// </summary>
        public static ParserBuilder Letter = InRange('A', 'Z').InfoBinder("upper case letter").Or(InRange('a','z').InfoBinder("lower case letter")).InfoBinder("letter");
        /// <summary>
        /// A digit from 0-9.
        /// </summary>
        public static ParserBuilder Digit = InRange('0', '9', NodeType.Digit).InfoBinder("digit");
        /// <summary>
        /// List of separate letters
        /// </summary>
        public static ParserBuilder Letters = Letter.OneOrMore().InfoBinder("letters");
        /// <summary>
        /// String of just letters.
        /// </summary>
        public static ParserBuilder Word = Letters.Map(Combiner.String, NodeType.Word).InfoBinder("word");
        /// <summary>
        /// One or more digits.
        /// </summary>
        public static ParserBuilder Integer = Digit.OneOrMore().Map(Combiner.String, NodeType.Integer).InfoBinder("integer");
        /// <summary>
        /// An integer followed by a '.' and another integer.
        /// </summary>
        public static ParserBuilder Number = Integer.FollowedBy(Char('.')).FollowedBy(Integer).InfoBinder("number");

        /// <summary>
        /// Space or tab
        /// </summary>
        public static ParserBuilder WhiteSpace = Char(' ').Or(Char('\t')).InfoBinder("whitespace");
        /// <summary>
        /// Multiple whitespace characters
        /// </summary>
        public static ParserBuilder WhiteSpaces = WhiteSpace.ZeroOrMore().Map(Combiner.String, NodeType.WhiteSpace).InfoBinder("whitespaces");
        /// <summary>
        /// Line feed (newline \n) or carriage return \r.
        /// </summary>
        public static ParserBuilder LineEnding = Char('\n').Or(Char('\r')).InfoBinder("line ending");
        /// <summary>
        /// Match multiple line ending characters (\n or \r)
        /// </summary>
        public static ParserBuilder LineEndings = LineEnding.OneOrMore().Map(Combiner.String, NodeType.Newline).InfoBinder("line endings");

        #endregion
    }







    /// <summary>
    /// Generic Text parser.
    /// </summary>
    /// <typeparam name="TNode">Node Enum Types</typeparam>
    public class Parser<TNode> : AParser<ParserBuilder<TNode>, Combiner<TNode>, PResult<TNode>, TNode> where TNode : struct, IConvertible
    {
        public Parser(Func<StreamReader, PResult<TNode>> func) : base(func) { }
        public Parser(string type, Func<StreamReader, PResult<TNode>> func) : base(type, func) {}
        public Parser(string type, dynamic specificValue, Func<StreamReader, PResult<TNode>> func) : base(type, func) { SpecificValue = specificValue; }
        public Parser(string type, dynamic specificValue) : base(type) { SpecificValue = specificValue; }
        public Parser(string type) : base(type) { }


        /// <summary>
        /// Only match a specific character.
        /// </summary>
        /// <param name="m">Character to match</param>
        /// <param name="type">The type of the matched character</param>
        /// <returns>Single character parser</returns>
        public static ParserBuilder<TNode> Char(char m, TNode type)
        {
            ParserBuilder<TNode> parser = new ParserBuilder<TNode>("character", m);
            parser.Func = s =>
            {
                if (s.Peek() == -1) return PResult<TNode>.EndOfInput(Error, parser);
                char c = (char) s.Peek();
                if (c == m)
                {
                    Debug.Print("Parsed char '" + m + "'");
                    ProcessChar(c);
                    return PResult<TNode>.Succeeded(new Node<TNode>(type, ((char)s.Read()).ToString()), s);
                }
                return PResult<TNode>.Failed(Error.Unexpected(c, parser), c, s);
            };
            return parser;
        }
        /// <summary>
        /// Accepts character if in range of n and m and set a custom named NodeType.
        /// </summary>
        /// <param name="n">Lower character bound</param>
        /// <param name="m">Upper character bound></param>
        /// <param name="charType">Named type of matched character</param>
        /// <returns>Parser for any character between n and m</returns>
        public static ParserBuilder<TNode> InRange(char n, char m, TNode charType)
        {
            ParserBuilder<TNode> parser = new ParserBuilder<TNode>("character in range " + n + " to " + m);
            parser.Func = s =>
            {
                if (s.Peek() == -1) return PResult<TNode>.EndOfInput(Error, parser);
                char c = (char) s.Peek();

                if (c >= n && c <= m)
                {
                    Debug.Print("Parsed char '" + c + "'");
                    ProcessChar(c);
                    return PResult<TNode>.Succeeded(new Node<TNode>(charType, ((char)s.Read()).ToString()), s);
                }
                return PResult<TNode>.Failed(Error.Unexpected(c, parser), c, s);
            };
            return parser;
        }

        public static ParserBuilder<TNode> InfoBinder(string type, ParserBuilder<TNode> parser) => InfoBinder(type, null, parser);
        public static ParserBuilder<TNode> InfoBinder(string type, string specificValue, ParserBuilder<TNode> parser)
        {
            parser.Type = type;
            parser.SpecificValue = specificValue;
            return parser;
        }
    }
}
