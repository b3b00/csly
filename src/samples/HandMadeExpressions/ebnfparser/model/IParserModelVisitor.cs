namespace handExpressions.ebnfparser.model;

public interface IParserModelVisitor<TOutput>
{
    TOutput VisitTerminal(TerminalClause terminal);

    TOutput VisitNonTerminal(NonTerminalClause nonTerminal);

    TOutput VisitOption(OptionalClause option, TOutput clause);

    TOutput VisitZeroOrMore(ZeroOrMoreClause zeroOrMore, TOutput clause);

    TOutput VisitOneOrMore(OneOrMoreClause oneOrMore, TOutput clause);

    TOutput VisitGroup(GroupClause group, IList<TOutput> clauses);
    
    TOutput VisitRule(Rule rule, IList<TOutput> clauses);
    
    TOutput VisitAlternate(AlternateClause alternate, IList<TOutput> clauses);
}