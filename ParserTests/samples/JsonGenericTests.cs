using jsonparser;
using jsonparser.JsonModel;
using sly.parser;
using sly.parser.generator;
using Xunit;

namespace ParserTests.samples
{
    public class JsonGenericTests
    {
        public JsonGenericTests()
        {
            var jsonParser = new EbnfJsonGenericParser();
            var builder = new ParserBuilder<JsonTokenGeneric, JSon>();
            Parser = builder.BuildParser(jsonParser, ParserType.EBNF_LL_RECURSIVE_DESCENT, "root").Result;
        }

        private static Parser<JsonTokenGeneric, JSon> Parser;


        private void AssertString(JObject obj, string key, string value)
        {
            Assert.True(obj.ContainsKey(key));
            Assert.True(obj[key].IsValue);
            var val = (JValue) obj[key];
            Assert.True(val.IsString);
            Assert.Equal(value, val.GetValue<string>());
        }

        private void AssertInt(JObject obj, string key, int value)
        {
            Assert.True(obj.ContainsKey(key));
            Assert.True(obj[key].IsValue);
            var val = (JValue) obj[key];
            Assert.True(val.IsInt);
            Assert.Equal(value, val.GetValue<int>());
        }


        private void AssertDouble(JObject obj, string key, double value)
        {
            Assert.True(obj.ContainsKey(key));
            Assert.True(obj[key].IsValue);
            var val = (JValue) obj[key];
            Assert.True(val.IsDouble);
            Assert.Equal(value, val.GetValue<double>());
        }


        private void AssertString(JList list, int index, string value)
        {
            Assert.True(list[index].IsValue);
            var val = (JValue) list[index];
            Assert.True(val.IsString);
            Assert.Equal(value, val.GetValue<string>());
        }

        private void AssertInt(JList list, int index, int value)
        {
            Assert.True(list[index].IsValue);
            var val = (JValue) list[index];
            Assert.True(val.IsInt);
            Assert.Equal(value, val.GetValue<int>());
        }


        private void AssertDouble(JList list, int index, double value)
        {
            Assert.True(list[index].IsValue);
            var val = (JValue) list[index];
            Assert.True(val.IsDouble);
            Assert.Equal(value, val.GetValue<double>());
        }


        private void AssertBool(JList list, int index, bool value)
        {
            Assert.True(list[index].IsValue);
            var val = (JValue) list[index];
            Assert.True(val.IsBool);
            Assert.Equal(value, val.GetValue<bool>());
        }


        private void AssertObject(JList list, int index, int count)
        {
            Assert.True(list[index].IsObject);
            var val = (JObject) list[index];
            Assert.Equal(count, val.Count);
        }

        [Fact]
        public void TestDoubleValue()
        {
            var r = Parser.Parse("0.1");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.True(r.Result.IsValue);
            Assert.True(((JValue) r.Result).IsDouble);
            Assert.Equal(0.1d, ((JValue) r.Result).GetValue<double>());
        }


        [Fact]
        public void TestEmptyListValue()
        {
            var r = Parser.Parse("[]");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.True(r.Result.IsList);
            Assert.Equal(0, ((JList) r.Result).Count);
        }

        [Fact]
        public void TestEmptyObjectValue()
        {
            var r = Parser.Parse("{}");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.True(r.Result.IsObject);
            Assert.Equal(0, ((JObject) r.Result).Count);
        }

        [Fact]
        public void TestFalseBooleanValue()
        {
            var r = Parser.Parse("false");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.True(r.Result.IsValue);
            var val = (JValue) r.Result;
            Assert.True(val.IsBool);
            Assert.False(val.GetValue<bool>());
        }

        [Fact]
        public void TestIntValue()
        {
            var r = Parser.Parse("1");
            Assert.False(r.IsError);
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.True(r.Result.IsValue);
            Assert.True(((JValue) r.Result).IsInt);
            Assert.Equal(1, ((JValue) r.Result).GetValue<int>());
        }

        [Fact]
        public void TestManyListValue()
        {
            var r = Parser.Parse("[1,2]");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.True(r.Result.IsList);
            var list = (JList) r.Result;
            Assert.Equal(2, list.Count);
            AssertInt(list, 0, 1);
            AssertInt(list, 1, 2);
        }

        [Fact]
        public void TestManyMixedListValue()
        {
            var r = Parser.Parse("[1,null,{},true,42.58]");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.NotNull(r.Result);
            object val = r.Result;
            Assert.True(r.Result.IsList);
            var list = (JList) r.Result;
            Assert.Equal(5, ((JList) r.Result).Count);
            AssertInt(list, 0, 1);
            Assert.True(((JList) r.Result)[1].IsNull);
            AssertObject(list, 2, 0);
            AssertBool(list, 3, true);
            AssertDouble(list, 4, 42.58d);
        }

        [Fact]
        public void TestManyNestedPropertyObjectValue()
        {
            var json = "{\"p1\":\"v1\",\"p2\":\"v2\",\"p3\":{\"inner1\":1}}";

            var r = Parser.Parse(json);
            Assert.False(r.IsError);
            Assert.NotNull(r);
            Assert.True(r.Result.IsObject);
            var values = (JObject) r.Result;
            Assert.Equal(3, values.Count);
            AssertString(values, "p1", "v1");
            AssertString(values, "p2", "v2");

            Assert.True(values.ContainsKey("p3"));
            var inner = values["p3"];
            Assert.True(inner.IsObject);
            var innerObj = (JObject) inner;

            Assert.Equal(1, innerObj.Count);
            AssertInt(innerObj, "inner1", 1);
        }

        [Fact]
        public void TestManyPropertyObjectValue()
        {
            var json = "{\"p1\":\"v1\",\"p2\":\"v2\"}";
            json = "{\"p1\":\"v1\" , \"p2\":\"v2\" }";
            var r = Parser.Parse(json);
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.True(r.Result.IsObject);
            var values = (JObject) r.Result;
            Assert.Equal(2, values.Count);
            AssertString(values, "p1", "v1");
            AssertString(values, "p2", "v2");
        }

        [Fact]
        public void TestNullValue()
        {
            var r = Parser.Parse("null");
            Assert.False(r.IsError);
            Assert.True(r.Result.IsNull);
        }

        [Fact]
        public void TestSingleListValue()
        {
            var r = Parser.Parse("[1]");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.True(r.Result.IsList);
            var list = (JList) r.Result;
            Assert.Equal(1, list.Count);
            AssertInt(list, 0, 1);
        }


        [Fact]
        public void TestSinglePropertyObjectValue()
        {
            var r = Parser.Parse("{\"prop\":\"value\"}");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.True(r.Result.IsObject);
            var values = (JObject) r.Result;
            Assert.Equal(1, values.Count);
            AssertString(values, "prop", "value");
        }

        [Fact]
        public void TestStringValue()
        {
            var val = "hello";
            var r = Parser.Parse("\"" + val + "\"");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.True(r.Result.IsValue);
            Assert.True(((JValue) r.Result).IsString);
            Assert.Equal(val, ((JValue) r.Result).GetValue<string>());
        }

        [Fact]
        public void TestTrueBooleanValue()
        {
            var r = Parser.Parse("true");
            Assert.False(r.IsError);
            Assert.NotNull(r.Result);
            Assert.True(r.Result.IsValue);
            var val = (JValue) r.Result;
            Assert.True(val.IsBool);
            Assert.True(val.IsBool);
            Assert.True(val.GetValue<bool>());
        }
    }
}