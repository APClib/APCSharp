#nullable enable
using System;
using System.Collections.Generic;
using System.Text;

namespace APCSharp.Parser
{
    public enum CombinerType
    {
        /// <summary>
        /// Node where the Value field is not expected to be null.
        /// </summary>
        Elements,
        /// <summary>
        /// Node with one or more Child nodes.
        /// </summary>
        Lists
    }

    public class Combiner
    {
        /// <summary>
        /// Compatible Node types
        /// </summary>
        public CombinerType Type { get; protected set; }
        public string Name { get; internal set; }
        /// <summary>
        /// Node combiner Function
        /// </summary>
        internal Func<Node, Node?, Node> Func { get; set; }

        /// <summary>
        /// Create a new combiner Function assuming Nodes are Elements.
        /// </summary>
        /// <param name="func">Node combiner Function</param>
        public Combiner(Func<Node, Node?, Node> func) : this(CombinerType.Elements, func) { }

        /// <summary>
        /// Create a new combiner.
        /// </summary>
        /// <param name="type">Compatible Node types</param>
        /// <param name="func">Node combiner Function</param>
        /// <param name="defaultValue">The default value for a node</param>
        public Combiner(CombinerType type, Func<Node, Node?, Node> func) : this(string.Empty, type,
            func) { }


        /// <summary>
        /// Create a new combiner.
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="type">Compatible Node types</param>
        /// <param name="func">Node combiner Function</param>
        private Combiner(string name, CombinerType type, Func<Node, Node?, Node> func)
        {
            Func = func;
            Name = name;
            Type = type;
        }
        protected Combiner() { }

        
        /// <summary>
        /// Combine two nodes, n1 and n2, into a single node.
        /// If there only is a single node, the other will be passed as null. This case must be handled by your function.
        /// </summary>
        /// <param name="n1">First Node</param>
        /// <param name="n2">Second Node. (May be null)</param>
        /// <returns>Node composed of two other Nodes</returns>
        public Node Combine(Node n1, Node? n2) => Func(n1, n2);

        public Combiner NameBinder(string name)
        {
            Name = name;
            return this;
        }



        /// <summary>
        /// Preset combiner that concatenates the two Nodes values to a string with a custom NodeType type-label.
        /// </summary>
        /// <param name="type">Result type</param>
        /// <returns>A new string combiner with a custom result type</returns>
        public static Combiner TypedString(NodeType type) => new Combiner((p1, p2) => new Node(type, (p1.Value?.ToString() ?? string.Empty) + (p2?.Value?.ToString() ?? string.Empty)));
        /// <summary>
        /// Preset combiner that concatenates the two Nodes values to a string.
        /// </summary>
        public static Combiner String = TypedString(NodeType.String).NameBinder("String");
        /// <summary>
        /// Preset combiner that creates a branch Node from two other Nodes.
        /// </summary>
        public static Combiner NodeList = new Combiner((p1, p2) => (p2 != null ? Node.List(p1, p2) : Node.List(p1))).NameBinder("List");
        /// <summary>
        /// Discard the second Node.
        /// </summary>
        public static Combiner First = new Combiner(CombinerType.Lists, (n1, n2) => n1).NameBinder("First");
        /// <summary>
        /// Discard the first Node.
        /// </summary>
        public static Combiner Second = new Combiner(CombinerType.Lists, (n1, n2) => n2 ?? n1).NameBinder("Second");
        
    }





    public class Combiner<TNode> : Combiner where TNode : struct, IConvertible
    {
        /// <summary>
        /// Node combiner Function
        /// </summary>
        internal new Func<Node<TNode>, Node<TNode>, Node<TNode>> Func { get; set; }

        /// <summary>
        /// Create a new combiner Function assuming Nodes are Elements.
        /// </summary>
        /// <param name="func">Node combiner Function</param>
        public Combiner(Func<Node<TNode>, Node<TNode>, Node<TNode>> func) : this(CombinerType.Elements, func) { }
        /// <summary>
        /// Create a new combiner.
        /// </summary>
        /// <param name="type">Compatible Node types</param>
        /// <param name="func">Node combiner Function</param>
        public Combiner(CombinerType type, Func<Node<TNode>, Node<TNode>, Node<TNode>> func) : this(string.Empty, type, func) {}
        /// <summary>
        /// Create a new combiner.
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="type">Compatible Node types</param>
        /// <param name="func">Node combiner Function</param>
        public Combiner(string name, CombinerType type, Func<Node<TNode>, Node<TNode>, Node<TNode>> func)
        {
            Func = func;
            Name = name;
            Type = type;
        }
        /// <summary>
        /// Combine two nodes, n1 and n2, into a single node.
        /// </summary>
        /// <param name="n1">First Node</param>
        /// <param name="n2">Second Node</param>
        /// <returns>Node composed of two other Nodes</returns>
        public Node<TNode> Combine(Node<TNode> n1, Node<TNode> n2) => Func(n1, n2);

    }
}
