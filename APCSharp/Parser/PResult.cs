namespace APCSharp.Parser
{
    public class PResult
    {
        public static PResult Empty(string rest) => Succeeded(Node.Empty, rest);
        public static PResult Unknown => Failed("Unknown", string.Empty);
        public static PResult Failed(string errorMsg, string rest) => new PResult(errorMsg, rest);
        public static PResult Succeeded(Node node, string rest) => new PResult(true, node, rest);
        public PResult(bool success, Node node, string rest)
        {
            Node = node;
            Remaining = rest;
            Success = success;
        }
        public PResult(string errorMessage, string rest) : this(false, Node.Corrupted, rest)
        {
            ErrorMessage = errorMessage;
        }

        public Node Node { get; }
        public string Remaining { get; }
        public bool Success { get; }
        public string ErrorMessage { get; }

        public override string ToString()
        {
            if (Success) return $"PResult {{ Status: Succeeded, Remaining: \"{Remaining}\", AST:\n" + Node.ToString(Util.Config.Indentation) + $"\n}}";
            else         return $"PResult {{ Status: Failed, Message: \"{ErrorMessage}\", Remaining: \"{Remaining}\" }}";
        }
    }
}