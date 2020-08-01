using System;
using System.Collections.Generic;
using System.Text;

namespace APCSharp
{
    public class ParserBuilder : Parser
    {
        public ParserBuilder(Func<string, PResult> func) : base(func) { }


        #region Dynamic Methods

        public ParserBuilder And(ParserBuilder parser, Func<Node, Node, Node> combiner)
        {
            return new ParserBuilder((string s) => {
                PResult p1 = Run(s); // Run this
                if (p1.Success)
                {
                    PResult p2 = parser.Run(p1.Remaining);
                    if (p2.Success)
                    {
                        return new PResult(true, combiner(p1.Node, p2.Node), p2.Remaining);
                    }
                }
                return PResult.Failed(s);
            });
        }

        #endregion
    }
}
