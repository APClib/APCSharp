using System;
using APCSharp.Parser;

namespace Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            JSONDemo();
        }

        static void CodingDemo()
        {
            Parser p =
                Parser.AnyOf(
                    Parser.String("Programming"),
                    Parser.String("Coding"),
                    Parser.Word,
                    Parser.AnyOf(
                        Parser.Char('!'),
                        Parser.Char('?'),
                        Parser.Char('.')
                    )
                 )
                .FollowedBy(Parser.WhiteSpaces.Maybe()).RemoveEmptyMaybeMatches().Many()
                .FollowedBy(Parser.AnyOf(
                    Parser.Char('!'),
                    Parser.Char('?'),
                    Parser.Char('.')
                ));

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

        static void JSONDemo()
        {
            JSONObject data = JSONObject.Parse(@"{
    ""Name"": ""Alex"",
    ""Age"": 37,
    ""Admin"": true,
    ""Contact"": {
        ""Site"": ""alexweb.com"",
        ""Phone"": 123456789,
        ""Address"": null
    },
    ""Tags"": [
        ""php"",
        ""web"",
        ""dev""
    ]
}");
            Console.WriteLine($"{data["Name"]} is {data["Age"]} years old and {(data["Admin"].AsBool().Value ? "is" : "is not")} admin.");
        }
    }
}
