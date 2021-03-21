using System;
using APCSharp.Info;

namespace APCSharp.Parser
{
    /// <summary>
    /// Parser result
    /// </summary>
    public class PResult
    {
        /// <summary>
        /// Root of parsed AST
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public Node AST { get; internal set; }
        /// <summary>
        /// Indicates if the paring was successful
        /// </summary>
        public bool Success { get; internal set; }
        /// <summary>
        /// If the parse was not successful, this field holds the error message
        /// </summary>
        public string ErrorMessage { get; internal set; }
        /// <summary>
        /// If the parse was not successful, this field holds the input sequence that failed to match
        /// </summary>
        public string ErrorSequence { get; internal set; }
        /// <summary>
        /// Constructor for a successful parse result
        /// </summary>
        /// <param name="node">Root AST</param>
        /// <returns>Successful parse result</returns>
        public static PResult Succeeded(Node node) => new PResult
        {
            Success = true,
            AST = node
        };
        /// <summary>
        /// Constructor for a failed parse result
        /// </summary>
        /// <param name="errorMsg">A descriptive error message</param>
        /// <param name="errorSequence">The sequence that failed to be parsed</param>
        /// <returns>Failed parse result</returns>
        public static PResult Failed(string errorMsg, string errorSequence) => new PResult
        {
            Success = false,
            ErrorMessage = errorMsg,
            ErrorSequence = errorSequence,
            AST = Node.Corrupted
        };
        /// <summary>
        /// Constructor for a failed parse result
        /// </summary>
        /// <param name="errorMsg">A descriptive error message</param>
        /// <param name="errorChar">The character that failed to parse</param>
        /// <returns></returns>
        public static PResult Failed(string errorMsg, char errorChar) =>
            Failed(errorMsg, errorChar.ToString());
        /// <summary>
        /// An empty parse result.
        /// Often used within the mechanism of a parser and should not be returned to the user.
        /// </summary>
        /// <returns>Empty parse result</returns>
        public static PResult Empty() => Succeeded(Node.Empty);

        internal static PResult EndOfInput(params APCSharp.Parser.Parser[] parsers) =>
            Failed(Error.Unexpected("end of input", parsers), '\0');



        protected PResult(){}

        public override string ToString()
        {
            return Success ? $"PResult {{ Status: Succeeded, AST:\n" + AST.ToString(Util.Config.Indentation) + $"\n}}" :
                             $"PResult {{ Status: Failed, Message: \"{ErrorMessage}\" }}";
        }
    }

    /// <summary>
    /// Generic parser result
    /// </summary>
    /// <typeparam name="TNode">Enum for node type</typeparam>
    public class PResult<TNode> : PResult where TNode : struct, IConvertible
    {
        /// <summary>
        /// Root of parsed AST
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public new Node<TNode> AST { get; internal set; }
        public static PResult<TNode> Succeeded(Node<TNode> node) => new PResult<TNode>
        {
            Success = true,
            AST = node
        };
        public static PResult<TNode> Failed(string errorMsg) => new PResult<TNode>
        {
            Success = false,
            ErrorMessage = errorMsg,
            AST = default(Node<TNode>)
        };
        protected PResult(){}
    }
}