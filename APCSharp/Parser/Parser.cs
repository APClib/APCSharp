using System;

namespace APCSharp.Parser
{
    public class Parser
    {
        internal Func<string, PResult> func;
        public PResult Run(string s) => func(s);

        public Parser(Func<string, PResult> func)
        {
            this.func = func;
        }

        public Parser(string type, Func<string, PResult> func) : this(func)
        {
            Type = type;
        }
        public Parser(string type, dynamic specificValue, Func<string, PResult> func) : this(type, func)
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

        #region Preset Parsers
        /// <summary>
        /// Accepts character if in range of n and m.
        /// </summary>
        /// <param name="n">Lower character bound</param>
        /// <param name="m">Upper character bound></param>
        /// <returns>Parser for any character between n and m</returns>
        public static ParserBuilder InRange(char n, char m) => InRange(n, m, NodeType.Char);
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
            parser.func = (string s) =>
            {
                if (!string.IsNullOrEmpty(s))
                {
                    ProcessChar(s[0]);
                    if (s[0] >= n && s[0] <= m) return PResult.Succeeded(new Node(charType, s[0]), s.Remove(0, 1));
                    else return PResult.Failed(Error.Error.Unexpected(s[0], parser), s.Remove(0, 1));
                }
                return PResult.Failed(Error.Error.Unexpected("end of input", parser), null);
            };
            return parser;
        }
        public static ParserBuilder Char(char c)
        {
            ParserBuilder parser = new ParserBuilder("character", c, null);
            parser.func = (string s) =>
            {
                if (!string.IsNullOrEmpty(s))
                {
                    ProcessChar(s[0]);
                    if (s[0] == c) return PResult.Succeeded(new Node(NodeType.Char, c), s.Remove(0, 1));
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

        public static ParserBuilder Letter = InRange('A', 'z').InfoBinder("letter");
        public static ParserBuilder Digit = InRange('0', '9', NodeType.Digit).InfoBinder("digit");

        public static ParserBuilder Letters = Letter.Many().InfoBinder("letters");
        public static ParserBuilder Word = Letters.Map(NodeType.Word).InfoBinder("word");
        public static ParserBuilder Integer = Digit.Many().Map(NodeType.Integer).InfoBinder("integer");
        public static ParserBuilder Number = Integer.FollowedBy(Char('.')).FollowedBy(Integer).InfoBinder("number");

        public static ParserBuilder WhiteSpace = AnyOf(
                                                    Char(' '),
                                                    Char('\t'),
                                                    Char('\n'),
                                                    Char('\r')
                                                ).InfoBinder("whitespace");
        public static ParserBuilder WhiteSpaces = WhiteSpace.Many().Map(Combiner.String, NodeType.WhiteSpace).InfoBinder("whitespaces");

        #endregion

        #region Meta Parsers
        public static ParserBuilder InfoBinder(string type, ParserBuilder parser) => InfoBinder(type, null, parser);
        public static ParserBuilder InfoBinder(string type, string spesificValue, ParserBuilder parser)
        {
            parser.Type = type;
            parser.SpecificValue = spesificValue;
            return parser;
        }

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

        #region Process Char

        private static void ProcessChar(char c)
        {
            Data.SharedData.LineColumn.NextColumn();
            switch(c)
            {
                case '\n':
                    Data.SharedData.LineColumn.NextLine();
                    break;
            }
        }

        #endregion
    }
}
