using System;
using APCSharp.Parser;

namespace Demo
{
    class Program
    {
        static void Main(string[] args)
        {

            Parser p =
                Parser.AnyOf(
                    Parser.String("Programming"),
                    Parser.String("Coding"),
                    Parser.Word
                 )
                .FollowedBy(Parser.WhiteSpaces.Maybe()).RemoveEmptyMaybeMatches().Many();

            PResult r = p.Run(@"Coding
  is   cool!");
            
            /*
            Parser p = Parser.Integer;

            PResult r = p.Run(@"12Kodning
is cool!");
            */

            if (r.Success) Console.WriteLine(r); 
            else Console.WriteLine(r.ErrorMessage);
        }
    }
}
