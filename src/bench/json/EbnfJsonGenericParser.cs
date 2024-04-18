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
        public JSon IntValue(Token<JsonTokenGeneric> intToken)
        {
            return new JValue(intToken.IntValue);
        }

        [Production("value : DOUBLE")]
        public JSon DoubleValue(Token<JsonTokenGeneric> doubleToken)
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
        public JSon BooleanValue(Token<JsonTokenGeneric> boolToken)
        {
            return new JValue(bool.Parse(boolToken.Value));
        }

        [Production("value : NULL[d]")]
        public JSon NullValue()
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

        [Production("object: ACCG[d] ACCD[d]")]
        public JSon EmptyObjectValue()
        {
            return new JObject();
        }

        [Production("object: ACCG[d] members ACCD[d]")]
        public JSon AttributesObjectValue(JObject members)
        {
            return members;
        }

        #endregion

        #region LIST

        [Production("list: CROG[d] CROD[d]")]
        public JSon EmptyList()
        {
            return new JList();
        }

        [Production("list: CROG[d] listElements CROD[d]")]
        public JSon List(JList elements)
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

        [Production("additionalValue: COMMA[d] value")]
        public JSon ListElementsOne(JSon value)
        {
            return value;
        }

        #endregion

        #region PROPERTIES

        [Production("members: property additionalProperty*")]
        public JSon Members(JObject head, List<JSon> tail)
        {
            var value = new JObject();
            value.Merge(head);
            foreach (var p in tail) value.Merge((JObject) p);
            return value;
        }

        [Production("additionalProperty : COMMA[d] property")]
        public JSon property(JObject property)
        {
            return property;
        }

        [Production("property: STRING COLON[d] value")]
        public JSon property(Token<JsonTokenGeneric> key, JSon value)
        {
            return new JObject(key.StringWithoutQuotes, value);
        }

        #endregion
    }
}