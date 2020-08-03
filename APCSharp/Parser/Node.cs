using System.Collections.Generic;
using APCSharp.Util;

namespace APCSharp.Parser
{
    public enum NodeType
    {
        Corrupted,
        Empty,
        Char,
        List,
        String,
        Word,
        WhiteSpace,
        Digit,
        Integer,
        Number
    }
    public class Node
    {
        public static Node List(params Node[] nodes) => new Node(NodeType.List, null, nodes);
        public static Node Empty = new Node(NodeType.Empty, string.Empty);
        public static Node Corrupted = new Node(NodeType.Corrupted, null);
        public Node(NodeType type, dynamic value, params Node[] children)
        {
            Type = type;
            Value = value;
            Children = new List<Node>(children);
        }
        public Node(NodeType type, dynamic value)
        {
            Type = type;
            Value = value;
            Children = new List<Node>();
        }

        public List<Node> Children { get; }
        public NodeType Type { get; internal set; }
        public dynamic Value { get; }

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
        public override string ToString() => ToString("");

        private string ValueToString()
        {
            string v = Value.ToString();
            return v.ReplaceAll('\n', "\\n").ReplaceAll('\r',"\\r").ReplaceAll('\t',"\\t");
        }
    }
}
