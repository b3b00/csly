using aot.parser;
using sly.buildresult;
using sly.lexer;
using sly.parser.syntax.grammar;

namespace sly.parser.generator;

public class AotRuleParser<IN, OUT> where IN : struct
{
    public IAotLexerBuilder<EbnfTokenGeneric> GetEbnfLexerBuilder()
    {
        var builder = AotLexerBuilder<EbnfTokenGeneric>.NewBuilder();
        builder
            .AlphaNumDashId(EbnfTokenGeneric.IDENTIFIER)
            .Sugar(EbnfTokenGeneric.COLON, ":")
            .Sugar(EbnfTokenGeneric.ZEROORMORE, "*")
            .Sugar(EbnfTokenGeneric.ONEORMORE, "+")
            .Sugar(EbnfTokenGeneric.OPTION, "?")
            .Sugar(EbnfTokenGeneric.DISCARD, "[d]")
            .Sugar(EbnfTokenGeneric.LPAREN, "(")
            .Sugar(EbnfTokenGeneric.RPAREN, ")")
            .Sugar(EbnfTokenGeneric.OR, "|")
            .Sugar(EbnfTokenGeneric.LCROG, "[")
            .Sugar(EbnfTokenGeneric.RCROG, "]")
            .String(EbnfTokenGeneric.STRING, "'", "\\");
        return builder;
    }

