using System;
using System.Collections.Generic;
using System.Text;

namespace APCSharp.Parser.Data
{
    internal static class SharedData
    {
        internal static LineColumn LineColumn = new LineColumn(0,0);
        internal static IDictionary<object, Memory> Memos = new Dictionary<object, Memory>();
    }
}
