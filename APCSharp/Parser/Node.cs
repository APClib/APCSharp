#nullable enable
using System;
using System.Collections.Generic;
using APCSharp.Util;

namespace APCSharp.Parser
{
    /// <summary>
    /// Default NodeType, should be sufficient for most parsers.
    /// </summary>
    public enum NodeType
    {
        /// <summary>
        /// Value is damaged and invalid with no child nodes.
        /// </summary>
        Corrupted,
        /// <summary>
        /// Used by Maybe() method when generating non-matching result.
        /// </summary>
        Empty,

        /// <summary>
        /// Node Value is of type Char.
        /// </summary>
        Char,
        /// <summary>
        /// Node Value is null but have child-nodes.
        /// </summary>
        List,
        /// <summary>
        /// Node Value is of type String or Char.
        /// </summary>
        String,
        /// <summary>
        /// Node Value is of type String or Char with label Word.
        /// </summary>
        Word,
        /// <summary>
        /// Node Value is of type String or Char with label Identifier.
        /// </summary>
        Identifier,
        /// <summary>
        /// Node Value is of type String or Char with label WhiteSpace.
        /// </summary>
        WhiteSpace,
        /// <summary>
        /// Node Value is of type String or Char with label Newline.
        /// </summary>
        Newline,
        /// <summary>
        /// Node Value is of type char with label Digit.
        /// </summary>
        Digit,
        /// <summary>
        /// Node Value is of type String or Char with label Integer.
        /// </summary>
        Integer,
        /// <summary>
        /// Node Value is of type String or Char with label Number.
        /// </summary>
        Number,
        /// <summary>
        /// Node Value is of type Object.
        /// </summary>
        Object,
        /// <summary>
        /// Node Value is a Pair of Nodes.
        /// </summary>
        Pair,
    }
    
    /// <summary>
    /// The kind of data a node contains.
    /// </summary>
    public enum NodeData
    {
        /// <summary>
        /// The node is missing both a value and child nodes
        /// </summary>
        NoData,
        /// <summary>
        /// Just a value
        /// </summary>
        Value,
        /// <summary>
        /// Just child nodes
        /// </summary>
        Children,
        /// <summary>
        /// Both value and child nodes
        /// </summary>
        ValueAndChildren
    }


    /// <summary>
    /// Default AST Node with NodeType.
    /// </summary>
    public class Node
    {
        /// <summary>
        /// List Node.
        /// </summary>
        /// <param name="nodes">Child nodes</param>
        /// <returns>Node with child nodes</returns>
        public static Node List(params Node[] nodes) => new Node(NodeType.List, nodes);

        /// <summary>
        /// Empty Node.
        /// </summary>
        public static Node Empty = new Node(NodeType.Empty, string.Empty);
        /// <summary>
        /// Corrupted Node.
        /// </summary>
        public static Node Corrupted = new Node();

        
        /// <summary>
        /// Create a new Node.
        /// </summary>
        /// <param name="type">Type of Node</param>
        /// <param name="value">Value to hold</param>
        /// <param name="children">Any child nodes</param>
        public Node(NodeType type, dynamic value, params Node[] children)
        {
            Type = type;
            Data = NodeData.ValueAndChildren;
            Value = value;
            Children = new List<Node>(children);
        }
        /// <summary>
        /// Create a new Node.
        /// </summary>
        /// <param name="type">Type of Node</param>
        /// <param name="children">Any child nodes</param>
        public Node(NodeType type, params Node[] children)
        {
            Type = type;
            Data = NodeData.Children;
            Value = null;
            Children = new List<Node>(children);
        }
        /// <summary>
        /// Create a new Node without child nodes.
        /// </summary>
        /// <param name="type">Type of Node</param>
        /// <param name="value">Value to hold</param>
        public Node(NodeType type, dynamic value)
        {
            Type = type;
            Data = NodeData.Value;
            Value = value;
            Children = new List<Node>();
        }
        /// <summary>
        /// Create a new Node without child nodes.
        /// </summary>
        /// <param name="type">Type of Node</param>
        public Node(NodeType type)
        {
            Type = type;
            Data = NodeData.NoData;
            Value = null;
            Children = new List<Node>();
        }

        protected Node() : this(NodeType.Corrupted) { }



        /// <summary>
        /// Child nodes of current Node.
        /// </summary>
        public List<Node> Children { get; set; }
        /// <summary>
        /// Node type.
        /// </summary>
        public NodeType Type { get; internal set; }
        /// <summary>
        /// Describes what data the node contains.
        /// </summary>
        public NodeData Data { get; internal set; }
        /// <summary>
        /// Arbitrary Node value.
        /// </summary>
        public dynamic? Value { get; internal set; }

        /// <summary>
        /// String formatted root Node representation.
        /// </summary>
        /// <returns>String formatted root Node representation.</returns>
        public string ToString(string indent)
        {
            if (Type == NodeType.Corrupted) return indent + "Node { Type: Corrupted }";
            string result = indent + $"{GetType().Name} {{ Type: {Type}"; 

            if (Value != null) result += $", Value: '{(Value.ToString() as string).ValueToHRT()}'";
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

    }








    
    
    /// <summary>
    /// AST Node with generic node type.
    /// </summary>
    /// <typeparam name="TNode">Enum describing nodes</typeparam>
    public class Node<TNode> : Node where TNode : struct, IConvertible
    {
        /// <summary>
        /// Generic node type.
        /// </summary>
        public new TNode Type { get; internal set; }
        /// <summary>
        /// Child nodes of current Node.
        /// </summary>
        public new List<Node<TNode>> Children { get; set; }

        /// <summary>
        /// Create a new Node.
        /// </summary>
        /// <param name="type">Type of Node</param>
        /// <param name="value">Value to hold</param>
        /// <param name="children">Any child nodes</param>
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
        /// <param name="children">Any child nodes</param>
        internal Node(TNode type, params Node<TNode>[] children)
        {
            if (!typeof(TNode).IsEnum) throw new ArgumentException("TNode must be an enumerated type");

            Type = type;
            Children = new List<Node<TNode>>(children);
        }
        /// <summary>
        /// Create a new Node without child nodes and leave value control to subclasses.
        /// </summary>
        /// <param name="type">Type of Node</param>
        internal Node(TNode type)
        {
            if (!typeof(TNode).IsEnum) throw new ArgumentException("T must be an enumerated type");

            Type = type;
            Children = new List<Node<TNode>>();
        }
        /// <summary>
        /// Create a new Node without child nodes.
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
    }
}