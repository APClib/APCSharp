using System;

namespace APCSharp.Parser
{
    /// <summary>
    /// Text parser.
    /// </summary>
    /// <typeparam name="TNode">Node Enum Types</typeparam>
    public class Parser<TNode> where TNode : struct, IConvertible
    {
        internal Func<string, PResult<TNode>> func;
        public PResult<TNode> Run(string s) => func(s);

        public Parser(Func<string, PResult<TNode>> func)
        {
            if (!typeof(TNode).IsEnum) throw new ArgumentException("TNode must be an enumerated type");

            this.func = func;
        }

        public Parser(string type, Func<string, PResult<TNode>> func) : this(func)
        {
            Type = type;
        }
        public Parser(string type, dynamic specificValue, Func<string, PResult<TNode>> func) : this(type, func)
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


        #region Meta Parsers
        public static ParserBuilder<TNode> InfoBinder(string type, ParserBuilder<TNode> parser) => InfoBinder(type, null, parser);
        public static ParserBuilder<TNode> InfoBinder(string type, string spesificValue, ParserBuilder<TNode> parser)
        {
            parser.Type = type;
            parser.SpecificValue = spesificValue;
            return parser;
        }

        /// <summary>
        /// Generate parser just in time. Allows for recursive calls and prevents stack overflows. 
        /// </summary>
        /// <returns></returns>
        public static ParserBuilder<TNode> Lazy(ParserBuilder<TNode> parser) => new ParserBuilder<TNode>((string s) => parser.Run(s));

        #endregion

        #region Static Methods

        public static ParserBuilder SequenceOf(params ParserBuilder[] parsers)
        {
            if (parsers.Length == 1) return parsers[0];
            int i = 1;
            ParserBuilder last = parsers[i - 1].FollowedBy(parsers[i]).Map();
            for (i++; i < parsers.Length; i++) last = last.FollowedBy(parsers[i]).Map();
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

        #endregion

        #region Preset Parsers
        /// <summary>
        /// Accepts character if in range of n and m and set a custom named NodeType.
        /// </summary>
        /// <param name="n">Lower character bound</param>
        /// <param name="m">Upper character bound></param>
        /// <param name="charType">Named type of matched character</param>
        /// <returns>Parser for any character between n and m</returns>
        public static ParserBuilder InRange(char n, char m, TNode charType)
        {
            ParserBuilder parser = new ParserBuilder("character in range " + n + " to " + m, null);
            parser.func = (string s) =>
            {
                if (!string.IsNullOrEmpty(s))
                {
                    ProcessChar(s[0]);
                    if (s[0] >= n && s[0] <= m) return PResult<TNode>.Succeeded(new Node<TNode>(charType, s[0]), s.Remove(0, 1));
                    else return PResult.Failed(Error.Error.Unexpected(s[0], parser), s.Remove(0, 1));
                }
                return PResult.Failed(Error.Error.Unexpected("end of input", parser), null);
            };
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

    }

    /// <summary>
    /// Default text parser enough for most parsers.
    /// </summary>
    public class Parser : Parser<NodeType>
    {
        public Parser(Func<string, PResult<NodeType>> func) : base(func) { }

        public Parser(string type, Func<string, PResult<NodeType>> func) : base(type, func) { }

        public Parser(string type, dynamic specificValue, Func<string, PResult<NodeType>> func) : base(type, func) { SpecificValue = specificValue; }

        public static Parser From<TNode>(Parser<TNode> parser) where TNode : struct, IConvertible
        {
            if (parser.func.GetType().Equals(typeof(Func<string, PResult<NodeType>>))) return new Parser(parser.Type, parser.SpecificValue, parser.func as Func<string, PResult<NodeType>>);
            throw new ArgumentException("Cannot cast Parser<" + typeof(TNode).Name + "> with " + parser.func.GetType() + " to Parser! Func must be of type Func<string, PResult>");
        }


        #region Preset Parsers
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
            parser.func = (string s) =>
            {
                if (!string.IsNullOrEmpty(s))
                {
                    if (s[0] == c)
                    {
                        ProcessChar(s[0]);
                        return PResult.Succeeded(new Node(NodeType.Char, c), s.Remove(0, 1));
                    }
                    else return PResult.Failed(Error.Error.Unexpected(s[0], parser), s.Remove(0, 1));
                }
                return PResult.Failed(Error.Error.Unexpected("end of input", parser), null);
            };
            return parser;
        }

        public static ParserBuilder String(string s)
        {
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
        /// <param name="escapeing">Allowed prefix characters to escape.</param>
        /// <param name="c">non-allowed characters</param>
        /// <returns></returns>
        public static ParserBuilder CharsBut(char[] escapeing, params char[] c)
        {
            ParserBuilder parser = new ParserBuilder("characters but", c, null);
            parser.func = (string s) =>
            {
                if (!string.IsNullOrEmpty(s)) // Refactor this 'if' into ProcessChar
                {
                    ProcessChar(s[0]);
                    int ci = 0;
                    for (int j = 0; j < escapeing.Length; j++)
                    {
                        if (s[ci] == escapeing[j]) ci = 1;
                    }
                    for (int i = 0; i < c.Length; i++)
                    {
                        if (s[ci] == c[i]) return PResult.Failed(Error.Error.Unexpected(s[0], parser), s.Remove(0, 1));
                    }
                    return PResult.Succeeded(new Node(NodeType.Char, s[ci]), s.Remove(0, 1));
                }
                return PResult.Failed(Error.Error.Unexpected("end of input", parser), null);
            };
            return parser;
        }


        public static ParserBuilder Letter = InRange('A', 'z').InfoBinder("letter");
        public static ParserBuilder Digit = InRange('0', '9', NodeType.Digit).InfoBinder("digit");

        public static ParserBuilder Letters = Letter.OneOrMore().InfoBinder("letters");
        public static ParserBuilder Word = Letters.Map(NodeType.Word).InfoBinder("word");
        public static ParserBuilder Integer = Digit.OneOrMore().Map(NodeType.Integer).InfoBinder("integer");
        public static ParserBuilder Number = Integer.FollowedBy(Char('.')).FollowedBy(Integer).InfoBinder("number");

        public static ParserBuilder WhiteSpace = AnyOf(
                                                    Char(' '),
                                                    Char('\t'),
                                                    Char('\n'),
                                                    Char('\r')
                                                ).InfoBinder("whitespace");

        public static ParserBuilder WhiteSpaces = WhiteSpace.ZeroOrMore().Map(Combiner.String, NodeType.WhiteSpace);

        #endregion
    }
}
