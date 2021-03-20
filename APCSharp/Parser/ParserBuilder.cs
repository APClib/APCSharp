#nullable enable
using System;
using APCSharp.Info;

namespace APCSharp.Parser
{
    public class ParserBuilder : Parser
    {
        public ParserBuilder(Func<string, PResult> Func) : base(Func) { }

        public ParserBuilder(string type, Func<string, PResult> Func) : base(type, Func) { }

        public ParserBuilder(string type, dynamic specificValue, Func<string, PResult> Func) : this(type, Func) { SpecificValue = specificValue.ToString(); }

        public new PResult Run(string s) => base.Run(s);

        #region Meta Parsers
        public ParserBuilder InfoBinder(string type) => InfoBinder(type, null, this);
        public ParserBuilder InfoBinder(string type, string specificValue) => InfoBinder(type, specificValue, this);
        #endregion

        #region Dynamic Methods
        /// <summary>
        /// Requires a parser match to be followed by another parser match.
        /// </summary>
        /// <param name="parser">Next parser to use for matching remaning input</param>
        /// <returns>A combined parser</returns>
        public ParserBuilder FollowedBy(ParserBuilder parser)
        {
            return new ParserBuilder((string s) => {
                PResult p1 = Func(s);
                if (p1.Success)
                {
                    PResult p2 = parser.Func(p1.Remaining);
                    if (p2.Success)
                    {
                        return PResult.Succeeded(Node.List(p1.ResultNode, p2.ResultNode), p2.Remaining);
                    }
                    else return p2;
                }
                else return p1;
            });
        }
        /// <summary>
        /// Preform two diffrent parsers on the same input and pick the first one that succeeds.
        /// </summary>
        /// <param name="parser">The other parser that can be matched</param>
        /// <returns></returns>
        public ParserBuilder Or(ParserBuilder parser)
        {
            return new ParserBuilder((string s) => {
                PResult p1 = Func(s); // Run this
                if (p1.Success)
                {
                    return PResult.Succeeded(p1.ResultNode, p1.Remaining);
                }
                else if (parser != null)
                {
                    PResult p2 = parser.Func(s);
                    if (p2.Success)
                    {
                        return PResult.Succeeded(p2.ResultNode, p2.Remaining);
                    }
                    return PResult.Failed(Error.Unexpected(p2.ErrorSequence, this, parser), p2.ErrorSequence, s);
                }
                return PResult.Failed(Error.Unexpected(s, this), p1.ErrorSequence, s);
            });
        }
        /// <summary>
        /// Generates a list of any Nodes that match the parser.
        /// </summary>
        /// <returns></returns>
        public ParserBuilder ZeroOrMore()
        {
            return new ParserBuilder((string s) => {
                Debug.Print("Looking for zero or more " + GetMatchString());
                PResult p = Func(s);
                Node root = Node.List();
                string remaining = s;
                while (p.Success)
                {
                    remaining = p.Remaining;
                    root.Children.Add(p.ResultNode);
                    if (string.IsNullOrEmpty(remaining)) break;
                    p = Func(p.Remaining);
                }
                return PResult.Succeeded(root, remaining);
            });
        }
        /// <summary>
        /// Generates a list of one or more Nodes that match the parser.
        /// </summary>
        /// <returns></returns>
        public ParserBuilder OneOrMore()
        {
            return new ParserBuilder((string s) => {
                Debug.Print("Looking for one or more " + GetMatchString());
                PResult p = Func(s);
                Node root = Node.List();
                if (!p.Success) return p; // PResult.Failed($"Expected at least one {GetMatchString()} but found '{(s.Length > 5 ? s.Substring(0, 5) + "..." : s)}'.", s);

                string remaining = string.Empty;
                while (p.Success)
                {
                    remaining = p.Remaining;
                    root.Children.Add(p.ResultNode);
                    if (string.IsNullOrEmpty(remaining)) break;
                    p = Func(p.Remaining);
                }
                return PResult.Succeeded(root, remaining);
            });
        }
        
        public ParserBuilder ListMap(Func<Node, Node?, Node> func) => Map(new Combiner(CombinerType.Lists, func), NodeType.String);
        public ParserBuilder ListMap(Func<Node, Node?, Node> func, NodeType namedType) => Map(new Combiner(CombinerType.Lists, func), namedType);
        public ParserBuilder Map(Func<Node, Node?, Node> func) => Map(new Combiner(func), NodeType.String);
        public ParserBuilder Map(Func<Node, Node?, Node> func, NodeType namedType) => Map(new Combiner(func), namedType);
        public ParserBuilder ListToString() => Map(Combiner.String, NodeType.String);
        public ParserBuilder Map(NodeType namedType) => Map(Combiner.String, namedType);

