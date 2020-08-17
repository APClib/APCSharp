using System;
namespace APCSharp.Parser
{
    public class PResult<TNode> where TNode : struct, IConvertible
    {
        public static PResult<TNode> Succeeded(Node<TNode> node, string rest) => new PResult<TNode>(true, node, rest);
        public static PResult<TNode> Failed(string errorMsg, string rest) => new PResult<TNode>(errorMsg, rest);
        public PResult(bool success, string errorMessage, Node<TNode> node, string rest)
        {
            ResultNode = node;
            Remaining = rest;
            Success = success;
            ErrorMessage = errorMessage;
        }
        public PResult(bool success, Node<TNode> node, string rest) : this(success, null, node, rest) { }
        public PResult(string errorMessage, string rest) : this(false, errorMessage, default(Node<TNode>), rest) { }

        public Node<TNode> ResultNode { get; internal set; }
        public string Remaining { get; internal set; }
        public bool Success { get; internal set; }
        public string ErrorMessage { get; internal set; }

        public override string ToString()
        {
            if (Success) return $"PResult {{ Status: Succeeded, Remaining: \"{Remaining}\", AST:\n" + ResultNode.ToString(Util.Config.Indentation) + $"\n}}";
            else         return $"PResult {{ Status: Failed, Message: \"{ErrorMessage}\", Remaining: \"{Remaining}\" }}";
        }


        public static implicit operator PResult(PResult<TNode> n)
        {
            if (typeof(TNode).Equals(typeof(NodeType))) return PResult.From(n as PResult<NodeType>);
            throw new ArgumentException("Cannot cast Node<" + typeof(TNode).Name + "> to Node! Must be Node<NodeType>");
        }
    }
    public class PResult : PResult<NodeType>
    {
        public static PResult Empty(string rest) => Succeeded(Node.Empty, rest);
        public static PResult Unknown => Failed("Unknown", string.Empty);

        public PResult(bool success, Node node, string rest) : base(success, node, rest) { }

        public PResult(string errorMessage, string rest) : base(false, Node.Corrupted, rest)
        {
            ErrorMessage = errorMessage;
        }

        public PResult(bool success, string errorMessage, Node<NodeType> node, string rest) : base(success, errorMessage, node, rest) { }

        public static PResult From(PResult<NodeType> presult)
        {
            return new PResult(presult.Success, presult.ErrorMessage, presult.ResultNode, presult.Remaining);
        }
    }
}