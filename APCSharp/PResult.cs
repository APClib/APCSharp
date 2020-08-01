namespace APCSharp
{
    public class PResult
    {
        public static PResult Failed(string s) => new PResult(false, Node.Corrupted, s);
        public PResult(bool success, Node node, string rest)
        {
            Node = node;
            Remaining = rest;
            Success = success;
        }

        public Node Node { get; }
        public string Remaining { get; }
        public bool Success { get; }

        public override string ToString()
        {
            if (Success) return $"PResult {{ Status: Succeeded, Remaining: \"{Remaining}\", AST:\n" + Node + $"\n}}";
            else         return $"PResult {{ Status: Failed, Input: \"{Remaining}\"}}";
        }
    }
}