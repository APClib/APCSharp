#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace APCSharp.Parser
{
    public abstract class ACombiner<TNode, TNodeType, TNodeData> where TNode : ANode<TNode, TNodeType, TNodeData>, new()
        where TNodeType : struct
        where TNodeData : struct
    {
        public string Name { get; internal set; }
        /// <summary>
        /// Node combiner Function
        /// </summary>
        internal Func<TNode, TNode?, TNode> Func { get; set; }
    }
    /// <summary>
    /// AST Transformation on nodes or lists of nodes.
    /// </summary>
    public class Combiner : ACombiner<Node, NodeType, NodeData>
    {

        /// <summary>
        /// Create a new combiner.
        /// The function must handle all cases of nodes. With values or with child nodes, neither or both.
        /// </summary>
        /// <param name="func">Node combiner Function</param>
        public Combiner(Func<Node, Node?, Node> func) : this(string.Empty, func) { }


        /// <summary>
        /// Create a new combiner.
        /// The function must handle all cases of nodes. With values or with child nodes, neither or both.
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="func">Node combiner Function</param>
        private Combiner(string name, Func<Node, Node?, Node> func)
        {
            Func = func;
            Name = name;
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
        /// <summary>
        /// Name a combiner
        /// </summary>
        /// <param name="name">name</param>
        /// <returns>Named combiner</returns>
        public Combiner NameBinder(string name)
        {
            Name = name;
            return this;
        }

        public static Node RecursiveMap(Node n, Combiner combiner) => new ParserBuilder(s => PResult.Succeeded(n, s)).Map(combiner).Func(StreamReader.Null).AST;



        /// <summary>
        /// Preset combiner that concatenates the two Nodes values to a string with a custom NodeType type-label.
        /// </summary>
        /// <param name="type">Result type</param>
        /// <returns>A new string combiner with a custom result type</returns>
        public static Combiner TypedString(NodeType type) => new Combiner((p1, p2) => new Node(type, TypedStringProcess(p1, type) + TypedStringProcess(p2, type)));

        private static string TypedStringProcess(Node? n, NodeType type)
        {
            if (n == null) return string.Empty;
            switch (n.Data)
            {
                case NodeData.Value: return n.Value?.ToString() ?? string.Empty;
                case NodeData.Children: return RecursiveMap(n, TypedString(type)).Value?.ToString() ?? string.Empty;
                case NodeData.ValueAndChildren: return n.Value?.ToString() ?? string.Empty + RecursiveMap(n, TypedString(type)).Value?.ToString() ?? string.Empty;
                default: return string.Empty;
            }
        }
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
        public static Combiner First = new Combiner((n1, n2) => n1).NameBinder("First");
        /// <summary>
        /// Discard the first Node.
        /// </summary>
        public static Combiner Second = new Combiner((n1, n2) => n2 ?? n1).NameBinder("Second");
        
    }



    
    /// <summary>
    /// AST Transformation on generic nodes or lists of generic nodes.
    /// </summary>
    /// <typeparam name="TNode">Enum for custom node types</typeparam>
    public class Combiner<TNode, TNodeType, TNodeData> : ACombiner<TNode, TNodeType, TNodeData> where TNode : ANode<TNode, TNodeType, TNodeData>, new()
        where TNodeType : struct
        where TNodeData : struct
    {
        /// <summary>
        /// Node combiner Function
        /// </summary>
        internal new Func<TNode, TNode?, TNode> Func { get; set; }

        /// <summary>
        /// Create a new combiner.
        /// The function must handle all cases of nodes. With values or with child nodes, neither or both.
        /// </summary>
        /// <param name="func">Node combiner Function</param>
        public Combiner(Func<TNode, TNode?, TNode> func) : this(string.Empty, func) {}
        /// <summary>
        /// Create a new combiner.
        /// The function must handle all cases of nodes. With values or with child nodes, neither or both.
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="func">Node combiner Function</param>
        public Combiner(string name, Func<TNode, TNode?, TNode> func)
        {
            Func = func;
            Name = name;
        }
        /// <summary>
        /// Combine two nodes, n1 and n2, into a single node.
        /// </summary>
        /// <param name="n1">First Node</param>
        /// <param name="n2">Second Node</param>
        /// <returns>Node composed of two other Nodes</returns>
        public TNode Combine(TNode n1, TNode? n2) => Func(n1, n2);

    }
}
