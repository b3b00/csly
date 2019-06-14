using System;
using System.Collections.Generic;
using bench.json.model;
using sly.lexer;
using sly.parser.generator;

namespace bench.json
{
    public class EbnfJsonGenericParser
    {
        #region root

        [Production("root : value")]
        public JSon Root(JSon value)
        {
            return value;
        }

        #endregion

        #region VALUE

        [Production("value : STRING")]
        public JSon StringValue(Token<JsonTokenGeneric> stringToken)
        {
            return new JValue(stringToken.StringWithoutQuotes);
        }

        [Production("value : INT")]
        public object IntValue(Token<JsonTokenGeneric> intToken)
        {
            return new JValue(intToken.IntValue);
        }

        [Production("value : DOUBLE")]
        public object DoubleValue(Token<JsonTokenGeneric> doubleToken)
        {
            double dbl;
            try
            {
                var doubleParts = doubleToken.Value.Split('.');
                dbl = double.Parse(doubleParts[0]);
                if (doubleParts.Length > 1)
                {
                    var decimalPart = double.Parse(doubleParts[1]);
                    for (var i = 0; i < doubleParts[1].Length; i++) decimalPart = decimalPart / 10.0;
                    dbl += decimalPart;
                }
            }
            catch (Exception)
            {
                dbl = double.MinValue;
            }

            return new JValue(dbl);
        }

        [Production("value : BOOLEAN")]
        public object BooleanValue(Token<JsonTokenGeneric> boolToken)
        {
            return new JValue(bool.Parse(boolToken.Value));
        }

        [Production("value : NULL")]
        public object NullValue(object forget)
        {
            return new JNull();
        }

        [Production("value : object")]
        public JSon ObjectValue(JSon value)
        {
            return value;
        }

        [Production("value: list")]
        public JSon ListValue(JList list)
        {
            return list;
        }

        #endregion

        #region OBJECT

        [Production("object: ACCG ACCD")]
        public JSon EmptyObjectValue(object accg, object accd)
        {
            return new JObject();
        }

        [Production("object: ACCG members ACCD")]
        public JSon AttributesObjectValue(object accg, JObject members, object accd)
        {
            return members;
        }

        #endregion

        #region LIST

        [Production("list: CROG CROD")]
        public JSon EmptyList(object crog, object crod)
        {
            return new JList();
        }

        [Production("list: CROG listElements CROD")]
        public JSon List(object crog, JList elements, object crod)
        {
            return elements;
        }


        [Production("listElements: value additionalValue*")]
        public JSon listElements(JSon head, List<JSon> tail)
        {
            var values = new JList(head);
            values.AddRange(tail);
            return values;
        }

        [Production("additionalValue: COMMA value")]
        public JSon ListElementsOne(Token<JsonTokenGeneric> discardedComma, JSon value)
        {
            return value;
        }

        #endregion

        #region PROPERTIES

        [Production("members: property additionalProperty*")]
        public object Members(JObject head, List<JSon> tail)
        {
            var value = new JObject();
            value.Merge(head);
            foreach (var p in tail) value.Merge((JObject) p);
            return value;
        }

        [Production("additionalProperty : COMMA property")]
        public object property(Token<JsonTokenGeneric> comma, JObject property)
        {
            return property;
        }

        [Production("property: STRING COLON value")]
        public object property(Token<JsonTokenGeneric> key, object colon, JSon value)
        {
            return new JObject(key.StringWithoutQuotes, value);
        }

        #endregion
    }
}