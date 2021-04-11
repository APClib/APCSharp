using System;
using System.Collections.Generic;
using System.Text;

namespace APCSharp.Parser.Data
{
    class Memory
    {
        public PResult Value { get; private set; }
        public int Accesses { get; private set; }
        public bool IsLeftRecursive { get; private set; }

        private Memory() { }
        public static Memory Empty() => new Memory
        {
            Accesses = 0
        };
        public Memory Result(PResult result) => new Memory
        {
            Value = result,
            Accesses = 0,
            IsLeftRecursive = false
        };

        public Memory SetLeftRecursive() => new Memory
        {
            Accesses = Accesses + 1,
            IsLeftRecursive = true
        };
    }
}
