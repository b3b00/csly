using System.Linq;
using Xunit;
using cpg.parser.parsgenerator.parser;
using lexer;
using parser.parsergenerator.generator;
using System.Collections.Generic;
using jsonparser;

namespace ParserTests
{
    
    public class JsonTests
    {

        private static Parser<JsonToken> Parser;

        private static Lexer<JsonToken> Lexer;

      
        public JsonTests()
        {
            Lexer = JSONParser.BuildJsonLexer(new Lexer<JsonToken>());
            Parser = ParserGenerator.BuildParser<JsonToken>(typeof(JSONParser), ParserType.RECURSIVE_DESCENT, "root");
        }

        private object Parse(string json )
        {
            List<Token<JsonToken>> tokens = Lexer.Tokenize(json).ToList<Token<JsonToken>>();
            object r = Parser.Parse(tokens);
            return r;
        }


#region VALUES
        [Fact]
        public void TestIntValue()
        {
            object r = Parse("1");
            Assert.NotNull(r);
            Assert.IsAssignableFrom(typeof(int), r);                
            Assert.Equal(1, (int)r);
        }

        [Fact]
        public void TestDoubleValue()
        {
            object r = Parse("0.1");
            Assert.NotNull(r);
            Assert.IsAssignableFrom(typeof(double),r);
            Assert.Equal(0.1d, (double)r);
        }

        [Fact]
        public void TestStringValue()
        {
            string val = "hello world!";            
            object r = Parse("\"" + val + "\"");
            Assert.NotNull(r);
            Assert.IsAssignableFrom(typeof(string),r);
            Assert.Equal(val, (string)r);
        }

        [Fact]
        public void TestTrueBooleanValue()
        {   
            object r = Parse("true");
            Assert.NotNull(r);
            Assert.IsAssignableFrom(typeof(bool),r);
            Assert.Equal(true, (bool)r);
        }

        [Fact]
        public void TestFalseBooleanValue()
        {
            object r = Parse("false");
            Assert.NotNull(r);
            Assert.IsAssignableFrom(typeof(bool),r);
            Assert.Equal(false, (bool)r);
        }

        [Fact]
        public void TestNullValue()
        {
            object r = Parse("null");            
            Assert.Null(r);
        }

        #endregion

        #region OBJECT

        [Fact]
        public void TestEmptyObjectValue()
        {
            object r = Parse("{}");
            Assert.NotNull(r);
            Assert.IsAssignableFrom(typeof(Dictionary<string,object>),r);
            Assert.Equal(0, ((Dictionary<string, object>)r).Count);
        }


        [Fact]
        public void TestSinglePropertyObjectValue()
        {
            object r = Parse("{\"prop\":\"value\"}");
            Assert.NotNull(r);
            Assert.IsAssignableFrom(typeof(Dictionary<string, object>),r);
            Dictionary<string, object> values = (Dictionary<string, object>)r;
            Assert.True(values.ContainsKey("prop"));
            Assert.Equal("value", values["prop"]);
        }

        [Fact]
        public void TestManyPropertyObjectValue()
        {
            string json = "{\"p1\":\"v1\",\"p2\":\"v2\"}";
            json = "{\"p1\":\"v1\" , \"p2\":\"v2\" }";
            object r = Parse(json);
            Assert.NotNull(r);
            Assert.IsAssignableFrom(typeof(Dictionary<string, object>),r);
            Dictionary<string, object> values = (Dictionary<string, object>)r;
            Assert.True(values.ContainsKey("p1"));
            Assert.Equal("v1", values["p1"]);
            Assert.True(values.ContainsKey("p2"));
            Assert.Equal("v2", values["p2"]);
        }

        [Fact]
        public void TestManyNestedPropertyObjectValue()
        {
            string json = "{\"p1\":\"v1\",\"p2\":\"v2\",\"p3\":{\"inner1\":1}}";
            
            object r = Parse(json);
            Assert.NotNull(r);
            Assert.IsAssignableFrom(typeof(Dictionary<string, object>),r);
            Dictionary<string, object> values = (Dictionary<string, object>)r;
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
            object r = Parse("[]");
            Assert.NotNull(r);
            Assert.IsAssignableFrom(typeof(List<object>),r);
            Assert.Equal(0, ((List<object>)r).Count);
        }

        [Fact]
        public void TestSingleListValue()
        {
            object r = Parse("[1]");
            Assert.NotNull(r);
            Assert.IsAssignableFrom(typeof(List<object>),r);
            Assert.Equal(1, ((List<object>)r).Count);
            Assert.Equal(1, ((List<object>)r)[0]);

        }

        [Fact]
        public void TestManyListValue()
        {
            object r = Parse("[1,2]");
            Assert.NotNull(r);
            Assert.IsAssignableFrom(typeof(List<object>),r);
            Assert.Equal(2, ((List<object>)r).Count);
            Assert.Equal(1, ((List<object>)r)[0]);
            Assert.Equal(2, ((List<object>)r)[1]);
        }

        [Fact]
        public void TestManyMixedListValue()
        {
            object r = Parse("[1,null,{},true,42.58]");
            Assert.NotNull(r);
            Assert.IsAssignableFrom(typeof(List<object>),r);
            Assert.Equal(5, ((List<object>)r).Count);
            Assert.Equal(1, ((List<object>)r)[0]);
            Assert.Null(((List<object>)r)[1]);
            Assert.IsAssignableFrom(typeof(Dictionary<string, object>),((List<object>)r)[2]);
            Assert.Equal(0, ((Dictionary<string, object>)(((List<object>)r)[2])).Count);
            Assert.Equal(true, ((List<object>)r)[3]);
            Assert.Equal(42.58d, ((List<object>)r)[4]);
        }

#endregion
    }
}
