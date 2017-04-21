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
            string val = "\"hello world!\"";            
            object r = Parse(val);
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

        [TestMethod]
        public void TestEmptyObjectValue()
        {
            object r = Parse("{}");
            Assert.IsNotNull(r);
            Assert.IsInstanceOfType(r, typeof(Dictionary<string,object>));
            Assert.AreEqual(0, ((Dictionary<string, object>)r).Count);
        }

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
            Assert.AreEqual(1, ((List<object>)r).Count);
            Assert.AreEqual(1, ((List<object>)r)[0]);

        }
    }
}
