using Xunit;
using sly.parser;
using sly.parser.generator;
using System.Collections.Generic;
using jsonparser;

namespace ParserTests
{
    
    public class JsonTests
    {

        private static Parser<JsonToken> Parser;
        
      
        public JsonTests()
        {
            JSONParser jsonParser = new JSONParser();
            ParserBuilder builder = new ParserBuilder();
            Parser = builder.BuildParser<JsonToken>(jsonParser, ParserType.LL_RECURSIVE_DESCENT, "root");
        }

        


#region VALUES
        [Fact]
        public void TestIntValue()
        {
            ParseResult<JsonToken> r = Parser.Parse("1");
            Assert.False(r.IsError);
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.IsAssignableFrom(typeof(int), r.Result);                
            Assert.Equal(1, (int)r.Result);
        }

        [Fact]
        public void TestDoubleValue()
        {
            ParseResult<JsonToken> r = Parser.Parse("0.1");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.IsAssignableFrom(typeof(double),r.Result);
            Assert.Equal(0.1d, (double)r.Result);
        }

        [Fact]
        public void TestStringValue()
        {
            string val = "hello world!";
            ParseResult<JsonToken> r = Parser.Parse("\"" + val + "\"");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.IsAssignableFrom(typeof(string),r.Result);
            Assert.Equal(val, (string)r.Result);
        }

        [Fact]
        public void TestTrueBooleanValue()
        {
            ParseResult<JsonToken> r = Parser.Parse("true");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.IsAssignableFrom(typeof(bool),r.Result);
            Assert.Equal(true, (bool)r.Result);
        }

        [Fact]
        public void TestFalseBooleanValue()
        {
            ParseResult<JsonToken> r = Parser.Parse("false");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.IsAssignableFrom(typeof(bool),r.Result);
            Assert.Equal(false, (bool)r.Result);
        }

        [Fact]
        public void TestNullValue()
        {
            ParseResult<JsonToken> r = Parser.Parse("null");            
            Assert.False(r.IsError);
            Assert.Null(r.Result);
        }

        #endregion

        #region OBJECT

        [Fact]
        public void TestEmptyObjectValue()
        {
            ParseResult<JsonToken> r = Parser.Parse("{}");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.IsAssignableFrom(typeof(Dictionary<string,object>),r.Result);
            Assert.Equal(0, ((Dictionary<string, object>)r.Result).Count);
        }


        [Fact]
        public void TestSinglePropertyObjectValue()
        {
            ParseResult<JsonToken> r = Parser.Parse("{\"prop\":\"value\"}");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.IsAssignableFrom(typeof(Dictionary<string, object>),r.Result);
            Dictionary<string, object> values = (Dictionary<string, object>)r.Result;
            Assert.True(values.ContainsKey("prop"));
            Assert.Equal("value", values["prop"]);
        }

        [Fact]
        public void TestManyPropertyObjectValue()
        {
            string json = "{\"p1\":\"v1\",\"p2\":\"v2\"}";
            json = "{\"p1\":\"v1\" , \"p2\":\"v2\" }";
            ParseResult<JsonToken> r = Parser.Parse(json);
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.IsAssignableFrom(typeof(Dictionary<string, object>),r.Result);
            Dictionary<string, object> values = (Dictionary<string, object>)r.Result;
            Assert.True(values.ContainsKey("p1"));
            Assert.Equal("v1", values["p1"]);
            Assert.True(values.ContainsKey("p2"));
            Assert.Equal("v2", values["p2"]);
        }

        [Fact]
        public void TestManyNestedPropertyObjectValue()
        {
            string json = "{\"p1\":\"v1\",\"p2\":\"v2\",\"p3\":{\"inner1\":1}}";

            ParseResult<JsonToken> r = Parser.Parse(json);
            Assert.False(r.IsError);
            Assert.NotNull(r);
            Assert.IsAssignableFrom(typeof(Dictionary<string, object>),r.Result);
            Dictionary<string, object> values = (Dictionary<string, object>)r.Result;
            Assert.True(values.ContainsKey("p1"));
            Assert.Equal("v1", values["p1"]);
            Assert.True(values.ContainsKey("p2"));
            Assert.Equal("v2", values["p2"]);

            Assert.True(values.ContainsKey("p3"));
            object inner = values["p3"];
            Assert.IsAssignableFrom(typeof(Dictionary<string, object>),inner);
            Dictionary<string, object> innerDic = (Dictionary<string, object>)inner;
            Assert.Equal(1, innerDic.Count);
            Assert.True(innerDic.ContainsKey("inner1"));
            Assert.Equal(1, innerDic["inner1"]);
        }

        #endregion


        #region LIST
        [Fact]
        public void TestEmptyListValue()
        {
            ParseResult<JsonToken> r = Parser.Parse("[]");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.IsAssignableFrom(typeof(List<object>),r.Result);
            Assert.Equal(0, ((List<object>)r.Result).Count);
        }

        [Fact]
        public void TestSingleListValue()
        {
            ParseResult<JsonToken> r = Parser.Parse("[1]");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.IsAssignableFrom(typeof(List<object>),r.Result);
            Assert.Equal(1, ((List<object>)r.Result).Count);
            Assert.Equal(1, ((List<object>)r.Result)[0]);

        }

        [Fact]
        public void TestManyListValue()
        {
            ParseResult<JsonToken> r = Parser.Parse("[1,2]");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.IsAssignableFrom(typeof(List<object>),r.Result);
            Assert.Equal(2, ((List<object>)r.Result).Count);
            Assert.Equal(1, ((List<object>)r.Result)[0]);
            Assert.Equal(2, ((List<object>)r.Result)[1]);
        }

        [Fact]
        public void TestManyMixedListValue()
        {
            ParseResult<JsonToken> r = Parser.Parse("[1,null,{},true,42.58]");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.NotNull(r.Result);
            object val = r.Result;
            Assert.IsAssignableFrom(typeof(List<object>), r.Result);
            Assert.Equal(5, ((List<object>)r.Result).Count);
            Assert.Equal(1, ((List<object>)r.Result)[0]);
            Assert.Null(((List<object>)r.Result)[1]);
            Assert.IsAssignableFrom(typeof(Dictionary<string, object>),((List<object>)r.Result)[2]);
            Assert.Equal(0, ((Dictionary<string, object>)(((List<object>)r.Result)[2])).Count);
            Assert.Equal(true, ((List<object>)r.Result)[3]);
            Assert.Equal(42.58d, ((List<object>)r.Result)[4]);
        }

        #endregion

       
    }
}
