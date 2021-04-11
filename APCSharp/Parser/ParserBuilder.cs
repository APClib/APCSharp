#nullable enable
using System;
using System.IO;
using APCSharp.Info;
using APCSharp.Util;

namespace APCSharp.Parser
{
    public class ParserBuilderBase<TParserBuilder, TCombiner, TPResult, TNode, TNodeType, TNodeData> : AParser<TParserBuilder, TCombiner, TPResult, TNode, TNodeType, TNodeData>
    where TParserBuilder : ParserBuilderBase<TParserBuilder, TCombiner, TPResult, TNode, TNodeType, TNodeData>
    where TCombiner : ACombiner<TNode, TNodeType, TNodeData>
    where TPResult : PResultBase<TParserBuilder, TCombiner, TPResult, TNode, TNodeType, TNodeData>, new()
    where TNode : ANode<TNode, TNodeType, TNodeData>, new()
        where TNodeType : struct
        where TNodeData : struct
    {
        public ParserBuilderBase(Func<StreamReader, TPResult> func) : base(func) { }

        public ParserBuilderBase(string type, Func<StreamReader, TPResult> func) : base(type, func) { }
        public ParserBuilderBase(string type, dynamic specificValue, Func<StreamReader, TPResult> func) : this(type, func) { SpecificValue = specificValue; }
        public ParserBuilderBase(string type, dynamic specificValue) : base(type) { SpecificValue = specificValue; }
        public ParserBuilderBase(string type) : base(type) { }

        public new TPResult Run(string s) => base.Run(s);

        public TParserBuilder InfoBinder(string type) => InfoBinder(type, null, (TParserBuilder)this);
        public TParserBuilder InfoBinder(string type, string specificValue) => InfoBinder(type, specificValue, (TParserBuilder)this);
    }


    /// <summary>
    /// Chainable methods to build complex parser logic.
    /// </summary>
    public class ParserBuilder : ParserBuilderBase<ParserBuilder, Combiner, PResult, Node, NodeType, NodeData>, ICastable<Parser>
    {
        public ParserBuilder(): base(string.Empty) {}
        public ParserBuilder(Func<StreamReader, PResult> func) : base(func) { }

        public ParserBuilder(string type, Func<StreamReader, PResult> func) : base(type, func) { }
        public ParserBuilder(string type, dynamic specificValue, Func<StreamReader, PResult> func) : this(type, func) { SpecificValue = specificValue; }
        public ParserBuilder(string type, dynamic specificValue) : base(type) { SpecificValue = specificValue; }
        public ParserBuilder(string type) : base(type) { }

