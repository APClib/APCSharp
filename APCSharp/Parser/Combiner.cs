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
        /// Node with one or more Childnodes.
        /// </summary>
        Lists
    }
    public class Combiner<TNode> where TNode : struct, IConvertible
    {
        /// <summary>
        /// Node combiner function
        /// </summary>
        internal Func<Node<TNode>, Node<TNode>, Node<TNode>> func { get; set; }

        /// <summary>
        /// Compatible Node types
        /// </summary>
        public CombinerType Type { get; }
        /// <summary>
        /// Create a new combiner function assuming Nodes are Elements.
        /// </summary>
        /// <param name="func">Node combiner function</param>
        public Combiner(Func<Node<TNode>, Node<TNode>, Node<TNode>> func) : this(CombinerType.Elements, func) { }
        /// <summary>
        /// Create a new combiner.
        /// </summary>
        /// <param name="type">Compatible Node types</param>
        /// <param name="func">Node combiner function</param>
        public Combiner(CombinerType type, Func<Node<TNode>, Node<TNode>, Node<TNode>> func)
        {
            this.func = func;
            Type = type;
        }
        /// <summary>
        /// Combine two nodes, n1 and n2, into a single node.
        /// </summary>
        /// <param name="n1">First Node</param>
        /// <param name="n2">Second Node</param>
        /// <returns>Node composed of two other Nodes</returns>
        public Node<TNode> Combine(Node<TNode> n1, Node<TNode> n2) => func(n1, n2);

        public static implicit operator Combiner(Combiner<TNode> n)
        {
            if (typeof(TNode).Equals(typeof(NodeType))) return n as Combiner;
            throw new ArgumentException("Cannot cast Combiner<" + typeof(TNode).Name + "> to Combiner! Must be Combiner<NodeType>");
        }
    }

    public class Combiner : Combiner<NodeType>
    {
        public Combiner(Func<Node<NodeType>, Node<NodeType>, Node<NodeType>> func) : base(func) { }

        public Combiner(CombinerType type, Func<Node<NodeType>, Node<NodeType>, Node<NodeType>> func) : base(type, func) { }

        /// <summary>
        /// Preset combiner that concatinates the two Nodes values to a string with a custom NodeType typelabel.
        /// </summary>
        /// <param name="type">Result type</param>
        /// <returns>A new string combiner with a custom result type</returns>
        public static Combiner TypedString(NodeType type) => new Combiner((Node<NodeType> p1, Node<NodeType> p2) => new Node<NodeType>(type, p1.Value.ToString() + p2.Value.ToString()));
        /// <summary>
        /// Preset combiner that concatinates the two Nodes values to a string.
        /// </summary>
        public static Combiner String = TypedString(NodeType.String);
        /// <summary>
        /// Preset combiner that creates a branch Node from two other Nodes.
        /// </summary>
        public static Combiner NodeList = new Combiner((Node<NodeType> p1, Node<NodeType> p2) => Node.List(p1, p2));
    }
}
