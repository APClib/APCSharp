using APCSharp.Parser;
using System;
using System.Collections.Generic;
using System.Text;

namespace Demo
{
    public abstract class JSONBase<T>
    {
        public JSONBase(T value)
        {
            Value = value;
        }
        internal JSONBase() : this(default) { }

        public T Value { get; }

        public JSONString AsString() => new JSONString(Value.ToString());
        public JSONNumber AsNumber() => (JSONNumber)Convert.ChangeType(Value, typeof(JSONNumber));
        public JSONBool AsBool() => (JSONBool)Convert.ChangeType(Value, typeof(JSONBool));
        public JSONArray AsArray() => (JSONArray)Convert.ChangeType(Value, typeof(JSONArray));
        public JSONObject AsObject() => (JSONObject)Convert.ChangeType(Value, typeof(JSONObject));

    }
    public class JSONBase : JSONBase<dynamic>
    {
        public JSONBase(dynamic value) {
            Value = value;
        }

        public new dynamic Value { get; }
    }
    public class JSONNull : JSONBase<string>
    {
        public JSONNull(string value) : base(value) { }

        internal static ParserBuilder nullParser
        {
            get
            {
                ParserBuilder parser = Parser.String("null").Or(Parser.String("undefined"));
                return parser;
            }
        }
    }
    public class JSONString : JSONBase<string>
    {
        public JSONString(string value) : base(value) { }

        internal static ParserBuilder stringParser
        {
            get
            {
                ParserBuilder parser =
                    Parser.Char('"')
                    .FollowedBy(
                        Parser.CharsBut(new[] { '\\' },'"')
                        .Many()
                        .FollowedBy(
                            Parser.Char('"')
                        )
                    )
                    .Or(
                        Parser.Char('"')
                    );
                return parser;
            }
        }
    }
    public class JSONNumber : JSONBase<decimal>
    {
        public JSONNumber(decimal value) : base(value) { }

        internal static ParserBuilder numberParser
        {
            get
            {
                ParserBuilder parser = Parser.Number;
                return parser;
            }
        }
    }
    public class JSONBool : JSONBase<bool>
    {
        public JSONBool(bool value) : base(value) { }

        internal static ParserBuilder boolParser
        {
            get
            {
                ParserBuilder parser = Parser.String("true").Or(Parser.String("false"));
                return parser;
            }
        }
    }
    public class JSONArray : JSONBase<JSONBase[]>
    {
        public JSONArray(JSONBase[] elements) : base(elements) { }

        internal static ParserBuilder arrayParser
        {
            get
            {
                ParserBuilder parser;
                parser = Parser.Char('[')
                    .FollowedBy(
                        Parser.WhiteSpaces.Maybe()
                        .FollowedBy(
                            Parser.AnyOf(
                                JSONNull.nullParser,
                                JSONBool.boolParser,
                                JSONNumber.numberParser,
                                JSONString.stringParser
                                // JSONObject.objectParser
                                // JSONArray.arrayParser
                            )
                        )
                        .FollowedBy(Parser.WhiteSpaces).Maybe()
                        .FollowedBy(Parser.Char(','))
                        .FollowedBy(Parser.WhiteSpaces).Maybe()
                        .Many()
                    .FollowedBy(Parser.Char(']'))
                    )
                    .Or(Parser.Char(']'));
                return parser;
            }
        }
    }
    public class JSONObject : JSONBase<Dictionary<string, JSONBase>>
    {
        public JSONObject(Dictionary<string, JSONBase> keyValuePairs) : base(keyValuePairs) { }

        public JSONObject() : this(new Dictionary<string, JSONBase>()) { }

        public JSONBase this[string key] {
            get
            {
                return Value[key];
            }
            set
            {
                Value[key] = value;
            }
        }

        internal static ParserBuilder objectParser { get
            {
                ParserBuilder parser;
                parser = Parser.Char('{').FollowedBy(
                    Parser.WhiteSpaces).Maybe()
                    .FollowedBy(
                        // Key: Value, pairs
                        Parser.AnyOf(
                            JSONString.stringParser,
                            Parser.Word,
                            Parser.Integer
                        )
                        .FollowedBy(Parser.WhiteSpaces).Maybe()
                        .FollowedBy(Parser.Char(':'))
                        .FollowedBy(Parser.WhiteSpaces).Maybe()
                        .FollowedBy(
                            Parser.AnyOf(
                                JSONNull.nullParser,
                                JSONBool.boolParser,
                                JSONNumber.numberParser,
                                JSONString.stringParser,
                                // JSONObject.objectParser,
                                JSONArray.arrayParser
                            )
                        )

                        // Comma separation
                        .FollowedBy(Parser.WhiteSpaces).Maybe()
                        .FollowedBy(Parser.Char(','))
                        .FollowedBy(Parser.WhiteSpaces).Maybe()
                    ).Maybe().FollowedBy(Parser.Char('}'));
                return parser;
            } }
        public static JSONObject Parse(string json)
        {
            JSONObject result = new JSONObject();

            PResult pr = objectParser.Run(json);
            if (!pr.Success) throw new FormatException(pr.ErrorMessage + "\n\nRemaining:\n" + pr.Remaining);
            for (int i = 0; i < pr.ResultNode.Children.Count; i++)
            {
                Node n = pr.ResultNode.Children[i];
                result[n.Value] = n.Children;
            }

            return result;
        }
    }
}