        public new PResult Run(string s) => base.Run(s);

        
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
                    PResult p2 = parser.Func(s);
                    if (p2.Success)
                    {
                        return PResult.Succeeded(Node.List(p1.AST, p2.AST), s);
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
                if (p1.Success) return PResult.Succeeded(p1.AST, s);
                
                PResult p2 = parser.Func(s); // Run next
                if (p2.Success) return PResult.Succeeded(p2.AST, s);
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
                PResult p = Func(s);
                Node root = Node.List();
                while (p.Success)
                {
                    root.Children.Add(p.AST);
                    if (s.EndOfStream) break;
                    p = Func(s);
                }
                return PResult.Succeeded(root, s);
            });
        }
        /// <summary>
        /// Generates a list of one or more Nodes that match the parser.
        /// </summary>
        /// <returns></returns>
        public ParserBuilder OneOrMore() => FollowedBy(ZeroOrMore()); // One match of the current parser followed by zero or more is the same as one or more.

        /// <summary>
        /// Expects the parser to be repeated n, number of times.
        /// </summary>
        /// <param name="n">Number of times</param>
        /// <returns></returns>
        public ParserBuilder Times(int n)
        {
            return new ParserBuilder(s => {
                Debug.Print("Repeating the parsing process " + n + " times.");
                Debug.Print("Iteration: 1");
                PResult p = Func(s);
                for (int i = 1; i <= n; i++)
                {
                    Debug.Print("Iteration: " + (i + 1));
                    if (!p.Success || s.EndOfStream) break;
                    p = Func(s);
                }
                return p;
            });
        }
        
        /// <summary>
        /// Preforms a combiner function on every child node of a parser result and labeling the result with a named type.
        /// </summary>
        /// <param name="combiner">Map combiner</param>
        /// <param name="namedType">Result type</param>
        /// <returns></returns>
        public ParserBuilder Map(Combiner combiner, NodeType namedType)
        {
            return new ParserBuilder(s => {
                PResult p = Func(s);
                Debug.Print("Mapping using " + (string.IsNullOrWhiteSpace(combiner.Name) ? "[Unnamed]" : combiner.Name) + " Combiner");
                if (!p.Success) return p;
                if (p.AST.Children.Count == 0) return PResult.Succeeded(Node.Empty, s); // If the map was given a node without any child nodes, just return an empty string node
                // Map over all child nodes
                Node n = p.AST.Children[0];
                if (p.AST.Children.Count == 1) n = combiner.Combine(n, null); // Use single value
                else for (int i = 1; i < p.AST.Children.Count; i++) n = combiner.Combine(n, p.AST.Children[i]);
                n.Type = namedType;
                return PResult.Succeeded(n, s);
            });
        }
        /// <summary>
        /// Preforms a combiner function on every child node of a parser result assuming the result is a string.
        /// </summary>
        /// <param name="combiner">Map combiner</param>
        /// <returns></returns>
        public ParserBuilder Map(Combiner combiner) => Map(combiner, NodeType.String);

        /// <summary>
        /// Map over the previous parsers AST with a function expecting certain node fields to be set, and label the result with a named type.
        /// </summary>
        /// <param name="type">Node expectations</param>
        /// <param name="func">Map function</param>
        /// <param name="namedType">Result label</param>
        /// <returns></returns>
        public ParserBuilder Map(Func<Node, Node?, Node> func, NodeType namedType) => Map(new Combiner(func), namedType);
        /// <summary>
        /// Map over the previous parsers AST with a function expecting certain node fields to be set, and assume the result is a string.
        /// </summary>
        /// <param name="type">Node expectations</param>
        /// <param name="func">Map function</param>
        /// <returns></returns>
        public ParserBuilder Map(Func<Node, Node?, Node> func) => Map(func, NodeType.String);
        /// <summary>
        /// Map over the previous parsers AST with a function expecting nodes to have child nodes, and label the result with a named type.
        /// </summary>
        /// <param name="func">Map function</param>
        /// <param name="namedType">Result label</param>
        /// <returns></returns>
        public ParserBuilder MapChildren(Func<Node, Node?, Node> func, NodeType namedType) => Map(func, namedType);
        /// <summary>
        /// Map over the previous parsers AST with a function expecting nodes to have child nodes, and assume the result is a string.
        /// </summary>
        /// <param name="func">Map function</param>
        /// <returns></returns>
        public ParserBuilder MapChildren(Func<Node, Node?, Node> func) => MapChildren(func, NodeType.String);
        /// <summary>
        /// Map over the previous parsers AST with a function expecting nodes to have a value, and label the result with a named type.
        /// </summary>
        /// <param name="func">Map function</param>
        /// <param name="namedType">Result label</param>
        /// <returns></returns>
        public ParserBuilder MapValues(Func<Node, Node?, Node> func, NodeType namedType) => Map(func, namedType);
        /// <summary>
        /// Map over the previous parsers AST with a function expecting nodes to have a value, and assume the result is a string.
        /// </summary>
        /// <param name="func">Map function</param>
        /// <returns></returns>
        public ParserBuilder MapValues(Func<Node, Node?, Node> func) => MapValues(func, NodeType.String);
        /// <summary>
        /// Map over the previous parsers AST and produce a string labeled with a named type.
        /// </summary>
        /// <returns></returns>
        public ParserBuilder ListToString(NodeType namedType) => Map(Combiner.String, namedType);
        /// <summary>
        /// Map over the previous parsers AST and produce a string.
        /// </summary>
        /// <returns></returns>
        public ParserBuilder ListToString() => ListToString(NodeType.String);
        /// <summary>
        /// Maps over a list of nodes and moves every node or child nodes into a new list.
        /// </summary>
        /// <returns></returns>
        public ParserBuilder Flatten() => Map(Combiner.Flatten);


        private ParserBuilder MaybeMatch()
        {
            return new ParserBuilder(s => {
                PResult p = Func(s);
                return p.Success ? p : PResult.Empty(s);
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
        /// <summary>
        /// Match a any amount of trailing whitespace, this will be appended on the previous parsed result.
        /// If you only want to allow trailing whitespaces, use IgnoredWhitespaces().
        /// </summary>
        /// <returns></returns>
        public ParserBuilder AnyWhitespaces() => FollowedBy(Parser.WhiteSpaces).Maybe().ListToString().InfoBinder("any whitespaces");

        /// <summary>
        /// Match and ignore any amount of whitespace
        /// </summary>
        /// <returns></returns>
        public ParserBuilder IgnoredWhitespaces() => FollowedBy(Parser.WhiteSpaces).Map(Combiner.First);

        public Parser Cast()
        {
            return new Parser
            {
                Func = Func,
                SpecificValue = SpecificValue,
                Type = Type
            };
        }

        public static implicit operator Parser(ParserBuilder p) => p.Cast();
    }









    
    /// <summary>
    /// Chainable methods to build complex generic parser logic.
    /// </summary>
    /// <typeparam name="TNode">Enum for custom node types</typeparam>
    public class ParserBuilder<TNode, TNodeType, TNodeData> : ParserBuilderBase<ParserBuilder<TNode, TNodeType, TNodeData>, Combiner<TNode, TNodeType, TNodeData>, PResult<TNode, TNodeType, TNodeData>, TNode, TNodeType, TNodeData>
        where TNode : ANode<TNode, TNodeType, TNodeData>, new()
            where TNodeType : struct
            where TNodeData : struct
    {
        public ParserBuilder(Func<StreamReader, PResult<TNode, TNodeType, TNodeData>>? func) : base(func) { }
        public ParserBuilder(string type, Func<StreamReader, PResult<TNode, TNodeType, TNodeData>> func) : base(type, func) { }
        public ParserBuilder(string type, dynamic specificValue, Func<StreamReader, PResult<TNode, TNodeType, TNodeData>> func) : this(type, func)
        {
            SpecificValue = specificValue;
        }
        public ParserBuilder(string type, dynamic specificValue) : base(type)
        {
            SpecificValue = specificValue;
        }
        public ParserBuilder(string type) : base(type) { }

        public ParserBuilder<TNode, TNodeType, TNodeData> FollowedBy(ParserBuilder<TNode, TNodeType, TNodeData> parser)
        {
            throw new NotImplementedException();
        }

        public ParserBuilder<TNode, TNodeType, TNodeData> Or(ParserBuilder<TNode, TNodeType, TNodeData> parser)
        {
            throw new NotImplementedException();
        }

        public ParserBuilder<TNode, TNodeType, TNodeData> ZeroOrMore()
        {
            throw new NotImplementedException();
        }

        public ParserBuilder<TNode, TNodeType, TNodeData> OneOrMore()
        {
            throw new NotImplementedException();
        }
        
        /// <summary>
        /// Expects the parser to be repeated n, number of times.
        /// </summary>
        /// <param name="n">Number of times</param>
        /// <returns></returns>
        public ParserBuilder<TNode, TNodeType, TNodeData> Times(int n)
        {
            return new ParserBuilder<TNode, TNodeType, TNodeData>(s => {
                Debug.Print("Repeating the parsing process " + n + " times.");
                Debug.Print("Iteration: 1");
                PResult<TNode, TNodeType, TNodeData> p = Func(s);
                for (int i = 1; i <= n; i++)
                {
                    Debug.Print("Iteration: " + (i + 1));
                    if (!p.Success || s.EndOfStream) break;
                    p = Func(s);
                }
                return p;
            });
        }
        
        /// <summary>
        /// Preforms a combiner function on every child node of a parser result and labeling the result with a named type.
        /// </summary>
        /// <param name="combiner">Map combiner</param>
        /// <param name="namedType">Result type</param>
        /// <returns></returns>
        public ParserBuilder<TNode, TNodeType, TNodeData> Map(Combiner<TNode, TNodeType, TNodeData> combiner, TNode namedType)
        {
            throw new NotImplementedException();
        }

        public ParserBuilder<TNode, TNodeType, TNodeData> Maybe()
        {
            throw new NotImplementedException();
        }

        public ParserBuilder<TNode, TNodeType, TNodeData> AnyWhitespaces()
        {
            throw new NotImplementedException();
        }

        public ParserBuilder<TNode, TNodeType, TNodeData> IgnoreAnyWhitespaces()
        {
            throw new NotImplementedException();
        }
    }
}
