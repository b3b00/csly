using sly.lexer;
using System.Linq;
using sly.parser.generator;

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;


namespace jsonparser
{
    public enum JsonToken
    {        
        STRING = 1,
        INT = 2,
        DOUBLE = 3,
        BOOLEAN = 4,
        ACCG = 5,
        ACCD = 6,
        CROG = 7,
        CROD = 8,
        COMMA = 9,
        COLON = 10,
        SEMICOLON = 11,
        WS = 12,
        EOL = 13,
        NULL = 14,
        QUOTE = 99


    }


    public static class DictionaryExtensionMethods
    {
        public static void Merge<TKey, TValue>(this Dictionary<TKey, TValue> me, Dictionary<TKey, TValue> merge)
        {
            foreach (var item in merge)
            {
                me[item.Key] = item.Value;
            }
        }
    }

    public class JSONParser
    {



        [LexerConfigurationAttribute]
        public  Lexer<JsonToken> BuildJsonLexer(Lexer<JsonToken> lexer = null)
        {
            if (lexer == null) {
                lexer = new Lexer<JsonToken>();
            }            
            lexer.AddDefinition(new TokenDefinition<JsonToken>(JsonToken.DOUBLE, "[0-9]+\\.[0-9]+"));
            lexer.AddDefinition(new TokenDefinition<JsonToken>(JsonToken.INT, "[0-9]+"));            
            lexer.AddDefinition(new TokenDefinition<JsonToken>(JsonToken.STRING, "(\\\"|')([^(\\\"|')]*)(\\\"|')"));
            lexer.AddDefinition(new TokenDefinition<JsonToken>(JsonToken.BOOLEAN, "(true|false)"));
            lexer.AddDefinition(new TokenDefinition<JsonToken>(JsonToken.NULL, "(null)"));
            lexer.AddDefinition(new TokenDefinition<JsonToken>(JsonToken.ACCG, "{"));
            lexer.AddDefinition(new TokenDefinition<JsonToken>(JsonToken.ACCD, "}"));
            lexer.AddDefinition(new TokenDefinition<JsonToken>(JsonToken.CROG, "\\["));
            lexer.AddDefinition(new TokenDefinition<JsonToken>(JsonToken.CROD, "\\]"));
            lexer.AddDefinition(new TokenDefinition<JsonToken>(JsonToken.COLON, ":"));
            lexer.AddDefinition(new TokenDefinition<JsonToken>(JsonToken.COMMA, ","));
            lexer.AddDefinition(new TokenDefinition<JsonToken>(JsonToken.WS, "[ \\t]+", true));
            lexer.AddDefinition(new TokenDefinition<JsonToken>(JsonToken.EOL, "[\\n\\r]+", true, true));
            return lexer;
        }


        #region root

        [Reduction("root : value")]
        public  object Root(object value)
        {
            return value;
        }


        #endregion

        #region VALUE

        [Reduction("value : STRING")]
        public  object StringValue(Token<JsonToken> stringToken)
        {
            return stringToken.StringWithoutQuotes;
        }

        [Reduction("value : INT")]
        public  object IntValue(Token<JsonToken> intToken)
        {
            return intToken.IntValue;
        }

        [Reduction("value : DOUBLE")]
        public  object DoubleValue(Token<JsonToken> doubleToken)
        {
            return doubleToken.DoubleValue;
        }

        [Reduction("value : BOOLEAN")]
        public  object BooleanValue(Token<JsonToken> boolToken)
        {
            return bool.Parse(boolToken.Value);
        }

        [Reduction("value : NULL")]
        public  object NullValue(object forget)
        {
            return null;
        }

        [Reduction("value : object")]
        public  object ObjectValue(object value)
        {
            return value;
        }

        [Reduction("value: list")]
        public  object ListValue(List<object> list)
        {
            return list;
        }

        #endregion

        #region OBJECT

        [Reduction("object: ACCG ACCD")]
        public  object EmptyObjectValue(object accg , object accd)
        {
            return new Dictionary<string, object>();
        }

        [Reduction("object: ACCG members ACCD")]
        public  object AttributesObjectValue(object accg ,Dictionary<string,object> members, object accd)
        {
            return members;
        }


        #endregion
        #region LIST

        [Reduction("list: CROG CROD")]
        public  object EmptyList(object crog , object crod)
        {
            return new List<object>();
        }

        [Reduction("list: CROG listElements CROD")]
        public  object List(object crog ,List<object> elements, object crod)
        {
            return elements;
        }


        [Reduction("listElements: value COMMA listElements")]
        public  object ListElementsMany(object value, object comma, List<object> tail)
        {
            List<object> elements = new List<object>() { value};
            elements.AddRange(tail);
            return elements;
        }

        [Reduction("listElements: value")]
        public  object ListElementsOne(object element)
        {
            return new List<object>() { element };
        }


        #endregion

        #region PROPERTIES

        [Reduction("property: STRING COLON value")]
        public  object property(Token<JsonToken> key, object colon, object value)
        {
            return new KeyValuePair<string, object>(key.StringWithoutQuotes, value);
        }

       
        [Reduction("members : property COMMA members")]
        public  object ManyMembers(KeyValuePair<string, object> pair, object comma, Dictionary<string, object> tail)
        {
            Dictionary<string, object> members = new Dictionary<string, object>();
            members[pair.Key] = pair.Value;
            foreach (string k in tail.Keys)
            {
                members.Add(k, tail[k]);
            }
            return members;
        }

        [Reduction("members : property")]
        public  object SingleMember(KeyValuePair<string, object> pair)
        {
            Dictionary<string, object> members = new Dictionary<string, object>();
            members.Add(pair.Key, pair.Value);
            return members;
        }

        #endregion




    }
}
