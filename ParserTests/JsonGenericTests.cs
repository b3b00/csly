using Xunit;
using sly.parser;
using sly.parser.generator;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using jsonparser;
using jsonparser.JsonModel;

namespace ParserTests
{
    
    public class JsonGenericTests
    {

        private static Parser<JsonTokenGeneric,JSon> Parser;
        
      
        public JsonGenericTests()
        {
            EbnfJsonGenericParser jsonParser = new EbnfJsonGenericParser();
            ParserBuilder<JsonTokenGeneric, JSon> builder = new ParserBuilder<JsonTokenGeneric, JSon>();
            Parser = builder.BuildParser(jsonParser, ParserType.EBNF_LL_RECURSIVE_DESCENT, "root").Result;
        }

        


#region VALUES
        [Fact]
        public void TestIntValue()
        {
            ParseResult<JsonTokenGeneric, JSon> r = Parser.Parse("1");
            Assert.False(r.IsError);
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.True(r.Result.IsValue);
            Assert.True(((JValue)r.Result).IsInt);
            Assert.Equal(1, ((JValue)r.Result).GetValue<int>());
        }

        [Fact]
        public void TestDoubleValue()
        {
            ParseResult<JsonTokenGeneric, JSon> r = Parser.Parse("0.1");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.True(r.Result.IsValue);
            Assert.True(((JValue)r.Result).IsDouble);
            Assert.Equal(0.1d, ((JValue)r.Result).GetValue<double>());
        }

        [Fact]
        public void TestStringValue()
        {
            string val = "hello";
            ParseResult<JsonTokenGeneric, JSon> r = Parser.Parse("\"" + val + "\"");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.True(r.Result.IsValue);
            Assert.True(((JValue)r.Result).IsString);
            Assert.Equal(val, ((JValue)r.Result).GetValue<string>());
        }

        [Fact]
        public void TestTrueBooleanValue()
        {
            ParseResult<JsonTokenGeneric, JSon> r = Parser.Parse("true");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.True(r.Result.IsValue);
            JValue val = ((JValue)r.Result);
            Assert.True(val.IsBool);
            Assert.True(val.IsBool);
            Assert.Equal(true, val.GetValue<bool>());
        }

        [Fact]
        public void TestFalseBooleanValue()
        {
            ParseResult<JsonTokenGeneric, JSon> r = Parser.Parse("false");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.True(r.Result.IsValue);
            JValue val = ((JValue)r.Result);
            Assert.True(val.IsBool);
            Assert.Equal(false, val.GetValue<bool>());
        }

        [Fact]
        public void TestNullValue()
        {
            ParseResult<JsonTokenGeneric, JSon> r = Parser.Parse("null");            
            Assert.False(r.IsError);
            Assert.True(r.Result.IsNull);
        }

        #endregion

        #region OBJECT

        [Fact]
        public void TestEmptyObjectValue()
        {
            ParseResult<JsonTokenGeneric, JSon> r = Parser.Parse("{}");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.True(r.Result.IsObject);
            Assert.Equal(0, ((JObject)r.Result).Count);
        }


        private void AssertString(JObject obj, string key, string value)
        {
            Assert.True(obj.ContainsKey(key));
            Assert.True(obj[key].IsValue);
            JValue val = (JValue) obj[key];
            Assert.True(val.IsString);
            Assert.Equal(value, val.GetValue<string>() );
        }
        
        private void AssertInt(JObject obj, string key, int value)
        {
            Assert.True(obj.ContainsKey(key));
            Assert.True(obj[key].IsValue);
            JValue val = (JValue) obj[key];
            Assert.True(val.IsInt);
            Assert.Equal(value, val.GetValue<int>() );
        }
        
        
        private void AssertDouble(JObject obj, string key, double value)
        {
            Assert.True(obj.ContainsKey(key));
            Assert.True(obj[key].IsValue);
            JValue val = (JValue) obj[key];
            Assert.True(val.IsDouble);
            Assert.Equal(value, val.GetValue<double>() );
        }
        
        
        private void AssertString(JList list, int index, string value)
        {
            Assert.True(list[index].IsValue);
            JValue val = (JValue) list[index];
            Assert.True(val.IsString);
            Assert.Equal(value, val.GetValue<string>() );
        }
        
