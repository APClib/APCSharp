using System;
namespace APCSharp.Parser
{
    public class PResult<TNode> where TNode : struct, IConvertible
    {
        public static PResult<TNode> Succeeded(Node<TNode> node, string rest) => new PResult<TNode>(true, node, rest);
        public static PResult<TNode> Failed(string errorMsg, string rest) => new PResult<TNode>(errorMsg, rest);
        public PResult(bool success, Node<TNode> node, string rest)
        {
            ResultNode = node;
            Remaining = rest;
            Success = success;
        }
        public PResult(string errorMessage, string rest)
        {
            ResultNode = default(Node<TNode>);
            Success = false;
            ErrorMessage = errorMessage;
            Remaining = rest;
        }

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
            if (n.GetType().Equals(typeof(PResult<NodeType>))) return n as PResult;
            throw new ArgumentException("Cannot cast Node<" + typeof(TNode).Name + "> to Node! Must be Node<NodeType>");
        }
    }
    public class PResult : PResult<NodeType>
    {
        public static PResult Empty(string rest) => Succeeded(Node.Empty, rest) as PResult;
        public static PResult Unknown => Failed("Unknown", string.Empty) as PResult;

        public PResult(string errorMessage, string rest) : base(false, Node.Corrupted, rest)
        {
            ErrorMessage = errorMessage;
        }

        public PResult(bool success, Node node, string rest) : base(success, node, rest)
        {
        }
    }
}