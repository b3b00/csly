using jsonparser;
using jsonparser.JsonModel;
using NFluent;
using sly.buildresult;
using sly.parser;
using sly.parser.generator;
using Xunit;

namespace ParserTests.samples
{
    public class GeneratedJsonGenericTests
    {
        public GeneratedJsonGenericTests()
        {
            // TODO : reactivate test
            EbnfJsonGenericParserGenerator parserGenerator = new EbnfJsonGenericParserGenerator();
            BuildResult<Parser<JsonTokenGeneric, JSon>> parserBuild = parserGenerator.GetParser();
            Check.That(parserBuild).IsNotNull();
            Check.That(parserBuild).IsOk();
            Check.That(parserBuild.Result).IsNotNull();
            this.JsonParser = parserBuild.Result;
        }

        private Parser<JsonTokenGeneric, JSon> JsonParser;

      

        [Fact]
        public void TestDoubleValue()
        {
            var r = JsonParser.Parse("0.1");
            Check.That(r).IsOkParsing();
            Check.That(r.Result.IsValue).IsTrue();
            Check.That(((JValue) r.Result).IsDouble).IsTrue();
            Check.That(((JValue) r.Result).GetValue<double>()).IsEqualTo(0.1d);
        }


        [Fact]
        public void TestEmptyListValue()
        {
            var r = JsonParser.Parse("[]");
            Check.That(r).IsOkParsing();
            
            Check.That(r.Result.IsList).IsTrue();
            Check.That(((JList) r.Result)).CountIs(0);
        }

        [Fact]
        public void TestEmptyObjectValue()
        {
            var r = JsonParser.Parse("{}");
            Check.That(r).IsOkParsing();
            Check.That(r.Result.IsObject).IsTrue();
            Check.That(((JObject) r.Result)).CountIs(0);
        }

        [Fact]
        public void TestFalseBooleanValue()
        {
            var r = JsonParser.Parse("false");
            Check.That(r).IsOkParsing();
            Check.That(r.Result.IsValue).IsTrue();
            var val = (JValue) r.Result;
            Check.That(val.IsBool).IsTrue();
            Check.That(val.GetValue<bool>()).IsFalse();
        }

        [Fact]
        public void TestIntValue()
        {
            var r = JsonParser.Parse("1");
            Check.That(r).IsOkParsing();
            
            Check.That(r.Result.IsValue).IsTrue();
            Check.That(((JValue) r.Result).IsInt).IsTrue();
            
            Check.That(((JValue) r.Result).GetValue<int>()).IsEqualTo(1);
        }

        [Fact]
        public void TestManyListValue()
        {
            var r = JsonParser.Parse("[1,2]");
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
            var r = JsonParser.Parse("[1,null,{},true,42.58]");
            Check.That(r).IsOkParsing();
            object val = r.Result;
            Check.That(r.Result.IsList).IsTrue();
            var list = (JList) r.Result;
            Check.That((JList) r.Result).CountIs(5);
            Check.That(list).HasItem(0, 1);
            Check.That(((JList) r.Result)[1].IsNull).IsTrue();
            Check.That(list).HasObjectItem(2,0);
            Check.That(list).HasItem(3, true);
            Check.That(list).HasItem(4, 42.58d);
        }

        [Fact]
        public void TestManyNestedPropertyObjectValue()
        {
            var json = "{\"p1\":\"v1\",\"p2\":\"v2\",\"p3\":{\"inner1\":1}}";

            var r = JsonParser.Parse(json);
            Check.That(r).IsOkParsing();
            Check.That(r.Result.IsObject).IsTrue();
            var values = (JObject) r.Result;
            Check.That(values).CountIs(3);
            Check.That(values).HasProperty("p1", "v1");
            Check.That(values).HasProperty("p1", "v1");
            Check.That(values).HasProperty("p2", "v2");

            Check.That(values).HasObjectProperty("p3");            
            var inner = values["p3"];
            Check.That(inner.IsObject).IsTrue();
            var innerObj = (JObject) inner;

            Check.That(innerObj).CountIs(1);
            Check.That(innerObj).HasProperty("inner1", 1);
        }

        [Fact]
        public void TestManyPropertyObjectValue()
        {
            var json = "{\"p1\":\"v1\",\"p2\":\"v2\"}";
            json = "{\"p1\":\"v1\" , \"p2\":\"v2\" }";
            var r = JsonParser.Parse(json);
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
            var r = JsonParser.Parse("null");
            Check.That(r).IsOkParsing();
            Check.That(r.Result.IsNull).IsTrue();
        }

        [Fact]
        public void TestSingleListValue()
        {
            var r = JsonParser.Parse("[1]");
            Check.That(r).IsOkParsing();
            
            Check.That(r.Result.IsList).IsTrue();
            var list = (JList) r.Result;
            Check.That(list).CountIs(1);
            Check.That(list).HasItem(0, 1);
        }


        [Fact]
        public void TestSinglePropertyObjectValue()
        {
            var r = JsonParser.Parse("{\"prop\":\"value\"}");
            Check.That(r.IsError).IsFalse();
            Check.That(r.Result).IsNotNull();
            Check.That(r.Result.IsObject).IsTrue();
            var values = (JObject) r.Result;
            Check.That(values).CountIs(1);
            Check.That(values).HasProperty("prop", "value");
        }

        [Fact]
        public void TestStringValue()
        {
            var val = "hello";
            var r = JsonParser.Parse("\"" + val + "\"");
            Check.That(r).IsOkParsing();
            Check.That(r.Result.IsValue).IsTrue();
            Check.That(((JValue) r.Result).IsString).IsTrue();
            Check.That(((JValue) r.Result).GetValue<string>()).IsEqualTo(val);
        }

        [Fact]
        public void TestTrueBooleanValue()
        {
            var r = JsonParser.Parse("true");
            Check.That(r).IsOkParsing();
            
            Check.That(r.Result.IsValue).IsTrue();
            var val = (JValue) r.Result;
            Check.That(val.IsBool).IsTrue();
            Check.That(val.IsBool).IsTrue();
            Check.That(val.GetValue<bool>()).IsTrue();
        }
    }
}