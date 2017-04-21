using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using jsonparser;
using cpg.parser.parsgenerator.parser;
using lexer;
using parser.parsergenerator.generator;
using System.Collections.Generic;

namespace ParserTests
{


    [TestClass]
    public class JsonTests
    {

        private static Parser<JsonToken> Parser;

        private static Lexer<JsonToken> Lexer;

        [ClassInitialize]
        public static  void Init(TestContext context)
        {
            Lexer = JSONParser.BuildJsonLexer(new Lexer<JsonToken>());
            Parser = ParserGenerator.BuildParser<JsonToken>(typeof(JSONParser), ParserType.LL, "root");
        }

        private object Parse(string json )
        {
            List<Token<JsonToken>> tokens = Lexer.Tokenize(json).ToList<Token<JsonToken>>();
            object r = Parser.Parse(tokens);
            return r;
        }


#region VALUES
        [TestMethod]
        public void TestIntValue()
        {
            object r = Parse("1");
            Assert.IsNotNull(r);
            Assert.IsInstanceOfType(r, typeof(int));
            Assert.AreEqual(1, (int)r);
        }

        [TestMethod]
        public void TestDoubleValue()
        {
            object r = Parse("0.1");
            Assert.IsNotNull(r);
            Assert.IsInstanceOfType(r, typeof(double));
            Assert.AreEqual(0.1d, (double)r);
        }

        [TestMethod]
        public void TestStringValue()
        {
            string val = "hello world!";            
            object r = Parse("\"" + val + "\"");
            Assert.IsNotNull(r);
            Assert.IsInstanceOfType(r, typeof(string));
            Assert.AreEqual(val, (string)r);
        }

        [TestMethod]
        public void TestTrueBooleanValue()
        {   
            object r = Parse("true");
            Assert.IsNotNull(r);
            Assert.IsInstanceOfType(r, typeof(bool));
            Assert.AreEqual(true, (bool)r);
        }

        [TestMethod]
        public void TestFalseBooleanValue()
        {
            object r = Parse("false");
            Assert.IsNotNull(r);
            Assert.IsInstanceOfType(r, typeof(bool));
            Assert.AreEqual(false, (bool)r);
        }

        [TestMethod]
        public void TestNullValue()
        {
            object r = Parse("null");            
            Assert.IsNull(r);
        }

        #endregion

        #region OBJECT

        [TestMethod]
        public void TestEmptyObjectValue()
        {
            object r = Parse("{}");
            Assert.IsNotNull(r);
            Assert.IsInstanceOfType(r, typeof(Dictionary<string,object>));
            Assert.AreEqual(0, ((Dictionary<string, object>)r).Count);
        }


        [TestMethod]
        public void TestSinglePropertyObjectValue()
        {
            object r = Parse("{\"prop\":\"value\"}");
            Assert.IsNotNull(r);
            Assert.IsInstanceOfType(r, typeof(Dictionary<string, object>));
            Dictionary<string, object> values = (Dictionary<string, object>)r;
            Assert.IsTrue(values.ContainsKey("prop"));
            Assert.AreEqual("value", values["prop"]);
        }

        [TestMethod]
        public void TestManyPropertyObjectValue()
        {
            string json = "{\"p1\":\"v1\",\"p2\":\"v2\"}";
            json = "{\"p1\":\"v1\" , \"p2\":\"v2\" }";
            object r = Parse(json);
            Assert.IsNotNull(r);
            Assert.IsInstanceOfType(r, typeof(Dictionary<string, object>));
            Dictionary<string, object> values = (Dictionary<string, object>)r;
            Assert.IsTrue(values.ContainsKey("p1"));
            Assert.AreEqual("v1", values["p1"]);
            Assert.IsTrue(values.ContainsKey("p2"));
            Assert.AreEqual("v2", values["p2"]);
        }

        [TestMethod]
        public void TestManyNestedPropertyObjectValue()
        {
            string json = "{\"p1\":\"v1\",\"p2\":\"v2\",\"p3\":{\"inner1\":1}}";
            
            object r = Parse(json);
            Assert.IsNotNull(r);
            Assert.IsInstanceOfType(r, typeof(Dictionary<string, object>));
            Dictionary<string, object> values = (Dictionary<string, object>)r;
            Assert.IsTrue(values.ContainsKey("p1"));
            Assert.AreEqual("v1", values["p1"]);
            Assert.IsTrue(values.ContainsKey("p2"));
            Assert.AreEqual("v2", values["p2"]);

            Assert.IsTrue(values.ContainsKey("p3"));
            object inner = values["p3"];
            Assert.IsInstanceOfType(inner, typeof(Dictionary<string, object>));
            Dictionary<string, object> innerDic = (Dictionary<string, object>)inner;
            Assert.AreEqual(1, innerDic.Count);
            Assert.IsTrue(innerDic.ContainsKey("inner1"));
            Assert.AreEqual(1, innerDic["inner1"]);
        }

        #endregion


        #region LIST
        [TestMethod]
        public void TestEmptyListValue()
        {
            object r = Parse("[]");
            Assert.IsNotNull(r);
            Assert.IsInstanceOfType(r, typeof(List<object>));
            Assert.AreEqual(0, ((List<object>)r).Count);
        }

        [TestMethod]
        public void TestSingleListValue()
        {
            object r = Parse("[1]");
            Assert.IsNotNull(r);
            Assert.IsInstanceOfType(r, typeof(List<object>));
            Assert.AreEqual(1, ((List<object>)r).Count);
            Assert.AreEqual(1, ((List<object>)r)[0]);

        }

        [TestMethod]
        public void TestManyListValue()
        {
            object r = Parse("[1,2]");
            Assert.IsNotNull(r);
            Assert.IsInstanceOfType(r, typeof(List<object>));
            Assert.AreEqual(2, ((List<object>)r).Count);
            Assert.AreEqual(1, ((List<object>)r)[0]);
            Assert.AreEqual(2, ((List<object>)r)[1]);
        }

        [TestMethod]
        public void TestManyMixedListValue()
        {
            object r = Parse("[1,null,{},true,42.58]");
            Assert.IsNotNull(r);
            Assert.IsInstanceOfType(r, typeof(List<object>));
            Assert.AreEqual(5, ((List<object>)r).Count);
            Assert.AreEqual(1, ((List<object>)r)[0]);
            Assert.IsNull(((List<object>)r)[1]);
            Assert.IsInstanceOfType(((List<object>)r)[2],typeof(Dictionary<string, object>));
            Assert.AreEqual(0, ((Dictionary<string, object>)(((List<object>)r)[2])).Count);
            Assert.AreEqual(true, ((List<object>)r)[3]);
            Assert.AreEqual(42.58d, ((List<object>)r)[4]);
        }

#endregion
    }
}
