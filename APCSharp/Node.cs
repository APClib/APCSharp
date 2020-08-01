using System;
using System.Collections.Generic;
using System.Text;

namespace APCSharp
{
    public enum NodeType
    {
        Char,
        String,
        Corrupted
    }
    public class Node
    {
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
        public NodeType Type { get; }
        public dynamic Value { get; }

        public string ToString(string indent)
        {
            if (Type == NodeType.Corrupted) return indent + "Node { Type: Corrupted }";

            string result = indent + $"Node {{ Type: {Type}, Value: \"{Value}\" ({Value.GetType().ToString()})";

            if (Children.Count > 0)
            {
                result += ", Children:\n";
                for (int i = 0; i < Children.Count; i++) result += Children[i].ToString(indent + Config.Indentation) + '\n';
            }
            else result += ' ';

            return result + '}';
        }
        public override string ToString() => ToString(Config.Indentation);
    }
}
