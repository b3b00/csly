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
        
        
        public static ICheckLink<ICheck<JObject>> HasObjectProperty(this ICheck<JObject> context, string key)
        {

            ExtensibilityHelper.BeginCheck(context)
                .FailWhen(sut => !sut.ContainsKey(key), " does not contains key {expected}")
                .FailWhen(sut => !(sut[key] is JObject), "attribute {expected} is not an object")
                .DefineExpectedValue($"{key}")
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
        
        public static ICheckLink<ICheck<JList>> HasItem(this ICheck<JList> context, int index, double expectedValue)
        {

            ExtensibilityHelper.BeginCheck(context)
                .FailWhen(sut => index < 0 && index >= sut.Count,$"{index} is out of range")
                .FailWhen(sut => !sut[index].IsValue, $" item at {index} is not a value")
                .FailWhen(sut => sut[index].IsValue && !(sut[index] is JValue),  $" item at {index} is not a value")
                .FailWhen(sut => sut[index] is JValue val && val.IsDouble && val.GetValue<double>() != expectedValue, "attribute {key} has not expected value {expectedValue}")
                .OnNegate("The {checked} contains the {expected} whereas it should not.")
                .DefineExpectedValue("list[{index}]:{expectedValue}")
                .EndCheck();
            return ExtensibilityHelper.BuildCheckLink(context);
        }

        
        public static ICheckLink<ICheck<JList>> HasItem(this ICheck<JList> context, int index, bool expectedValue)
        {

            ExtensibilityHelper.BeginCheck(context)
                .FailWhen(sut => index < 0 && index >= sut.Count,$"{index} is out of range")
                .FailWhen(sut => !sut[index].IsValue, $" item at {index} is not a value")
                .FailWhen(sut => sut[index].IsValue && !(sut[index] is JValue),  $" item at {index} is not a value")
                .FailWhen(sut => sut[index] is JValue val && val.IsBool && val.GetValue<bool>() != expectedValue, "attribute {key} has not expected value {expectedValue}")
                .OnNegate("The {checked} contains the {expected} whereas it should not.")
                .DefineExpectedValue("list[{index}]:{expectedValue}")
                .EndCheck();
            return ExtensibilityHelper.BuildCheckLink(context);
        }

        public static ICheckLink<ICheck<JList>> HasObjectItem(this ICheck<JList> context, int index, int expectedPropertycount)
        {

            ExtensibilityHelper.BeginCheck(context)
                .FailWhen(sut => index < 0 && index >= sut.Count,$"{index} is out of range")
                .FailWhen(sut => !sut[index].IsObject, $" item at {index} is not an object")
                .FailWhen(sut => sut[index].IsObject && !(sut[index] is JObject),  $" item at {index} is not a object")
                .FailWhen(sut => sut[index] is JObject o && o.Count != expectedPropertycount, "attribute {key} has not expected property count {expectedValue}")
                .OnNegate("The {checked} contains the {expected} whereas it should not.")
                .EndCheck();
            return ExtensibilityHelper.BuildCheckLink(context);
        }
        
        public static ICheckLink<ICheck<JList>> CountIs(this ICheck<JList> context, int expectedCount)
        {

            ExtensibilityHelper.BeginCheck(context)
                .FailWhen(sut => sut.Count != expectedCount,$"list dos not have expected count : {expectedCount}")
                .OnNegate("The {checked} contains the {expected} count.")
                .EndCheck();
            return ExtensibilityHelper.BuildCheckLink(context);
        }
        
        public static ICheckLink<ICheck<JObject>> CountIs(this ICheck<JObject> context, int expectedCount)
        {

            ExtensibilityHelper.BeginCheck(context)
                .FailWhen(sut => sut.Count != expectedCount,$"list dos not have expected count : {expectedCount}")
                .OnNegate("The {checked} contains the {expected} count.")
                .EndCheck();
            return ExtensibilityHelper.BuildCheckLink(context);
        }
        
       
        
        public static ICheckLink<ICheck<JList>> IsEmpty(this ICheck<JList> context)
        {

            ExtensibilityHelper.BeginCheck(context)
                .FailWhen(sut => sut.Count > 0,"The {checked} is not empty.")
                .OnNegate("The {checked} is empty.")
                .EndCheck();
            return ExtensibilityHelper.BuildCheckLink(context);
        }
        
        public static ICheckLink<ICheck<JObject>> IsEmpty(this ICheck<JObject> context)
        {

            ExtensibilityHelper.BeginCheck(context)
                .FailWhen(sut => sut.Count > 0,"The {checked} is not empty.")
                .OnNegate("The {checked} is empty.")
                .EndCheck();
            return ExtensibilityHelper.BuildCheckLink(context);
        }
    }
}