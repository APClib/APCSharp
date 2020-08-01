using System;
using APCSharp;

namespace Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser p = Parser.String("Ala");
            PResult r = p.RunSafe("Alla");
            Console.WriteLine(r);
        }
    }
}
