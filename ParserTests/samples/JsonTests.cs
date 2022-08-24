using jsonparser;
using jsonparser.JsonModel;
using NFluent;
using sly.parser;
using sly.parser.generator;
using Xunit;

namespace ParserTests.samples
{
    public class JsonTests
    {
        public JsonTests()
        {
            var jsonParser = new JSONParser();
            var builder = new ParserBuilder<JsonToken, JSon>();
            var build = builder.BuildParser(jsonParser, ParserType.LL_RECURSIVE_DESCENT, "root");
            Check.That(build).IsOk();
            Parser = build.Result;
        }

        private static Parser<JsonToken, JSon> Parser;


        



      

        [Fact]
        public void TestDoubleValue()
        {
            var r = Parser.Parse("0.1");
            Check.That(r).IsOkParsing();
            Check.That(r.Result.IsValue).IsTrue();
            Check.That(((JValue) r.Result).IsDouble).IsTrue();
            Check.That(((JValue) r.Result).GetValue<double>()).IsEqualTo(0.1d);
        }


        [Fact]
        public void TestEmptyListValue()
        {
            var r = Parser.Parse("[]");
            Check.That(r).IsOkParsing();
            Check.That(r.Result.IsList).IsTrue();
            Check.That(((JList) r.Result)).IsEmpty();
        }

        [Fact]
        public void TestEmptyObjectValue()
        {
            var r = Parser.Parse("{}");
            Check.That(r).IsOkParsing();
            Check.That(r.Result.IsObject).IsTrue();
            Check.That((JObject) r.Result).IsEmpty();
        }

        [Fact]
        public void TestFalseBooleanValue()
        {
            var r = Parser.Parse("false");
            Check.That(r).IsOkParsing();
            Check.That(r.Result.IsValue).IsTrue();
            var val = (JValue) r.Result;
            Check.That(val.IsBool).IsTrue();
            Check.That(val.GetValue<bool>()).IsFalse();
        }

        [Fact]
        public void TestIntValue()
        {
            var r = Parser.Parse("1");
            Check.That(r).IsOkParsing();
            Check.That(r.Result.IsValue).IsTrue();
            Check.That(((JValue) r.Result).IsInt).IsTrue();
            Check.That(((JValue) r.Result).GetValue<int>()).IsEqualTo(1);
        }

        [Fact]
        public void TestManyListValue()
        {
            var r = Parser.Parse("[1,2]");
            Check.That(r).IsOkParsing();
            Check.That(r.Result.IsList).IsTrue();
            var list = (JList) r.Result;
            Check.That(list).CountIs(2);
            Check.That(list).HasItem(0, 1);
            Check.That(list).HasItem(1, 2);
        }

        [Fact]
        public void TestManyMixedListValue()
        {
            var r = Parser.Parse("[1,null,{},true,42.58]");
            Check.That(r).IsOkParsing();
            object val = r.Result;
            Check.That(r.Result.IsList).IsTrue();
            var list = (JList) r.Result;
            Check.That(((JList) r.Result).Count).IsEqualTo(5);
            Check.That(list).HasItem(0, 1);
            Check.That(((JList) r.Result)[1].IsNull).IsTrue();
            Check.That(list).HasObjectItem(2, 0);
            Check.That(list).HasItem(3, true);
            Check.That(list).HasItem(4, 42.58d);
        }

        [Fact]
        public void TestManyNestedPropertyObjectValue()
        {
            var json = "{\"p1\":\"v1\",\"p2\":\"v2\",\"p3\":{\"inner1\":1}}";

            var r = Parser.Parse(json);
            Check.That(r).IsOkParsing();
            Check.That(r.Result.IsObject).IsTrue();
            var values = (JObject) r.Result;
            Check.That(values.Count).IsEqualTo(3);
            Check.That(values).HasProperty("p1", "v1");
            Check.That(values).HasProperty("p2", "v2");

            Check.That(values.ContainsKey("p3")).IsTrue();
            var inner = values["p3"];
            Check.That(inner.IsObject).IsTrue();
            var innerObj = (JObject) inner;

            Check.That(innerObj.Count).IsEqualTo(1);
            Check.That(innerObj).HasProperty("inner1", 1);
        }

        [Fact]
        public void TestManyPropertyObjectValue()
        {
            var json = "{\"p1\":\"v1\",\"p2\":\"v2\"}";
            json = "{\"p1\":\"v1\" , \"p2\":\"v2\" }";
            var r = Parser.Parse(json);
            Check.That(r).IsOkParsing();
            Check.That(r.Result.IsObject).IsTrue();
            var values = (JObject) r.Result;
            Check.That(values).CountIs(2);
            Check.That(values).HasProperty("p1", "v1");
            Check.That(values).HasProperty("p2", "v2");
        }

        [Fact]
        public void TestNullValue()
        {
            var r = Parser.Parse("null");
            Check.That(r).IsOkParsing();
            Check.That(r.Result.IsNull).IsTrue();
        }

        [Fact]
        public void TestSingleListValue()
        {
            var r = Parser.Parse("[1]");
            Check.That(r).IsOkParsing();
            Check.That(r.Result.IsList).IsTrue();
            var list = (JList) r.Result;
            Check.That(list).CountIs(1);
            Check.That(list).HasItem(0, 1);
        }


        [Fact]
        public void TestSinglePropertyObjectValue()
        {
            var r = Parser.Parse("{\"prop\":\"value\"}");
            Check.That(r).IsOkParsing();
            Check.That(r.Result.IsObject).IsTrue();
            var values = (JObject) r.Result;
            Check.That(values).CountIs(1);
            Check.That(values).HasProperty("prop", "value");
        }

        [Fact]
        public void TestStringValue()
        {
            var val = "hello world!";
            var r = Parser.Parse("\"" + val + "\"");
            Check.That(r).IsOkParsing();
            Check.That(r.Result.IsValue).IsTrue();
            Check.That(((JValue) r.Result).IsString).IsTrue();
            Check.That(((JValue) r.Result).GetValue<string>()).IsEqualTo(val);
        }

        [Fact]
        public void TestTrueBooleanValue()
        {
            var r = Parser.Parse("true");
            Check.That(r).IsOkParsing();
            Check.That(r.Result.IsValue).IsTrue();
            var val = (JValue) r.Result;
            Check.That(val.IsBool).IsTrue();
            Check.That(val.IsBool).IsTrue();
            Check.That(val.GetValue<bool>()).IsTrue();
        }
    }
}