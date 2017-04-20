using lexer;
using System.Linq;
using parser.parsergenerator.generator;

using System;
using System.Collections.Generic;


namespace ParserExample
{
    public enum JsonToken
    {
        //IDENTIFIER,
        STRING,
        INT,
        DOUBLE,
        BOOLEAN,
        ACCG,
        ACCD,
        CROG,
        CROD,
        COMMA,
        COLON,
        SEMICOLON,
        WS,
        EOL
        
       
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

    class JSONParser
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

        [Reduction("root : object ")]        
        public static object Root(List<object> args)
        {
            return args[0];
        }

        [Reduction("object : ACCG properties ACCD ")]
        public static object Object(List<object> args)
        {
            
            return args[1];
        }

        [Reduction("properties : property COMMA properties")]
        [Reduction("properties : ")]
        public static object Properties(List<object> args)
        {
            Dictionary<string, object> r = new Dictionary<string, object>();
            if (args.Count == 2 || args.Count == 3 || args.Count == 4) 
            {
                KeyValuePair<string, object> pair = (KeyValuePair<string, object>)args[0];
                Dictionary<string, object> dico = (Dictionary<string, object>)args[args.Count-1];                
                r[pair.Key] = pair.Value;

                if (args.Count == 4)
                {
                    Dictionary<string, object> dico2 = (Dictionary<string, object>)args[args.Count - 2];
                    r.Merge(dico2);
                }
            }           
            else 
            {
                r = new Dictionary<string, object>();
            }
            return r;
        }

        [Reduction("property : STRING COLON value")]
        public static object Property(List<object> args)
        {
            return new KeyValuePair<string, object>((args[0] as Token<JsonToken>).Value, args[2]);
        }

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

        [Reduction("value : object")]
        public static object ObjectProperty(List<object> args)
        {
            return args[0];
        }


        [Reduction("list : CROG values CROD")]
        public static object List(List<object> args)
        {
            return (List<object>)args[1];
        }

        [Reduction("values : value COMMA list")]
        [Reduction("values :")]
        public static object Values(List<object> args)
        {
            List<object> r = new List<object>();
            if (args.Count == 3)
            {
                r.Add((args[0] as Token<JsonToken>).Value);
                List<string> tail = args[2] as List<string>;
                r.AddRange(tail);
                return r;
                ;
            }
            else
            {

            }
            return r;
        }

        

    }
}