        /// <summary>
        /// Preforms a Function on every childnode of a parser result.
        /// </summary>
        /// <param name="combiner">The Function to preform</param>
        /// <param name="namedType"></param>
        /// <returns>A combined parser</returns>
        public ParserBuilder Map(Combiner combiner, NodeType namedType)
        {
            return new ParserBuilder((string s) => {
                PResult p = Func(s);
                Debug.Print("Mapping using " + (string.IsNullOrWhiteSpace(combiner.Name) ? "[Unnamed]" : combiner.Name) + " Combiner");
                if (!p.Success) return p;
                if (p.ResultNode.Children.Count == 0) return PResult.Succeeded(Node.Empty, p.Remaining);
                Node n = p.ResultNode.Children[0];
                if (p.ResultNode.Children.Count == 1) n = combiner.Combine(n, null); // Use single value
                else for (int i = 1; i < p.ResultNode.Children.Count; i++)
                {
                    if (combiner.Type != CombinerType.Lists && (n.Type == NodeType.List || p.ResultNode.Children[i].Type == NodeType.List)) throw new ArgumentException($"Cannot perform mapping of lists with the given combiner '{combiner.Func.Method.Name}'. Please use a combiner that works over elements.");
                    n = combiner.Combine(n, p.ResultNode.Children[i]);
                }
                n.Type = namedType;
                return PResult.Succeeded(n, p.Remaining);
            });
        }

        private ParserBuilder MaybeMatch()
        {
            return new ParserBuilder((string s) => {
                PResult p = Func(s);
                if (p.Success) return p;
                else return PResult.Empty(p.Remaining);
            });
        }

        public ParserBuilder RemoveEmptyMaybeMatches()
        {
            return new ParserBuilder((string s) => {
                PResult p = Func(s);
                for (int i = 0; i < p.ResultNode.Children.Count; i++)
                {
                    if (p.ResultNode.Children[i].Type == NodeType.Empty) p.ResultNode.Children.RemoveAt(i);
                }
                return p;
            });
        }
        /// <summary>
        /// Adds a parsers match if any. Use for optional matches.
        /// Automatically removes empty maybe matches.
        /// </summary>
        /// <returns></returns>
        public ParserBuilder Maybe() => MaybeMatch().RemoveEmptyMaybeMatches();
        #endregion

        public ParserBuilder ArbitraryWhitespaces() => FollowedBy(Parser.WhiteSpaces).Maybe().InfoBinder("whitespaces");
        public ParserBuilder IgnoredArbitraryWhitespaces() => ArbitraryWhitespaces().Map(Combiner.First, NodeType.List);
    }










    public class ParserBuilder<TNode> : Parser<TNode> where TNode : struct, IConvertible
    {
        public ParserBuilder(Func<string, PResult<TNode>> Func) : base(Func) { }
        public ParserBuilder(string type, Func<string, PResult<TNode>> Func) : base(type, Func) { }
        public ParserBuilder(string type, dynamic specificValue, Func<string, PResult<TNode>> Func) : this(type, Func)
        {
            SpecificValue = specificValue.ToString();
        }

        public static ParserBuilder<T> From<T>(Parser<T> parser) where T : struct, IConvertible
        {
            return new ParserBuilder<T>(parser.Type, parser.SpecificValue, parser.Func);
        }

        #region Dynamic Methods

        /// <summary>
        /// Expects the parser to be repeated n, number of times.
        /// </summary>
        /// <param name="n">Number of times</param>
        /// <returns>parser</returns>
        public ParserBuilder<TNode> Times(int n)
        {
            return new ParserBuilder<TNode>((string s) => {
                PResult<TNode> p = Func(s);
                for (int i = 1; i < n; i++)
                {
                    p = Func(s);
                    if (!p.Success) break;
                    s = p.Remaining;
                }
                return p;
            });
        }

        #endregion

        #region Meta Parsers
        public ParserBuilder<TNode> InfoBinder(string type) => InfoBinder(type, null, this);
        public ParserBuilder<TNode> InfoBinder(string type, string specificValue) => InfoBinder(type, specificValue, this);
        #endregion
    }
}
