using lexer;
using System.Linq;
using parser.parsergenerator.generator;

using System;
using System.Collections.Generic;


namespace jsonparser
{
    public enum JsonToken
    {
        //IDENTIFIER,
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
        NULL = 14
        
       
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
        public static Lexer<JsonToken> BuildJsonLexer(Lexer<JsonToken> lexer = null)
        {
            if (lexer == null) { 
                lexer = new Lexer<JsonToken>();
            }
            lexer.AddDefinition(new TokenDefinition<JsonToken>(JsonToken.DOUBLE, "[0-9]+\\.[0-9]+"));
            lexer.AddDefinition(new TokenDefinition<JsonToken>(JsonToken.INT, "[0-9]+"));
            //lexer.AddDefinition(new TokenDefinition<JsonToken>(JsonToken.IDENTIFIER, "[A-Za-z0-9_àâéèêëîô][A-Za-z0-9\u0080-\u00FF_àâéèêëîô°]*"));
            lexer.AddDefinition(new TokenDefinition<JsonToken>(JsonToken.STRING, "\\\"([^\\\"]*)\\\""));
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
        public static object Root(List<object> args)
        {
            return args[0];
        }


        #endregion

        #region VALUE

        [Reduction("value : STRING")]
        public static object StringValue(List<object> args)
        {
            return (args[0] as Token<JsonToken>).Value;
        }

        [Reduction("value : INT")]
        public static object IntValue(List<object> args)
        {
            return (args[0] as Token<JsonToken>).IntValue;
        }

        [Reduction("value : DOUBLE")]
        public static object DoubleValue(List<object> args)
        {
            return (args[0] as Token<JsonToken>).DoubleValue;
        }

        [Reduction("value : BOOLEAN")]
        public static object BooleanValue(List<object> args)
        {
            return bool.Parse((args[0] as Token<JsonToken>).Value);
        }

        [Reduction("value : NULL")]
        public static object NullValue(List<object> args)
        {
            return null;
        }

        [Reduction("value : object")]
        public static object ObjectValue(List<object> args)
        {
            return args[0];
        }

        [Reduction("value: list")]
        public static object ListValue(List<object> args)
        {
            return args[0];
        }

        #endregion

        #region OBJECT

        [Reduction("object: ACCG ACCD")]
        public static object EmptyObjectValue(List<object> args)
        {
            return new Dictionary<string,object>();
        }


        #endregion
        #region LIST

        [Reduction("list: CROG CROD")]
        public static object EmptyList(List<object> args)
        {
            return new List<object>();
        }

        [Reduction("list: CROG listElements CROD")]
        public static object List(List<object> args)
        {
            return args[1];            
        }


        [Reduction("listElements: value COMMA listElements")]
        public static object ListElementsMany(List<object> args)
        {
            List<object> elements = new List<object>() { args[0] };
            elements.AddRange(args[2] as List<object>);
            return elements;
        }

        [Reduction("listElements: value")]
        public static object ListElementsOne(List<object> args)
        {
            return new List<object>() { args[0] };
        }

        


        #endregion



    }
}
