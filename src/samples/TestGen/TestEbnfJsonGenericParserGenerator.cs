using handExpressions;
using handExpressions.sourceGenerator;
using sly.lexer;
using testgen.JsonModel;

namespace testgen;


[CslyParser]
public partial class TestEbnfJsonGenericParserGenerator : AbstractCslyParser<TestJsonTokenGeneric,TestEbnfJsonGenericParser, JSon> {
    public TestEbnfJsonGenericParserGenerator(TestEbnfJsonGenericParser instance) : base(instance)
    {
    }

}