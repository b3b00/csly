using NFluent;
using SimpleTemplate;
using SimpleTemplate.model;
using sly.parser;
using sly.parser.generator;
using Xunit;
using rules;
using rules.model;

namespace ParserTests.samples;

public class RulesTests
{
    
    public Parser<RulesToken, IRule> GetParser(ParserType parserType)
    {
        var builder = new ParserBuilder<RulesToken, IRule>();
        var instance = new RulesParser();
        var build = builder.BuildParser(instance, parserType, "rule");
        Check.That(build.IsError).IsFalse();
        Check.That(build.Result).IsNotNull();
        return build.Result;
    }
    
    [Fact]
    public void test()
    {
        var parser = GetParser(ParserType.EBNF_LL_RECURSIVE_DESCENT);
        var source = @"
// rule's comment
REGLE #1 
	SI in1 == VIDE
		ET ( in2 == ""27""
		ET in3 == VIDE )
		ET in4 DANS {VIDE, ""01"", ""2000"" }
		ET in5 DANS {VIDE, ""01"", ""99"" }
		ET in6 DANS {VIDE, ""01"", ""99"" }
		ET in7 <> ""25""
		ET in8 <> ""333""
	ALORS
		// some insightful comment
		// even more relevant 
		out1 = 0
		out2 = 0
		out3 = 0
		out4 = 0
// another comment'
		out5 = in9 * in10
	FINSI
FIN REGLE
";
        var parsed = parser.Parse(source);
        Check.That(parsed).IsNotNull();
        Check.That(parsed).IsOkParsing();
        var rule = parsed.Result as Rule;
        Check.That(rule).IsNotNull();
        Check.That(rule.PrecedingComments).CountIs(1);
        Check.That(rule.PrecedingComments[0].Text.Trim()).IsEqualTo("rule's comment");
        
        var firstAssign = rule.Action.Stats[0];
        Check.That(firstAssign).IsNotNull();
        Check.That(firstAssign.PrecedingComments).CountIs<RuleComment>(2);
        var comments =  firstAssign.PrecedingComments;
        Check.That(comments[0].Text.Trim()).IsEqualTo("some insightful comment");
        Check.That(comments[1].Text.Trim()).IsEqualTo("even more relevant");

    }
}