        private void AssertInt(JList list, int index, int value)
        {
            Assert.True(list[index].IsValue);
            JValue val = (JValue) list[index];
            Assert.True(val.IsInt);
            Assert.Equal(value, val.GetValue<int>() );
        }
        
        
        private void AssertDouble(JList list, int index, double value)
        {
            Assert.True(list[index].IsValue);
            JValue val = (JValue) list[index];
            Assert.True(val.IsDouble);
            Assert.Equal(value, val.GetValue<double>() );
        }
        
        
        private void AssertBool(JList list, int index, bool value)
        {
            Assert.True(list[index].IsValue);
            JValue val = (JValue) list[index];
            Assert.True(val.IsBool);
            Assert.Equal(value, val.GetValue<bool>() );
        }
        
        
        private void AssertObject(JList list, int index, int count)
        {
            Assert.True(list[index].IsObject);
            JObject val = (JObject) list[index];
            Assert.Equal(count, val.Count);
        }
        
        
        [Fact]
        public void TestSinglePropertyObjectValue()
        {
            ParseResult<JsonTokenGeneric, JSon> r = Parser.Parse("{\"prop\":\"value\"}");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.True(r.Result.IsObject);
            JObject values = (JObject)r.Result;
            Assert.Equal(1,values.Count);
            AssertString(values,"prop","value");
        }

        [Fact]
        public void TestManyPropertyObjectValue()
        {
            string json = "{\"p1\":\"v1\",\"p2\":\"v2\"}";
            json = "{\"p1\":\"v1\" , \"p2\":\"v2\" }";
            ParseResult<JsonTokenGeneric,JSon> r = Parser.Parse(json);
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.True(r.Result.IsObject);
            JObject values = (JObject)r.Result;
            Assert.Equal(2,values.Count);
            AssertString(values,"p1","v1");
            AssertString(values,"p2","v2");
        }

        [Fact]
        public void TestManyNestedPropertyObjectValue()
        {
            string json = "{\"p1\":\"v1\",\"p2\":\"v2\",\"p3\":{\"inner1\":1}}";

            ParseResult<JsonTokenGeneric,JSon> r = Parser.Parse(json);
            Assert.False(r.IsError);
            Assert.NotNull(r);
            Assert.True(r.Result.IsObject);
            JObject values = (JObject)r.Result;
            Assert.Equal(3,values.Count);
            AssertString(values,"p1","v1");
            AssertString(values,"p2","v2");

            Assert.True(values.ContainsKey("p3"));
            JSon inner = values["p3"];
            Assert.True(inner.IsObject);
            JObject innerObj = (JObject) inner;
            
            Assert.Equal(1, innerObj.Count);
            AssertInt(innerObj,"inner1",1);
        }

        #endregion


        #region LIST
        
        
        [Fact]
        public void TestEmptyListValue()
        {
            ParseResult<JsonTokenGeneric,JSon> r = Parser.Parse("[]");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.True(r.Result.IsList);
            Assert.Equal(0, ((JList)r.Result).Count);
        }

        [Fact]
        public void TestSingleListValue()
        {
            ParseResult<JsonTokenGeneric, JSon> r = Parser.Parse("[1]");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.True(r.Result.IsList);
            JList list = (JList) r.Result;
            Assert.Equal(1, list.Count);
            AssertInt(list, 0, 1);
        }

        [Fact]
        public void TestManyListValue()
        {
            ParseResult<JsonTokenGeneric,JSon> r = Parser.Parse("[1,2]");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.True(r.Result.IsList);
            JList list = (JList) r.Result;
            Assert.Equal(2, list.Count);
            AssertInt(list, 0, 1);
            AssertInt(list, 1, 2);
        }

        [Fact]
        public void TestManyMixedListValue()
        {
            ParseResult<JsonTokenGeneric,JSon> r = Parser.Parse("[1,null,{},true,42.58]");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.NotNull(r.Result);
            object val = r.Result;
            Assert.True(r.Result.IsList);
            JList list = (JList) r.Result;
            Assert.Equal(5, ((JList)r.Result).Count);
            AssertInt(list,0,1);
            Assert.True(((JList)r.Result)[1].IsNull);
            AssertObject(list,2,0);
            AssertBool(list,3,true);
            AssertDouble(list,4,42.58d);
        }

        #endregion

       
    }
}
