using System;
namespace APCSharp.Parser
{
    public class PResult
    {
        public Node ResultNode { get; internal set; }
        public string Remaining { get; internal set; }
        public bool Success { get; internal set; }
        public string ErrorMessage { get; internal set; }
        public string ErrorSequence { get; internal set; }
        public static PResult Succeeded(Node node, string rest) => new PResult
        {
            Success = true,
            ResultNode = node,
            Remaining = rest
        };
        public static PResult Failed(string errorMsg, string errorSequence, string rest) => new PResult
        {
            Success = false,
            ErrorMessage = errorMsg,
            ErrorSequence = errorSequence,
            ResultNode = Node.Corrupted,
            Remaining = rest
        };
        public static PResult Empty(string rest) => Succeeded(Node.Empty, rest);
        public static PResult Unknown => Failed("Unknown", null, string.Empty);

        public PResult(){}

        public PResult(bool success, Node node, string rest)
        {
            Success = success;
            ResultNode = node;
            Remaining = rest;
        }

        public override string ToString()
        {
            if (Success) return $"PResult {{ Status: Succeeded, Remaining: \"{Remaining}\", AST:\n" + ResultNode.ToString(Util.Config.Indentation) + $"\n}}";
            else         return $"PResult {{ Status: Failed, Message: \"{ErrorMessage}\", Remaining: \"{Remaining}\" }}";
        }
    }
    public class PResult<TNode> : PResult where TNode : struct, IConvertible
    {
        public new Node<TNode> ResultNode { get; internal set; }
        public static PResult<TNode> Succeeded(Node<TNode> node, string rest) => new PResult<TNode>
        {
            Success = true,
            ResultNode = node,
            Remaining = rest
        };
        public static PResult<TNode> Failed(string errorMsg, string rest) => new PResult<TNode>
        {
            Success = false,
            ErrorMessage = errorMsg,
            ResultNode = default(Node<TNode>),
            Remaining = rest
        };
        public PResult(){}
    }
}