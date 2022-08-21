using jsonparser.JsonModel;
using NFluent;
using NFluent.Extensibility;
using Xunit;

namespace ParserTests
{
    public static class JSonNFluentExtensions
    {
        
        public static ICheckLink<ICheck<JObject>> HasProperty(this ICheck<JObject> context, string key, string expectedValue)
        {

            ExtensibilityHelper.BeginCheck(context)
                .FailWhen(sut => !sut.ContainsKey(key), " does not contains key {key}")
                .FailWhen(sut => !(sut[key] is JValue), "attribute {key} is not a value")
                .FailWhen(sut => sut[key] is JValue val && !val.IsString, "attribute {key} is not a string value")
                .FailWhen(sut => sut[key] is JValue val && val.IsString && val.GetValue<string>() != expectedValue,
                    "attribute {key} has not expected value {expectedValue}")
                .DefineExpectedValue($"{key}:{expectedValue}")
                .OnNegate("The {checked} contains the {expected} whereas it should not.")
                .EndCheck();
            return ExtensibilityHelper.BuildCheckLink(context);
        }

        public static ICheckLink<ICheck<JObject>> HasProperty(this ICheck<JObject> context, string key, int expectedValue)
        {

            ExtensibilityHelper.BeginCheck(context)
                .FailWhen(sut => !sut.ContainsKey(key), " does not contains key {key}")
                .FailWhen(sut => !(sut[key] is JValue), "attribute {key} is not a value")
                .FailWhen(sut => sut[key] is JValue val && !val.IsInt, "attribute {key} is not a string value")
                .FailWhen(sut => sut[key] is JValue val && val.IsString && val.GetValue<int>() != expectedValue,
                    "attribute {key} has not expected value {expectedValue}")
                .DefineExpectedValue($"{key}:{expectedValue}")
                .OnNegate("The {checked} contains the {expected} whereas it should not.")
                .EndCheck();
            return ExtensibilityHelper.BuildCheckLink(context);
        }
        
        
        public static ICheckLink<ICheck<JList>> HasItem(this ICheck<JList> context, int index, int expectedValue)
        {

            ExtensibilityHelper.BeginCheck(context)
                .FailWhen(sut => index < 0 && index >= sut.Count,$"{index} is out of range")
                .FailWhen(sut => !sut[index].IsValue, $" item at {index} is not a value")
                .FailWhen(sut => sut[index].IsValue && !(sut[index] is JValue),  $" item at {index} is not a value")
                .FailWhen(sut => sut[index] is JValue val && val.IsInt && val.GetValue<int>() != expectedValue, "attribute {key} has not expected value {expectedValue}")
                .OnNegate("The {checked} contains the {expected} whereas it should not.")
                .DefineExpectedValue("list[{index}]:{expectedValue}")
                .EndCheck();
            return ExtensibilityHelper.BuildCheckLink(context);
        }


        public static void CheckInt(JList list, int index, int value)
        {
            Assert.True(list[index].IsValue);
            var val = (JValue) list[index];
            Check.That(val.IsInt).IsTrue();
            Check.That(val.GetValue<int>()).IsEqualTo(value);
        }
    }
}