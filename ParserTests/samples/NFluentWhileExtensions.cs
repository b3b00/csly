using csly.whileLang.interpreter;
using csly.whileLang.model;
using NFluent;
using NFluent.Extensibility;

namespace ParserTests.samples
{
    public static class NFluentWhileExtensions
    {
        public static ICheckLink<ICheck<InterpreterContext>> HasVariableWithValue(this ICheck<InterpreterContext> context, string variableName, int expectedValue) 
        {
            ExtensibilityHelper.BeginCheck(context)
                .FailWhen(sut => sut.GetVariable(variableName) == null, "expecting {expected} but variable not found.")
                .FailWhen(sut => sut.GetVariable(variableName).IntValue != expectedValue, "expecting {expected} found {checked}.")
                .OnNegate("variable is ok")
                .DefineExpectedValue($"{variableName}={expectedValue}")
                .EndCheck();
            return ExtensibilityHelper.BuildCheckLink(context);
        }
        
        public static ICheckLink<ICheck<SequenceStatement>> CountIs(this ICheck<SequenceStatement> context, int expectedCount) 
        {
            ExtensibilityHelper.BeginCheck(context)
                .FailWhen(sut => sut.Count != expectedCount, "expecting {expected} but {checked}.")
                .OnNegate("length is ok")
                .DefineExpectedValue($"expected count is {expectedCount}")
                .EndCheck();
            return ExtensibilityHelper.BuildCheckLink(context);
        }
    }
}