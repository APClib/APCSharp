using System;
using APCSharp.Info;
using APCSharp.Util;

namespace APCSharp.Parser
{
    /// <summary>
    /// Default text parser enough for most parsers.
    /// </summary>
    public class Parser
    {
        internal Func<string, PResult> Func;
        public PResult Run(string s)
        {
            Data.SharedData.LineColumn.Reset();
            return Func(s);
        }
        
        protected Parser() {}
        public Parser(Func<string, PResult> func)
        {
            Func = func;
        }

        public Parser(string type, Func<string, PResult> Func) : this(Func)
        {
            Type = type;
        }
        public Parser(string type, dynamic specificValue, Func<string, PResult> Func) : this(type, Func)
        {
            SpecificValue = specificValue.ToString();
        }
        internal string Type { get; set; }
        internal string SpecificValue { get; set; }
        internal string GetMatchString()
        {
            if (!string.IsNullOrEmpty(SpecificValue) && !string.IsNullOrEmpty(Type)) return Type + $" '{SpecificValue}'";
            else if (!string.IsNullOrEmpty(SpecificValue)) return $"'{SpecificValue}'";
            else if (!string.IsNullOrEmpty(Type)) return Type;
            else return null;
        }
        

        /// <summary>
        /// Generate parser just in time. Allows for recursive calls and prevents stack overflows. 
        /// </summary>
        /// <returns></returns>
        public static ParserBuilder Lazy(ParserBuilder parser) => new ParserBuilder((string s) => parser.Func(s));


        #region Preset Parsers

