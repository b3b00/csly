using sly.lexer;
using System.Linq;
using sly.parser.generator;

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using jsonparser.JsonModel;


namespace jsonparser
{


    public class EbnfJsonParser
    {



        [LexerConfiguration]
        public ILexer<JsonToken> BuildJsonLexer(ILexer<JsonToken> lexer)
        {                  
            lexer.AddDefinition(new TokenDefinition<JsonToken>(JsonToken.DOUBLE, "[0-9]+\\.[0-9]+"));
            lexer.AddDefinition(new TokenDefinition<JsonToken>(JsonToken.INT, "[0-9]+"));            
            lexer.AddDefinition(new TokenDefinition<JsonToken>(JsonToken.STRING, "(\\\")([^(\\\")]*)(\\\")"));
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

        [Production("root : value")]
        public  JSon Root(JSon value)
        {
            return value;
        }


        #endregion

        #region VALUE

        [Production("value : STRING")]
        public  JSon StringValue(Token<JsonToken> stringToken)
        {
            return new JValue(stringToken.StringWithoutQuotes);
        }

        [Production("value : INT")]
        public  object IntValue(Token<JsonToken> intToken)
        {
            return new JValue(intToken.IntValue);
        }

        [Production("value : DOUBLE")]
        public object DoubleValue(Token<JsonToken> doubleToken)
        {
            double dbl = double.MinValue;
            try
            {
                string[] doubleParts = doubleToken.Value.Split('.');
                dbl = double.Parse(doubleParts[0]);
                if (doubleParts.Length > 1)
                {
                    double decimalPart = double.Parse(doubleParts[1]);
                    for (int i = 0; i < doubleParts[1].Length; i++)
                    {
                        decimalPart = decimalPart / 10.0;
                    }
                    dbl += decimalPart;
                }
            }
            catch (Exception e)
            {
                dbl = double.MinValue;
            }

            return new JValue(dbl);
        }

        [Production("value : BOOLEAN")]
        public  object BooleanValue(Token<JsonToken> boolToken)
        {
            return new JValue(bool.Parse(boolToken.Value));
        }

        [Production("value : NULL")]
        public  object NullValue(object forget)
        {
            return new JNull();
        }

        [Production("value : object")]
        public  JSon ObjectValue(JSon value)
        {
            return value;
        }

        [Production("value: list")]
        public  JSon ListValue(JList list)
        {
            return list;
        }

        #endregion

        #region OBJECT

        [Production("object: ACCG ACCD")]
        public  JSon EmptyObjectValue(object accg , object accd)
        {
            return new JObject();
        }

        [Production("object: ACCG members ACCD")]
        public  JSon AttributesObjectValue(object accg ,JObject members, object accd)
        {
            return members;
        }


        #endregion
        #region LIST

        [Production("list: CROG CROD")]
        public  JSon EmptyList(object crog , object crod)
        {
            return new JList();
        }

        [Production("list: CROG listElements CROD")]
        public  JSon List(object crog ,JList elements, object crod)
        {
            return elements;
        }


        [Production("listElements: value additionalValue+")]
        public JSon listElements(JSon head, List<JSon> tail)
        {
            JList values = new JList(head);
            values.AddRange(tail);
            return values;
        }

        [Production("additionalValue: COMMA value")]
        public JSon ListElementsOne(Token<JsonToken> discardedComma, JSon value)
        {
            return value;
        }

        

        #endregion

        #region PROPERTIES



        [Production("members: property additionalProperty+")]
        public object Members(JObject head, List<JSon> tail)
        {
            JObject value = new JObject();
            value.Merge(head);
            foreach(JSon p in tail)
            {
                value.Merge((JObject)p);
            }
            return value;
        }

        [Production("additionalProperty : COMMA property")]
        public object property(Token<JsonToken> comma, JObject property )
        {
            return property;
        }

        [Production("property: STRING COLON value")]
        public  object property(Token<JsonToken> key, object colon, JSon value)
        {
            return new JObject(key.StringWithoutQuotes ,value);
        }

       
        //[Production("members : property COMMA members")]
        //public  object ManyMembers(KeyValuePair<string, object> pair, object comma, Dictionary<string, object> tail)
        //{
        //    Dictionary<string, object> members = new Dictionary<string, object>();
        //    members[pair.Key] = pair.Value;
        //    members.Merge(tail);
        //    return members;
        //}

        //[Production("members : property")]
        //public  object SingleMember(KeyValuePair<string, object> pair)
        //{
        //    Dictionary<string, object> members = new Dictionary<string, object>();
        //    members.Add(pair.Key, pair.Value);
        //    return members;
        //}

        #endregion




    }
}
