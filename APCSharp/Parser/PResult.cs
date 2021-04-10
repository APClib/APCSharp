using System;
using System.IO;
using APCSharp.Info;
using APCSharp.Parser.Data;

namespace APCSharp.Parser
{
    /// <summary>
    /// Abstract parse result.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public abstract class PResultBase<TParserBuilder, TCombiner, TPResult, TNode, TNodeType, TNodeData>
        where TParserBuilder : ParserBuilderBase<TParserBuilder, TCombiner, TPResult, TNode, TNodeType, TNodeData>
        where TCombiner : ACombiner<TNode, TNodeType, TNodeData>
        where TNode : ANode<TNode, TNodeType, TNodeData>, new()
            where TNodeType : struct
            where TNodeData : struct
        where TPResult : PResultBase<TParserBuilder, TCombiner, TPResult, TNode, TNodeType, TNodeData>, new()
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
        
        private LineColumn StoppedAt { get; set; } = SharedData.LineColumn;
        public string Remaining
        {
            get
            {
                var tmp = new MemoryStream(new byte[Stream.BaseStream.Length - StoppedAt.TotalChars]); // Create buffer for all chars between the successfully parsed ones and the remaining in the stream
                long p = Stream.BaseStream.Position; // Do this in case we for some reason would like to keep reading from the base stream afterwards
                Stream.BaseStream.Position = StoppedAt.TotalChars; // Start copying data from the index of the last successful char
                Stream.BaseStream.CopyTo(tmp);
                Stream.BaseStream.Position = p;
                tmp.Position = 0; // The position is updated when copied, so reset to be able to read from the stream
                return new StreamReader(tmp).ReadToEnd();
            }
        }

        /// <summary>
        /// Constructor for a successful parse result
        /// </summary>
        /// <param name="node">Root AST</param>
        /// <returns>Successful parse result</returns>
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
            ErrorSequence = errorSequence,
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
        public static TPResult EndOfInput(ErrorLogger<TParserBuilder, TCombiner, TPResult, TNode, TNodeType, TNodeData> error, params TParserBuilder[] parsers) => Failed(error.Unexpected("end of input", parsers), '\0', StreamReader.Null);
        public override string ToString()
        {
            return Success ? $"{GetType().Name} {{ Status: Succeeded, AST:\n" + AST.ToString(Util.Config.Indentation) + $"\n}}" :
                $"{GetType().Name} {{ Status: Failed, Message: \"{ErrorMessage}\" }}";
        }
    }

    /// <summary>
    /// Parse result.
    /// </summary>
    public class PResult : PResultBase<ParserBuilder, Combiner, PResult, Node, NodeType, NodeData>
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
    /// <typeparam name="TNodeType"></typeparam>
    /// <typeparam name="TNodeData"></typeparam>
    public class PResult<TNode, TNodeType, TNodeData> : PResultBase<ParserBuilder<TNode, TNodeType, TNodeData>, Combiner<TNode, TNodeType, TNodeData>, PResult<TNode, TNodeType, TNodeData>, TNode, TNodeType, TNodeData>
        where TNode : ANode<TNode, TNodeType, TNodeData>, new()
            where TNodeType : struct
            where TNodeData : struct
    { }
}