        /// <summary>
        /// Accepts character if in range of n and m and set a custom named NodeType.
        /// </summary>
        /// <param name="n">Lower character bound</param>
        /// <param name="m">Upper character bound></param>
        /// <param name="charType">Named type of matched character</param>
        /// <returns>Parser for any character between n and m</returns>
        public static ParserBuilder InRange(char n, char m, NodeType charType)
        {
            ParserBuilder parser = new ParserBuilder("character in range " + n + " to " + m, null);
            parser.Func = (string s) =>
            {
                if (!string.IsNullOrEmpty(s))
                {
                    Debug.Print("Parsed char '" + s[0] + "'");
                    ProcessChar(s[0]);
                    if (s[0] >= n && s[0] <= m) return PResult.Succeeded(new Node(charType, s[0]), s.Remove(0, 1));
                    else return PResult.Failed(Error.Unexpected(s[0], parser), s[0].ToString(), s.Remove(0, 1));
                }
                return PResult.Failed(Error.Unexpected("end of input", parser), "", null);
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
        public static ParserBuilder Char(char c)
        {
            ParserBuilder parser = new ParserBuilder("character", c, null);
            parser.Func = (string s) =>
            {
                if (!string.IsNullOrEmpty(s))
                {
                    if (s[0] == c)
                    {
                        Debug.Print("Parsed char '" + c + "'");
                        ProcessChar(s[0]);
                        return PResult.Succeeded(new Node(NodeType.Char, c), s.Remove(0, 1));
                    }
                    else return PResult.Failed(Error.Unexpected(s[0], parser), s[0].ToString(), s.Remove(0, 1));
                }
                return PResult.Failed(Error.Unexpected("end of input", parser), "", null);
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

        public static ParserBuilder CharsBut(params char[] c) => CharsBut(new char[] { }, c);
        /// <summary>
        /// Matching all characters but a number of given ones. Those can be accepted anyways if they are escaped using a prefix character.
        /// </summary>
        /// <param name="escaping">Allowed prefix characters to escape.</param>
        /// <param name="c">non-allowed characters</param>
        /// <returns></returns>
        public static ParserBuilder CharsBut(char[] escaping, params char[] c)
        {
            ParserBuilder parser = new ParserBuilder("characters but", c.ArrayToString(), null);
            parser.Func = (string s) =>
            {
                if (!string.IsNullOrEmpty(s)) // Refactor this 'if' into ProcessChar
                {
                    ProcessChar(s[0]);
                    int ci = 0;
                    for (int j = 0; j < escaping.Length; j++)
                    {
                        if (s[ci] == escaping[j]) ci = 1;
                    }
                    for (int i = 0; i < c.Length; i++)
                    {
                        if (s[ci] == c[i]) return PResult.Failed(Error.Unexpected(s[0], parser), s[0].ToString(), s.Remove(0, 1));
                    }
                    Debug.Print("Parsed char '" + s[ci] + "'");
                    return PResult.Succeeded(new Node(NodeType.Char, s[ci]), s.Remove(0, 1));
                }
                return PResult.Failed(Error.Unexpected("end of input", parser), "", null);
            };
            return parser;
        }

        #region Meta Parsers
        public static ParserBuilder InfoBinder(string type, ParserBuilder parser) => InfoBinder(type, null, parser);
        public static ParserBuilder InfoBinder(string type, string specificValue, ParserBuilder parser)
        {
            parser.Type = type;
            parser.SpecificValue = specificValue;
            return parser;
        }

        #endregion

        #region Process Char

        internal static void ProcessChar(char c)
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
        public static ParserBuilder Word = Letters.Map(NodeType.Word).InfoBinder("word");
        /// <summary>
        /// One or more digits.
        /// </summary>
        public static ParserBuilder Integer = Digit.OneOrMore().Map(NodeType.Integer).InfoBinder("integer");
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
    public class Parser<TNode> : Parser where TNode : struct, IConvertible
    {
        internal new Func<string, PResult<TNode>> Func;

        public Parser(Func<string, PResult<TNode>> func)
        {
            if (!typeof(TNode).IsEnum) throw new ArgumentException("TNode must be an enumerated type");
            Func = func;
        }

        public Parser(string type, Func<string, PResult<TNode>> func) : this(func)
        {
            Type = type;
        }
        public Parser(string type, dynamic specificValue, Func<string, PResult<TNode>> func) : this(type, func)
        {
            SpecificValue = specificValue.ToString();
        }
        
        public new PResult<TNode> Run(string s)
        {
            Data.SharedData.LineColumn.Reset();
            return Func(s);
        }

        #region Preset Parsers
        /// <summary>
        /// Accepts character if in range of n and m and set a custom named NodeType.
        /// </summary>
        /// <param name="n">Lower character bound</param>
        /// <param name="m">Upper character bound></param>
        /// <param name="charType">Named type of matched character</param>
        /// <returns>Parser for any character between n and m</returns>
        public static ParserBuilder<TNode> InRange(char n, char m, TNode charType)
        {
            ParserBuilder<TNode> parser = new ParserBuilder<TNode>("character in range " + n + " to " + m, null);
            parser.Func = (string s) =>
            {
                if (!string.IsNullOrEmpty(s))
                {
                    Debug.Print("Parsed char '" + s[0] + "'");
                    ProcessChar(s[0]);
                    if (s[0] >= n && s[0] <= m) return PResult<TNode>.Succeeded(new Node<TNode>(charType, s[0]), s.Remove(0, 1));
                    else return PResult<TNode>.Failed(Error.Unexpected(s[0], parser), s.Remove(0, 1));
                }
                return PResult<TNode>.Failed(Error.Unexpected("end of input", parser), null);
            };
            return parser;
        }

        #endregion
        public static ParserBuilder<TNode> InfoBinder(string type, ParserBuilder<TNode> parser) => InfoBinder(type, null, parser);
        public static ParserBuilder<TNode> InfoBinder(string type, string specificValue, ParserBuilder<TNode> parser)
        {
            parser.Type = type;
            parser.SpecificValue = specificValue;
            return parser;
        }

        public Parser ToParser()
        {
            if (Func.GetType() == typeof(Func<string, PResult<NodeType>>)) return new Parser(Type, SpecificValue, Func as Func<string, PResult>);
            throw new ArgumentException("Cannot cast Parser<" + typeof(TNode).Name + "> with " + Func.GetType() + " to Parser! Func must be of type Func<string, PResult>");
        }
    }
}
