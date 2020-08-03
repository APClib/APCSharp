using System;
using System.Collections.Generic;
using System.Text;

namespace APCSharp.Parser
{
    public enum CombinerType
    {
        Elements,
        Lists
    }
    public class Combiner
    {
        internal readonly Func<Node, Node, Node> func;

        public static Combiner TypedString(NodeType type) => new Combiner((Node p1, Node p2) => new Node(type, p1.Value.ToString() + p2.Value.ToString()));
        public static Combiner String = TypedString(NodeType.String);
        public static Combiner Node = new Combiner((Node p1, Node p2) => new Node(NodeType.List, null, p1, p2));

        public CombinerType Type { get; }

        public Combiner(Func<Node, Node, Node> func) : this(CombinerType.Elements, func) { }
        public Combiner(CombinerType type, Func<Node, Node, Node> func)
        {
            this.func = func;
            Type = type;
        }

        public Node Combine(Node n1, Node n2) => func(n1, n2);
    }
}
