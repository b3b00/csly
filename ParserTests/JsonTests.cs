using System.Collections.Generic;
using jsonparser;
using NUnit.Framework;
using sly.parser;
using sly.parser.generator;

namespace ParserTests
{
    [TestFixture]
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
        [Test]
        public void TestIntValue()
        {
            ParseResult<JsonToken> r = Parser.Parse("1");
            Assert.False(r.IsError);
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.IsAssignableFrom(typeof(int), r.Result);
            Assert.AreEqual(1, (int)r.Result);
        }

        [Test]
        public void TestDoubleValue()
        {
            ParseResult<JsonToken> r = Parser.Parse("0.1");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.IsAssignableFrom(typeof(double), r.Result);
            Assert.AreEqual(0.1d, (double)r.Result);
        }

        [Test]
        public void TestStringValue()
        {
            string val = "hello world!";
            ParseResult<JsonToken> r = Parser.Parse("\"" + val + "\"");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.IsAssignableFrom(typeof(string), r.Result);
            Assert.AreEqual(val, (string)r.Result);
        }

        [Test]
        public void TestTrueBooleanValue()
        {
            ParseResult<JsonToken> r = Parser.Parse("true");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.IsAssignableFrom(typeof(bool), r.Result);
            Assert.AreEqual(true, (bool)r.Result);
        }

        [Test]
        public void TestFalseBooleanValue()
        {
            ParseResult<JsonToken> r = Parser.Parse("false");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.IsAssignableFrom(typeof(bool), r.Result);
            Assert.AreEqual(false, (bool)r.Result);
        }

        [Test]
        public void TestNullValue()
        {
            ParseResult<JsonToken> r = Parser.Parse("null");
            Assert.False(r.IsError);
            Assert.Null(r.Result);
        }

        #endregion

        #region OBJECT

        [Test]
        public void TestEmptyObjectValue()
        {
            ParseResult<JsonToken> r = Parser.Parse("{}");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.IsAssignableFrom(typeof(Dictionary<string, object>), r.Result);
            Assert.AreEqual(0, ((Dictionary<string, object>)r.Result).Count);
        }


        [Test]
        public void TestSinglePropertyObjectValue()
        {
            ParseResult<JsonToken> r = Parser.Parse("{\"prop\":\"value\"}");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.IsAssignableFrom(typeof(Dictionary<string, object>), r.Result);
            Dictionary<string, object> values = (Dictionary<string, object>)r.Result;
            Assert.True(values.ContainsKey("prop"));
            Assert.AreEqual("value", values["prop"]);
        }

        [Test]
        public void TestManyPropertyObjectValue()
        {
            string json = "{\"p1\":\"v1\",\"p2\":\"v2\"}";
            json = "{\"p1\":\"v1\" , \"p2\":\"v2\" }";
            ParseResult<JsonToken> r = Parser.Parse(json);
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.IsAssignableFrom(typeof(Dictionary<string, object>), r.Result);
            Dictionary<string, object> values = (Dictionary<string, object>)r.Result;
            Assert.True(values.ContainsKey("p1"));
            Assert.AreEqual("v1", values["p1"]);
            Assert.True(values.ContainsKey("p2"));
            Assert.AreEqual("v2", values["p2"]);
        }

        [Test]
        public void TestManyNestedPropertyObjectValue()
        {
            string json = "{\"p1\":\"v1\",\"p2\":\"v2\",\"p3\":{\"inner1\":1}}";

            ParseResult<JsonToken> r = Parser.Parse(json);
            Assert.False(r.IsError);
            Assert.NotNull(r);
            Assert.IsAssignableFrom(typeof(Dictionary<string, object>), r.Result);
            Dictionary<string, object> values = (Dictionary<string, object>)r.Result;
            Assert.True(values.ContainsKey("p1"));
            Assert.AreEqual("v1", values["p1"]);
            Assert.True(values.ContainsKey("p2"));
            Assert.AreEqual("v2", values["p2"]);

            Assert.True(values.ContainsKey("p3"));
            object inner = values["p3"];
            Assert.IsAssignableFrom(typeof(Dictionary<string, object>), inner);
            Dictionary<string, object> innerDic = (Dictionary<string, object>)inner;
            Assert.AreEqual(1, innerDic.Count);
            Assert.True(innerDic.ContainsKey("inner1"));
            Assert.AreEqual(1, innerDic["inner1"]);
        }

        #endregion


        #region LIST
        [Test]
        public void TestEmptyListValue()
        {
            ParseResult<JsonToken> r = Parser.Parse("[]");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.IsAssignableFrom(typeof(List<object>), r.Result);
            Assert.AreEqual(0, ((List<object>)r.Result).Count);
        }

        [Test]
        public void TestSingleListValue()
        {
            ParseResult<JsonToken> r = Parser.Parse("[1]");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.IsAssignableFrom(typeof(List<object>), r.Result);
            Assert.AreEqual(1, ((List<object>)r.Result).Count);
            Assert.AreEqual(1, ((List<object>)r.Result)[0]);

        }

        [Test]
        public void TestManyListValue()
        {
            ParseResult<JsonToken> r = Parser.Parse("[1,2]");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.IsAssignableFrom(typeof(List<object>), r.Result);
            Assert.AreEqual(2, ((List<object>)r.Result).Count);
            Assert.AreEqual(1, ((List<object>)r.Result)[0]);
            Assert.AreEqual(2, ((List<object>)r.Result)[1]);
        }

        [Test]
        public void TestManyMixedListValue()
        {
            ParseResult<JsonToken> r = Parser.Parse("[1,null,{},true,42.58]");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.NotNull(r.Result);
            object val = r.Result;
            Assert.IsAssignableFrom(typeof(List<object>), r.Result);
            Assert.AreEqual(5, ((List<object>)r.Result).Count);
            Assert.AreEqual(1, ((List<object>)r.Result)[0]);
            Assert.Null(((List<object>)r.Result)[1]);
            Assert.IsAssignableFrom(typeof(Dictionary<string, object>), ((List<object>)r.Result)[2]);
            Assert.AreEqual(0, ((Dictionary<string, object>)(((List<object>)r.Result)[2])).Count);
            Assert.AreEqual(true, ((List<object>)r.Result)[3]);
            Assert.AreEqual(42.58d, ((List<object>)r.Result)[4]);
        }

        #endregion
    }
}
