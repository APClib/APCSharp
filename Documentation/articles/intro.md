# Intro

*We believe that the best way to learn something is to just start working with it.* 

In order to start building parsers we need to import the **APC#** library.

```csharp
using APCSharp.Parser;
```

Now write

```csharp
var HelloWorld = Parser.String("Hello,").AnyWhitespaces().FollowedBy(Parser.String("world!"));
var result = HelloWorld.Run("Hello, world!");
Console.WriteLine(result.AST.ToString());
```

The output should look something like this.

```json
Node { Type: List, Children:
    Node { Type: List, Children:
        Node { Type: String, Value: "Hello,"}
        Node { Type: WhiteSpace, Value: " "}
    }
    Node { Type: String, Value: "world!"}
}
```



Take a look at the examples below for more inspiration!


## Examples
This C# code below, creates a new parser, then using the `Run()` method to parse the input expression, and generate an AST.

```c#
Parser p = Parser.AnyOf(
        Parser.String("Programming"),
        Parser.String("Coding"),
        Parser.Word // Any string containing letters
    )
    .FollowedBy(
    	Parser.WhiteSpaces.Maybe() // Matches WhiteSpaces if there are any, else return an empty Node
	).RemoveEmptyMaybeMatches()	// Removes all empty Nodes
    .Many(); // Match every instance of: (<string/string/word> <whitespace?>)+

PResult r = p.Run(@"Coding
  is   cool!");
Console.WriteLine(r);
```
This produces the following output AST:

```text
PResult { Status: Succeeded, Remaining: "", AST:
    Node { Type: List, Children:
        Node { Type: List, Children:
            Node { Type: String, Value: "Coding" (System.String) }
            Node { Type: WhiteSpace, Value: "\r\n  " (System.String) }
        }
        Node { Type: List, Children:
            Node { Type: Word, Value: "is" (System.String) }
            Node { Type: WhiteSpace, Value: "   " (System.String) }
        }
        Node { Type: List, Children:
            Node { Type: Word, Value: "cool" (System.String) }
        }
    }
}
```