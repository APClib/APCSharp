using System;
using System.Collections.Generic;
using System.Text;

namespace APCSharp.Parser
{
    public class ParserBuilder<TNode> : Parser<TNode> where TNode : struct, IConvertible
    {
        public ParserBuilder(Func<string, PResult<TNode>> func) : base(func) { }
        public ParserBuilder(string type, Func<string, PResult<TNode>> func) : base(type, func) { }
        public ParserBuilder(string type, dynamic specificValue, Func<string, PResult<TNode>> func) : this(type, func)
        {
            SpecificValue = specificValue.ToString();
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
                PResult<TNode> p = func(s);
                for (int i = 1; i < n; i++)
                {
                    p = func(s);
                    if (!p.Success) break;
                    s = p.Remaining;
                }
                return p;
            });
        }

        #endregion

        #region Meta Parsers
        public ParserBuilder<TNode> InfoBinder(string type) => Parser<TNode>.InfoBinder(type, null);
        public ParserBuilder<TNode> InfoBinder(string type, string spesificValue) => InfoBinder(type, spesificValue, this);
        #endregion

        public static implicit operator ParserBuilder(ParserBuilder<TNode> n)
        {
            if (n.GetType().Equals(typeof(ParserBuilder<NodeType>))) return n as ParserBuilder;
            throw new ArgumentException("Cannot cast Node<" + typeof(TNode).Name + "> to Node! Must be Node<NodeType>");
        }
        public Parser<TNode> ToParser() => this as Parser<TNode>;
    }

    public class ParserBuilder : ParserBuilder<NodeType>
    {
        public ParserBuilder(Func<string, PResult<NodeType>> func) : base(func) { }

        public ParserBuilder(string type, Func<string, PResult<NodeType>> func) : base(type, func) { }

        public ParserBuilder(string type, dynamic specificValue, Func<string, PResult<NodeType>> func) : base(type, func) { SpecificValue = specificValue; }



        #region Dynamic Methods
        public ParserBuilder FollowedBy(ParserBuilder parser)
        {
            return new ParserBuilder((string s) => {
                PResult p1 = func(s);
                if (p1.Success)
                {
                    PResult p2 = parser.func(p1.Remaining);
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
                PResult p1 = func(s); // Run this
                if (p1.Success)
                {
                    return PResult.Succeeded(p1.ResultNode, p1.Remaining);
                }
                else
                {
                    PResult p2 = parser.func(s);
                    if (p2.Success)
                    {
                        return PResult.Succeeded(p2.ResultNode, p2.Remaining);
                    }
                }
                return PResult.Failed(Error.Error.Unexpected(s, this, parser), s);
            });
        }
        /// <summary>
        /// Generates a list of Nodes that match the parser
        /// </summary>
        /// <returns></returns>
        public ParserBuilder Many()
        {
            return new ParserBuilder((string s) => {
                PResult p = func(s);
                if (!p.Success) return p;

                Node root = Node.List();
                string remaining = string.Empty;
                while (p.Success)
                {
                    remaining = p.Remaining;
                    root.Children.Add(p.ResultNode);
                    p = func(p.Remaining);
                }
                if (root.Children.Count == 1) return PResult.Succeeded(root.Children[0], remaining);
                else return PResult.Succeeded(root, remaining);
            });
        }

        public ParserBuilder Map(Func<Node, Node, Node> func) => Map(new Combiner(func), NodeType.String);
        public ParserBuilder Map(Func<Node, Node, Node> func, NodeType namedType) => Map(new Combiner(func), namedType);
        public ParserBuilder Map() => Map(Combiner.String, NodeType.String);
        public ParserBuilder Map(NodeType namedType) => Map(Combiner.String, namedType);
        /// <summary>
        /// Preforms a function on every childnode of a parser result.
        /// </summary>
        /// <param name="combiner">The function to preform</param>
        /// <returns>A combined parser</returns>
        public ParserBuilder Map(Combiner combiner, NodeType namedType)
        {
            return new ParserBuilder((string s) => {
                PResult p = func(s);
                if (p.ResultNode.Children.Count < 2) return p;
                Node n = p.ResultNode.Children[0];
                for (int i = 1; i < p.ResultNode.Children.Count; i++)
                {
                    if (combiner.Type != CombinerType.Lists && (n.Type == NodeType.List || p.ResultNode.Children[i].Type == NodeType.List)) throw new ArgumentException($"Cannot perform mapping of lists with the given combiner '{combiner.func.Method.Name}'. Please use a combiner that works over elements.");
                    n = combiner.Combine(n, p.ResultNode.Children[i]);
                }
                n.Type = namedType;
                return new PResult(p.Success, n, p.Remaining);
            });
        }

        public ParserBuilder Maybe()
        {
            return new ParserBuilder((string s) => {
                PResult p = func(s);
                if (p.Success) return p;
                else return PResult.Empty(p.Remaining);
            });
        }

        public ParserBuilder RemoveEmptyMaybeMatches()
        {
            return new ParserBuilder((string s) => {
                PResult p = func(s);
                for (int i = 0; i < p.ResultNode.Children.Count; i++)
                {
                    if (p.ResultNode.Children[i].Type == NodeType.Empty) p.ResultNode.Children.RemoveAt(i);
                }
                return p;
            });
        }
        #endregion


        public static implicit operator Parser(ParserBuilder p) => p.ToParser() as Parser;
    }
}
