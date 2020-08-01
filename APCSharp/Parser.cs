using System;
using System.Collections.Generic;

namespace APCSharp
{
    public class Parser
    {
        private readonly Func<string, PResult> func;
        public PResult Run(string s) => func(s);
        public PResult RunSafe(string s, bool debug = true) {
            try
            {
                return func(s);
            }
            catch (ParserException e)
            {
                if (debug) Console.WriteLine(e.Message);
                return e.Result;
            }
        }

        public Parser(Func<string, PResult> func)
        {
            this.func = func;
        }

        #region Preset Parsers

        public static ParserBuilder Char(char c) => new ParserBuilder((string s) => {
            ProcessChar(s[0]);
            if (s[0] == c) return new PResult(true, new Node(NodeType.Char, c), s.Remove(0, 1));
            else throw Error.Unexpected("character", s[0], c, s);
        });

        public static ParserBuilder String(string s)
        {
            ParserBuilder[] parsers = new ParserBuilder[s.Length];
            for (int i = 0; i < s.Length; i++) parsers[i] = Char(s[i]);
            return SequenceOf(parsers);
        }

        #endregion

        #region Static Methods

        public static ParserBuilder SequenceOf(params ParserBuilder[] parsers)
        {
            int i = 1;
            ParserBuilder last = parsers[i - 1].And(parsers[i], StringCombiner);
            for (i++; i < parsers.Length; i++) last = last.And(parsers[i], StringCombiner);
            return last;
        }

        #endregion

        #region Combiners

        public static Node StringCombiner(Node p1, Node p2) => new Node(NodeType.String, p1.Value.ToString() + p2.Value.ToString());

        #endregion

        #region Process Char

        private static void ProcessChar(char c)
        {
            SharedData.LineColumn.NextColumn();
            switch(c)
            {
                case '\n':
                    SharedData.LineColumn.NextLine();
                    break;
            }
        }

        #endregion
    }
}
