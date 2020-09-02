using System;
using System.Collections.Generic;
using System.Text;

namespace APCSharp.Parser
{
     /// <summary>
     /// Default NodeType, should be sufficient for most parsers.
     /// </summary>
    public enum NodeType
    {
        /// <summary>
        /// Value is damaged and invalid with no childnodes.
        /// </summary>
        Corrupted,
        /// <summary>
        /// Used by Maybe() method when generating non-matching result.
        /// </summary>
        Empty,

        /// <summary>
        /// Node Value is of type Char.
        /// </summary>
        Char,
        /// <summary>
        /// Node Value is null but have child-nodes.
        /// </summary>
        List,
        /// <summary>
        /// Node Value is of type String or Char.
        /// </summary>
        String,
        /// <summary>
        /// Node Value is of type String or Char with label Word.
        /// </summary>
        Word,
        /// <summary>
        /// Node Value is of type String or Char with label WhiteSpace.
        /// </summary>
        WhiteSpace,
        /// <summary>
        /// Node Value is of type char with label Digit.
        /// </summary>
        Digit,
        /// <summary>
        /// Node Value is of type String or Char with label Integer.
        /// </summary>
        Integer,
        /// <summary>
        /// Node Value is of type String or Char with label Number.
        /// </summary>
        Number,
        /// <summary>
        /// Node Value is of type Object.
        /// </summary>
        Object,
        /// <summary>
        /// Node Value is a Pair of Nodes.
        /// </summary>
        Pair
    }
}
