﻿using APCSharp.Parser;
using System;
using System.Collections.Generic;
using System.Text;

namespace Demo
{
    public abstract class JsonBase<T>
    {
        protected JsonBase(T value)
        {
            Value = value;
        }
        internal JsonBase() : this(default) { }

        public T Value { get; }

        public JsonString AsString() => new JsonString(Value.ToString());
        public JsonNumber AsNumber() => (JsonNumber)Convert.ChangeType(Value, typeof(JsonNumber));
        public JsonBool AsBool() => (JsonBool)Convert.ChangeType(Value, typeof(JsonBool));
        public JsonArray AsArray() => (JsonArray)Convert.ChangeType(Value, typeof(JsonArray));
        public JSONObject AsObject() => (JSONObject)Convert.ChangeType(Value, typeof(JSONObject));

    }
    public class JsonBase : JsonBase<dynamic>
    {
        public JsonBase(dynamic value) {
            Value = value;
        }

        public new dynamic Value { get; }
    }
    public class JsonNull : JsonBase<string>
    {
        public JsonNull(string value) : base(value) { }

        internal static ParserBuilder NullParser = Parser.String("null").Or(Parser.String("undefined"));
    }
    public class JsonString : JsonBase<string>
    {
        public JsonString(string value) : base(value) { }

        internal static ParserBuilder StringParser =
            Parser.Char('"')
                .FollowedBy(
                    Parser.CharsBut(new[] {'\\'}, '"')
                        .OneOrMore()
                        .ListToString()
                )
                .FollowedBy(
                    Parser.Char('"')
                )
                .MapChildren((n1, n2) => n1.Children[1], NodeType.String); // Extract string value
    }
    public class JsonNumber : JsonBase<decimal>
    {
        public JsonNumber(decimal value) : base(value) { }

        internal static ParserBuilder NumberParser = Parser.Number;
    }
    public class JsonBool : JsonBase<bool>
    {
        public JsonBool(bool value) : base(value) { }

        internal static ParserBuilder BoolParser = Parser.String("true").Or(Parser.String("false"));
    }
    public class JsonArray : JsonBase<JsonBase[]>
    {
        public JsonArray(JsonBase[] elements) : base(elements) { }

        internal static ParserBuilder ArrayParser =
            Parser.Char('[')
            .IgnoreAnyWhitespaces()
            .FollowedBy(
                        Parser.AnyOf(
                            JsonNull.NullParser,
                            JsonBool.BoolParser,
                            JsonNumber.NumberParser,
                            JsonString.StringParser
                            // JSONObject.objectParser
                            // JSONArray.arrayParser
                        )
                    )
                    .IgnoreAnyWhitespaces()
                    .FollowedBy(Parser.Char(','))
                    .IgnoreAnyWhitespaces()
                    .OneOrMore()
                    .FollowedBy(Parser.Char(']'))
            .Or(Parser.Char(']'));
    }
    public class JSONObject : JsonBase<Dictionary<string, JsonBase>>
    {
        public JSONObject(Dictionary<string, JsonBase> keyValuePairs) : base(keyValuePairs) { }

        public JSONObject() : this(new Dictionary<string, JsonBase>()) { }

        public JsonBase this[string key] {
            get => Value[key];
            set => Value[key] = value;
        }

        internal static ParserBuilder LazyObjectParser = Parser.Lazy(JSONObject.ObjectParser);
        internal static ParserBuilder KeyValueParser =
            Parser.AnyOf(
                JsonString.StringParser,
                Parser.Word,
                Parser.Integer
            )
            .IgnoreAnyWhitespaces()
            .FollowedBy(Parser.Char(':'))
            .IgnoreAnyWhitespaces()
            .Map(Combiner.First, NodeType.String)
            .FollowedBy(
                Parser.AnyOf(
                    /*JSONNull.nullParser,
                    JSONBool.boolParser,
                    JSONNumber.numberParser,*/
                    JsonString.StringParser /*,
                    JSONObject.lazyObjectParser,
                    JSONArray.arrayParser*/
                )
            ).MapChildren((n1, n2) => Node.List(n1, n2), NodeType.Pair).InfoBinder("Key-Value", "Key value pair");

        internal static ParserBuilder ObjectParser =
            Parser.Char('{')
                .IgnoreAnyWhitespaces()
                // Key: Value, pairs
                .FollowedBy(
                    KeyValueParser
                    .FollowedBy(Parser.Char(','))
                    .IgnoreAnyWhitespaces()
                )
                .ZeroOrMore().Maybe()
                .FollowedBy(KeyValueParser)
                .FollowedBy(Parser.Char('}'))
                .Map(Combiner.First, NodeType.Object);
        
        public static JSONObject Parse(string json)
        {
            JSONObject result = new JSONObject();

            PResult pr = ObjectParser.Run(json);
            if (pr.Success) Console.WriteLine(pr);
            else Console.WriteLine(pr.ErrorMessage);

            if (!pr.Success) throw new FormatException(pr.ErrorMessage + "\n\nRemaining:\n" + pr.Remaining);
            for (int i = 0; i < pr.AST.Children.Count; i++)
            {
                Node n = (Node)pr.AST.Children[i];
                result[n.Value] = n.Children;
            }

            return result;
        }
    }
}
