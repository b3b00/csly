using System;
using System.Collections.Generic;
using sly.lexer;
using sly.sourceGenerator.generated.ebnfparser.model;

namespace sly.sourceGenerator.generated.ebnfparser;

public class EbnfRuleParser : BaseParser<EbnfRuleToken, IGrammarNode>
{
    public Match<EbnfRuleToken,IGrammarNode> Root(IList<Token<EbnfRuleToken>> tokens, int position) {
        Func<object[],IGrammarNode> visitor = (object[] args) =>
        {
            return null;
        };
        var parser = Sequence(Value);
        var result = parser(tokens,position);
        if (result.Matched &&  result.Node is SyntaxNode<JsonTokenGeneric,JSon> node) {
            node.LambdaVisitor = visitor;
        }
        return result;
    }
}