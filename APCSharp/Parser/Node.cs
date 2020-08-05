using System.Collections.Generic;
using APCSharp.Util;

namespace APCSharp.Parser
{
    public enum NodeType
    {
        /// <summary>
        /// Value is damaged and invalid with no childnodes.
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
        /// Node Value is null but have childnodes.
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
        /// Node Value is of type String or Char with label WhiteSpace.
        /// </summary>
        WhiteSpace,
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
        Number
    }
    /// <summary>
    /// AST Node.
    /// </summary>
    public class Node
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
        public Node(NodeType type, dynamic value, params Node[] children)
        {
            Type = type;
            Value = value;
            Children = new List<Node>(children);
        }
        /// <summary>
        /// Create a new Node without childnodes.
        /// </summary>
        /// <param name="type">Type of Node</param>
        /// <param name="value">Value to hold</param>
        public Node(NodeType type, dynamic value)
        {
            Type = type;
            Value = value;
            Children = new List<Node>();
        }
        /// <summary>
        /// Childnodes of current Node.
        /// </summary>
        public List<Node> Children { get; }
        /// <summary>
        /// Node type.
        /// </summary>
        public NodeType Type { get; internal set; }
        /// <summary>
        /// Arbitrary Node value.
        /// </summary>
        public dynamic Value { get; }
        /// <summary>
        /// String formatted Node representation.
        /// </summary>
        /// <param name="indent">Indentation prefix</param>
        /// <returns>String formatted Node representation</returns>
        public string ToString(string indent)
        {
            if (Type == NodeType.Corrupted) return indent + "Node { Type: Corrupted }";

            string result = indent + $"Node {{ Type: {Type}";

            if (Value != null) result += $", Value: \"{ValueToString()}\" ({Value.GetType().ToString()}) ";
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
        /// <summary>
        /// Readable representation of the node value.
        /// </summary>
        /// <returns></returns>
        private string ValueToString()
        {
            string v = Value.ToString();
            return v.ReplaceAll('\n', "\\n").ReplaceAll('\r',"\\r").ReplaceAll('\t',"\\t");
        }
    }
}