    public BuildResult<Parser<EbnfTokenGeneric, GrammarNode<IN, OUT>>> BuildParser(string i18N)
    {
        RuleParser<IN, OUT> instance = new RuleParser<IN, OUT>();
        IAotParserBuilder<EbnfTokenGeneric, GrammarNode<IN, OUT>> builder =
            AotParserBuilder<EbnfTokenGeneric, GrammarNode<IN, OUT>>.NewBuilder(
                instance, "rule", i18N);

        builder = builder.Production("rule : IDENTIFIER COLON clauses", (args =>
            {
                return instance.Root((Token<EbnfTokenGeneric>)args[0], (Token<EbnfTokenGeneric>)args[1],
                    (ClauseSequence<IN, OUT>)args[2]);
            }))
            .Production("clauses : clause clauses",
                (args => { return instance.Clauses((IClause<IN, OUT>)args[0], (ClauseSequence<IN, OUT>)args[1]); }))
            .Production("clauses : clause", (args => { return instance.SingleClause((IClause<IN, OUT>)args[0]); }))
            .Production("clause : IDENTIFIER ZEROORMORE",
                (args =>
                {
                    return instance.ZeroMoreClause((Token<EbnfTokenGeneric>)args[0],
                        (Token<EbnfTokenGeneric>)args[1]);
                }))
            .Production("clause : IDENTIFIER ONEORMORE",
                (args =>
                {
                    return instance.OneMoreClause((Token<EbnfTokenGeneric>)args[0],
                        (Token<EbnfTokenGeneric>)args[1]);
                }))
            .Production("clause : IDENTIFIER OPTION",
                (args =>
                {
                    return instance.OptionClause((Token<EbnfTokenGeneric>)args[0],
                        (Token<EbnfTokenGeneric>)args[1]);
                }))
            .Production("clause : IDENTIFIER ",
                (args => { return instance.SimpleClause((Token<EbnfTokenGeneric>)args[0]); }))
            .Production("clause : STRING ",
                (args => { return instance.ExplicitTokenClause((Token<EbnfTokenGeneric>)args[0]); }))
            .Production("clause : IDENTIFIER DISCARD", (args =>
            {
                return instance.SimpleDiscardedClause((Token<EbnfTokenGeneric>)args[0],
                    (Token<EbnfTokenGeneric>)args[1]);
            }))
            .Production("clause : STRING DISCARD", (args =>
            {
                return instance.ExplicitTokenClauseDiscarded((Token<EbnfTokenGeneric>)args[0],
                    (Token<EbnfTokenGeneric>)args[1]);
            }))
            .Production("clause : choiceclause",
                (args => { return instance.AlternateClause((ChoiceClause<IN, OUT>)args[0]); }))
            .Production("choiceclause : LCROG choices RCROG", (args =>
            {
                return instance.AlternateChoices((Token<EbnfTokenGeneric>)args[0], (IClause<IN, OUT>)args[1],
                    (Token<EbnfTokenGeneric>)args[2]);
            }))
            .Production("choices : IDENTIFIER",
                (args => { return instance.ChoicesOne((Token<EbnfTokenGeneric>)args[0]); }))
            .Production("choices : STRING",
                (args => { return instance.ChoicesString((Token<EbnfTokenGeneric>)args[0]); }))
            .Production("choices : IDENTIFIER OR choices", (args =>
            {
                return instance.ChoicesMany((Token<EbnfTokenGeneric>)args[0], (Token<EbnfTokenGeneric>)args[1],
                    (ChoiceClause<IN, OUT>)args[2]);
            }))
            .Production("choices : STRING OR choices", (args =>
            {
                return instance.ChoicesManyExplicit((Token<EbnfTokenGeneric>)args[0], (Token<EbnfTokenGeneric>)args[1],
                    (ChoiceClause<IN, OUT>)args[2]);
            }))
            .Production("clause : LPAREN groupclauses RPAREN ONEORMORE", (args =>
            {
                return instance.GroupOneOrMore((Token<EbnfTokenGeneric>)args[0], (GroupClause<IN, OUT>)args[1],
                    (Token<EbnfTokenGeneric>)args[2], (Token<EbnfTokenGeneric>)args[3]);
            }))
            .Production("clause : LPAREN groupclauses RPAREN ZEROORMORE", (args =>
            {
                return instance.GroupZeroOrMore((Token<EbnfTokenGeneric>)args[0], (GroupClause<IN, OUT>)args[1],
                    (Token<EbnfTokenGeneric>)args[2], (Token<EbnfTokenGeneric>)args[3]);
            }))
            .Production("clause : LPAREN groupclauses RPAREN OPTION", (args =>
            {
                return instance.GroupOptional((Token<EbnfTokenGeneric>)args[0], (GroupClause<IN, OUT>)args[1],
                    (Token<EbnfTokenGeneric>)args[2], (Token<EbnfTokenGeneric>)args[3]);
            }))
            .Production("clause : LPAREN groupclauses RPAREN", (args =>
            {
                return instance.Group((Token<EbnfTokenGeneric>)args[0], (GroupClause<IN, OUT>)args[1],
                    (Token<EbnfTokenGeneric>)args[2]);
            }))
            .Production("groupclause : IDENTIFIER",
                (args => { return instance.GroupClause((Token<EbnfTokenGeneric>)args[0]); }))
            .Production("groupclause : STRING",
                (args => { return instance.GroupClauseExplicit((Token<EbnfTokenGeneric>)args[0]); }))
            .Production("groupclause : IDENTIFIER DISCARD", (args =>
            {
                return instance.GroupClauseDiscarded((Token<EbnfTokenGeneric>)args[0],
                    (Token<EbnfTokenGeneric>)args[1]);
            }))
            .Production("groupclause : STRING DISCARD", (args =>
            {
                return instance.GroupClauseExplicitDiscarded((Token<EbnfTokenGeneric>)args[0],
                    (Token<EbnfTokenGeneric>)args[1]);
            }))
            .Production("groupclause : choiceclause",
                (args => { return instance.GroupChoiceClause((ChoiceClause<IN, OUT>)args[0]); }))
            .Production("groupclauses : groupclause",
                (args => { return instance.GroupClausesOne((GroupClause<IN, OUT>)args[0]); }))
            .Production("groupclauses : groupclause groupclauses",
                (args =>
                {
                    return instance.GroupClauses((GroupClause<IN, OUT>)args[0], (GroupClause<IN, OUT>)args[1]);
                }))
            .Production("clause : choiceclause DISCARD", args =>
            {
                return instance.AlternateDiscardedClause((ChoiceClause<IN, OUT>)args[0],
                    (Token<EbnfTokenGeneric>)args[1]);
            })
            .Production("clause : choiceclause ONEORMORE ",
                args =>
                {
                    return instance.ChoiceOneOrMore((ChoiceClause<IN, OUT>)args[0], (Token<EbnfTokenGeneric>)args[1]);
                })
            .Production("clause : choiceclause ZEROORMORE ",
                args =>
                {
                    return instance.ChoiceZeroOrMore((ChoiceClause<IN, OUT>)args[0], (Token<EbnfTokenGeneric>)args[1]);
                })
            .Production("clause : choiceclause OPTION ",
                args =>
                {
                    return instance.ChoiceOptional((ChoiceClause<IN, OUT>)args[0], (Token<EbnfTokenGeneric>)args[1]);
                })
            .WithLexerbuilder(GetEbnfLexerBuilder());

        var parser = builder.BuildParser();
        return parser;
    }

}