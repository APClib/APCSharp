using System;
using System.Collections.Generic;
using System.Text;

namespace APCSharp
{
    internal class ParserException : Exception
    {
        public ParserException(string message, PResult result) : base(message) {
            Result = result;
        }
        public ParserException(string message, PResult result, Exception innerException) : base(message, innerException) {
            Result = result;
        }

        public PResult Result { get; }
    }
}
