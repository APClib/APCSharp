using System;
using APCSharp.Parser;
using APCSharp.Util;

namespace Demo
{
    class Program
    {

        static void Main(string[] args)
        {
            MatchDemo();
        }

        static void MatchDemo()
        {
            Parser equalSignParser = Parser.Char('=').InfoBinder("equal sign '='");
            Parser underscoreParser = Parser.Char('_').InfoBinder("underscore '_'");
            Parser identifierParser =  Parser.AnyOf(Parser.Word, underscoreParser).FollowedBy(Parser.AnyOf(Parser.Word, underscoreParser, Parser.Integer).ZeroOrMore().ListToString()).ListToString().InfoBinder("variable identifier");
            
            ParserBuilder expressionParser = null;
            expressionParser = Parser.AnyOf( 
                Parser.Char('(').FollowedBy(Parser.Ref(() => expressionParser)).FollowedBy(Parser.Char(')')).Flatten().InfoBinder("parameterized expression"),
                Parser.Integer
            );

            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("LI> "); 
                var expr = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(expr)) continue;
                PResult result = expressionParser.Run(expr);
                if (result.Success)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine(result.AST.ToString());
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine($"Remaining ({result.Remaining.Length}): '{result.Remaining}'");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(result.ErrorMessage);
                }
            }
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
                .FollowedBy(Parser.WhiteSpaces.Maybe()).OneOrMore()
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
            /*
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
*/
            JSONObject data = JSONObject.Parse(
@"{
    ""Name"" :  ""Alex"",
    ""Age"" :  ""20""
}");
            Console.WriteLine($"{data["Name"]} is {data["Age"]} years old and {(data["Admin"].AsBool().Value ? "is" : "is not")} admin.");
        }
    }
}
