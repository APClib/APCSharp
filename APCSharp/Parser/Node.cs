using System;
using System.Collections.Generic;
using APCSharp.Util;

namespace APCSharp.Parser
{
    public class Node<TNode> where TNode : struct, IConvertible
    {
        /// <summary>
        /// Create a new Node.
        /// </summary>
        /// <param name="type">Type of Node</param>
        /// <param name="value">Value to hold</param>
        /// <param name="children">Any childnodes</param>
        public Node(TNode type, dynamic value, params Node<TNode>[] children)
        {
            if (!typeof(TNode).IsEnum) throw new ArgumentException("TNode must be an enumerated type");

            Type = type;
            Value = value;
            Children = new List<Node<TNode>>(children);
        }
        /// <summary>
        /// Create a new Node but leave value control to subclasses.
        /// </summary>
        /// <param name="type">Type of Node</param>
        /// <param name="children">Any childnodes</param>
        internal Node(TNode type, params Node<TNode>[] children)
        {
            if (!typeof(TNode).IsEnum) throw new ArgumentException("TNode must be an enumerated type");

            Type = type;
            Children = new List<Node<TNode>>(children);
        }
        /// <summary>
        /// Create a new Node without childnodes and leave value control to subclasses.
        /// </summary>
        /// <param name="type">Type of Node</param>
        /// <param name="value">Value to hold</param>
        internal Node(TNode type)
        {
            if (!typeof(TNode).IsEnum) throw new ArgumentException("T must be an enumerated type");

            Type = type;
            Children = new List<Node<TNode>>();
        }
        /// <summary>
        /// Create a new Node without childnodes.
        /// </summary>
        /// <param name="type">Type of Node</param>
        /// <param name="value">Value to hold</param>
        public Node(TNode type, dynamic value)
        {
            if (!typeof(TNode).IsEnum) throw new ArgumentException("T must be an enumerated type");

            Type = type;
            Value = value;
            Children = new List<Node<TNode>>();
        }
        /// <summary>
        /// Childnodes of current Node.
        /// </summary>
        public List<Node<TNode>> Children { get; set; }
        /// <summary>
        /// Node type.
        /// </summary>
        public TNode Type { get; internal set; }
        /// <summary>
        /// Arbitrary Node value.
        /// </summary>
        public dynamic Value { get; internal set; }

        /// <summary>
        /// String formatted root Node representation.
        /// </summary>
        /// <returns>String formatted root Node representation.</returns>
        public string ToString(string indent)
        {
            string result = indent + $"Node {{ Type: {Type}";

            if (Value != null) result += $", Value: \"{(Value.ToString() as string).ValueToHRT()}\" ({Value.GetType().ToString()}) ";
            if (Children.Count > 0)
            {
                result += ", Children:\n";
                for (int i = 0; i < Children.Count; i++) result += Children[i].ToString(indent + Config.Indentation) + '\n';
                result += indent;
            }

            return result + "}";
        }
        /// <summary>
        /// String formatted root Node representation.
        /// </summary>
        /// <returns>String formatted root Node representation.</returns>
        public override string ToString() => ToString("");


        public static implicit operator Node(Node<TNode> n)
        {
            if (typeof(TNode).Equals(typeof(NodeType))) return Node.From(n as Node<NodeType>);
            throw new ArgumentException("Cannot cast Node<" + typeof(TNode).Name + "> to Node! Must be Node<NodeType>");
        }
    }

    /// <summary>
    /// Default AST Node.
    /// </summary>
    public class Node : Node<NodeType>
    {
        /// <summary>
        /// List Node.
        /// </summary>
        /// <param name="nodes">Childnodes</param>
        /// <returns>Node with childnodes</returns>
        public static Node List(params Node[] nodes) => new Node(NodeType.List, null, nodes);
        /// <summary>
        /// Empty Node.
        /// </summary>
        public static Node Empty = new Node(NodeType.Empty, string.Empty);
        /// <summary>
        /// Corrupted Node.
        /// </summary>
        public static Node Corrupted = new Node(NodeType.Corrupted, null);
        /// <summary>
        /// Create a new Node.
        /// </summary>
        /// <param name="type">Type of Node</param>
        /// <param name="value">Value to hold</param>
        /// <param name="children">Any childnodes</param>
        public Node(NodeType type, dynamic value, params Node[] children) : base(type, children) { Value = value; }
        public Node(NodeType type, dynamic value, params Node<NodeType>[] children) : base(type, children) { Value = value; }
        /// <summary>
        /// Create a new Node without childnodes.
        /// </summary>
        /// <param name="type">Type of Node</param>
        /// <param name="value">Value to hold</param>
        public Node(NodeType type, dynamic value) : base(type) { Value = value; }
        internal static Node From(Node<NodeType> n)
        {
            return new Node(n.Type, n.Value, n.Children.ToArray());
        }

        /// <summary>
        /// String formatted Node representation.
        /// </summary>
        /// <param name="indent">Indentation prefix</param>
        /// <returns>String formatted Node representation</returns>
        public new string ToString(string indent)
        {
            if (Type == NodeType.Corrupted) return indent + "Node { Type: Corrupted }";
            return base.ToString(indent);
        }

    }
}