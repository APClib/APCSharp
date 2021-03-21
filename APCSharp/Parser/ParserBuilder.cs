#nullable enable
using System;
using APCSharp.Info;

namespace APCSharp.Parser
{
    public class ParserBuilder : Parser
    {
        public ParserBuilder(Func<string, PResult> func) : base(func) { }

        public ParserBuilder(string type, Func<string, PResult> func) : base(type, func) { }

        public ParserBuilder(string type, dynamic specificValue, Func<string, PResult> func) : this(type, func) { SpecificValue = specificValue.ToString(); }

        public new PResult Run(string s) => base.Run(s);

        #region Meta Parsers
        public ParserBuilder InfoBinder(string type) => InfoBinder(type, null, this);
        public ParserBuilder InfoBinder(string type, string specificValue) => InfoBinder(type, specificValue, this);
        #endregion

        /// <summary>
        /// Requires a parser match to be followed by another parser match.
        /// </summary>
        /// <param name="parser">Next parser to use for matching remaining input</param>
        /// <returns>A combined parser</returns>
        public ParserBuilder FollowedBy(ParserBuilder parser)
        {
            return new ParserBuilder(s => {
                PResult p1 = Func(s);
                if (p1.Success)
                {
                    PResult p2 = parser.Func(p1.Remaining);
                    if (p2.Success)
                    {
                        return PResult.Succeeded(Node.List(p1.AST, p2.AST), p2.Remaining);
                    }
                    else return p2;
                }
                else return p1;
            });
        }
        /// <summary>
        /// Preform two different parsers on the same input and pick the first one that succeeds.
        /// </summary>
        /// <param name="parser">The other parser that can be matched</param>
        /// <returns></returns>
        public ParserBuilder Or(ParserBuilder parser)
        {
            return new ParserBuilder(s => {
                PResult p1 = Func(s); // Run this
                if (p1.Success) return PResult.Succeeded(p1.AST, p1.Remaining);
                
                PResult p2 = parser.Func(s);
                if (p2.Success) return PResult.Succeeded(p2.AST, p2.Remaining);
                return PResult.Failed(Error.Unexpected(p2.ErrorSequence, this, parser), p2.ErrorSequence, s);
            });
        }
        /// <summary>
        /// Generates a list of any Nodes that match the parser.
        /// </summary>
        /// <returns></returns>
        public ParserBuilder ZeroOrMore()
        {
            return new ParserBuilder(s => {
                Debug.Print("Looking for zero or more " + GetMatchString());
                PResult p = Func(s);
                Node root = Node.List();
                string remaining = s;
                while (p.Success)
                {
                    remaining = p.Remaining;
                    root.Children.Add(p.AST);
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
            return new ParserBuilder(s => {
                Debug.Print("Looking for one or more " + GetMatchString());
                PResult p = Func(s);
                Node root = Node.List();
                if (!p.Success) return p;

                string remaining = string.Empty;
                while (p.Success)
                {
                    remaining = p.Remaining;
                    root.Children.Add(p.AST);
                    if (string.IsNullOrEmpty(remaining)) break;
                    p = Func(p.Remaining);
                }
                return PResult.Succeeded(root, remaining);
            });
        }
        
        /// <summary>
        /// Expects the parser to be repeated n, number of times.
        /// </summary>
        /// <param name="n">Number of times</param>
        /// <returns></returns>
        public ParserBuilder Times(int n)
        {
            return new ParserBuilder(s => {
                PResult p = Func(s);
                for (int i = 1; i < n; i++)
                {
                    p = Func(s);
                    if (!p.Success) break;
                    s = p.Remaining;
                }
                return p;
            });
        }
        
        public ParserBuilder ListMap(Func<Node, Node?, Node> func) => Map(new Combiner(CombinerType.OnChildren, func), NodeType.String);
        public ParserBuilder ListMap(Func<Node, Node?, Node> func, NodeType namedType) => Map(new Combiner(CombinerType.OnChildren, func), namedType);
        public ParserBuilder Map(CombinerType type, Func<Node, Node?, Node> func) => Map(new Combiner(type, func), NodeType.String);
        public ParserBuilder Map(CombinerType type, Func<Node, Node?, Node> func, NodeType namedType) => Map(new Combiner(type, func), namedType);
        public ParserBuilder ListToString() => ListToString(NodeType.String);
        public ParserBuilder ListToString(NodeType namedType) => Map(Combiner.String, namedType);

        /// <summary>
        /// Preforms a Function on every childnode of a parser result.
        /// </summary>
        /// <param name="combiner">The Function to preform</param>
        /// <param name="namedType"></param>
        /// <returns>A combined parser</returns>
        public ParserBuilder Map(Combiner combiner, NodeType namedType)
        {
            return new ParserBuilder(s => {
                PResult p = Func(s);
                Debug.Print("Mapping using " + (string.IsNullOrWhiteSpace(combiner.Name) ? "[Unnamed]" : combiner.Name) + " Combiner");
                if (!p.Success) return p;
                if (p.AST.Children.Count == 0) return PResult.Succeeded(Node.Empty, p.Remaining);
                Node n = p.AST.Children[0];
                if (p.AST.Children.Count == 1) n = combiner.Combine(n, null); // Use single value
                else for (int i = 1; i < p.AST.Children.Count; i++)
                {
                    if (combiner.Type != CombinerType.OnChildren && (n.Type == NodeType.List || p.AST.Children[i].Type == NodeType.List)) throw new ArgumentException($"Cannot perform mapping of lists with the given combiner '{combiner.Func.Method.Name}'. Please use a combiner that works over elements.");
                    n = combiner.Combine(n, p.AST.Children[i]);
                }
                n.Type = namedType;
                return PResult.Succeeded(n, p.Remaining);
            });
        }

        private ParserBuilder MaybeMatch()
        {
            return new ParserBuilder(s => {
                PResult p = Func(s);
                if (p.Success) return p;
                else return PResult.Empty(p.Remaining);
            });
        }

        private ParserBuilder RemoveEmptyMaybeMatches()
        {
            return new ParserBuilder(s => {
                PResult p = Func(s);
                for (int i = 0; i < p.AST.Children.Count; i++)
                {
                    if (p.AST.Children[i].Type == NodeType.Empty) p.AST.Children.RemoveAt(i);
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

        public ParserBuilder AnyWhitespaces() => FollowedBy(WhiteSpaces).Maybe().InfoBinder("any whitespaces");
        public ParserBuilder IgnoreAnyWhitespaces() => AnyWhitespaces().Map(Combiner.First, NodeType.List);
    }










    public class ParserBuilder<TNode> : Parser<TNode> where TNode : struct, IConvertible
    {
        public ParserBuilder(Func<string, PResult<TNode>> func) : base(func) { }
        public ParserBuilder(string type, Func<string, PResult<TNode>> func) : base(type, func) { }
        public ParserBuilder(string type, dynamic specificValue, Func<string, PResult<TNode>> func) : this(type, func)
        {
            SpecificValue = specificValue.ToString();
        }

        /// <summary>
        /// Expects the parser to be repeated n, number of times.
        /// </summary>
        /// <param name="n">Number of times</param>
        /// <returns>parser</returns>
        public ParserBuilder<TNode> Times(int n)
        {
            return new ParserBuilder<TNode>(s => {
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

        public ParserBuilder<TNode> InfoBinder(string type) => InfoBinder(type, null, this);
        public ParserBuilder<TNode> InfoBinder(string type, string specificValue) => InfoBinder(type, specificValue, this);

    }
}
