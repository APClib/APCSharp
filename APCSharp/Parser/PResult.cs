using System;
using System.IO;
using APCSharp.Info;

namespace APCSharp.Parser
{
    /// <summary>
    /// Abstract parse result.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public class PResultBase<TPResult, TNode, TNodeType, TNodeData>
        where TNode : ANode<TNode, TNodeType, TNodeData>
            where TNodeType : struct
            where TNodeData : struct
        where TPResult : PResultBase<TPResult, TNode, TNodeType, TNodeData>, new()
    {
        /// <summary>
        /// Root of parsed AST
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public TNode AST { get; internal set; }

        public StreamReader Stream { get; internal set; }
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
        public static TPResult Succeeded(TNode node, StreamReader stream) => new TPResult
        {
            Success = true,
            AST = node,
            Stream = stream
        };
        /// <summary>
        /// Constructor for a failed parse result
        /// </summary>
        /// <param name="errorMsg">A descriptive error message</param>
        /// <param name="errorSequence">The character sequence that failed to parse</param>
        /// <param name="stream">Stream reader</param>
        /// <returns></returns>
        public static TPResult Failed(string errorMsg, string errorSequence, StreamReader stream) => new TPResult
        {
            Success = false,
            ErrorMessage = errorMsg,
            AST = default(TNode),
            Stream = stream
        };
        /// <summary>
        /// Constructor for a failed parse result
        /// </summary>
        /// <param name="errorMsg">A descriptive error message</param>
        /// <param name="errorChar">The character that failed to parse</param>
        /// <param name="stream">Stream reader</param>
        /// <returns></returns>
        public static TPResult Failed(string errorMsg, char errorChar, StreamReader stream) => Failed(errorMsg, errorChar.ToString(), stream);
        public static TPResult EndOfInput(ErrorLogger<ParserBuilder, Combiner, PResult, Node, NodeType, NodeData> error, params IParserBuilder<ParserBuilder, Combiner, Node, NodeType, NodeData>[] parsers) => Failed(error.Unexpected("end of input", parsers), '\0', StreamReader.Null);
        public override string ToString()
        {
            return Success ? $"{GetType().Name} {{ Status: Succeeded, AST:\n" + AST.ToString(Util.Config.Indentation) + $"\n}}" :
                $"{GetType().Name} {{ Status: Failed, Message: \"{ErrorMessage}\" }}";
        }
    }

    /// <summary>
    /// Parse result.
    /// </summary>
    public class PResult : PResultBase<PResult, Node, NodeType, NodeData>
    {
        /// <summary>
        /// An empty parse result.
        /// Often used within the mechanism of a parser and should not be returned to the user.
        /// </summary>
        /// <returns>Empty parse result</returns>
        public static PResult Empty(StreamReader stream) => Succeeded(Node.Empty, StreamReader.Null);
    }

    /// <summary>
    /// Generic parser result.
    /// </summary>
    /// <typeparam name="TNode">Enum for node type</typeparam>
    public class PResult<TNode, TNodeType, TNodeData> : PResultBase<PResult<TNode, TNodeType, TNodeData>, TNode, TNodeType, TNodeData>
        where TNode : ANode<TNode, TNodeType, TNodeData>, new()
            where TNodeType : struct
            where TNodeData : struct
    { }
}