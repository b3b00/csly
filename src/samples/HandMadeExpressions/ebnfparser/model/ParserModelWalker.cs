using sly.parser.syntax.grammar;

namespace handExpressions.ebnfparser.model;

public class ParserModelWalker<TOutput>
{
    
    private readonly IParserModelVisitor<TOutput> _visitor;
    public ParserModelWalker(IParserModelVisitor<TOutput> visitor)
    {
        _visitor = visitor;
    }

    public TOutput Visit(IGrammarNode node)
    {
        var value = node switch
        {
            TerminalClause terminal => WalkTerminal(terminal),
            NonTerminalClause nonTerminal => WalkNonTerminal(nonTerminal),
            OptionalClause option => WalkOption(option),
            ZeroOrMoreClause zeroOrMore => WalkZeroOrMore(zeroOrMore),
            OneOrMoreClause oneOrMore => WalkOneOrMore(oneOrMore),
            GroupClause groupClause => WalkGroup(groupClause),
            AlternateClause alternateClause => WalkAlternate(alternateClause),
            Rule rule => WalkRule(rule),
            _ => default(TOutput),
        };
        return value;
    }

    private TOutput WalkTerminal(TerminalClause terminal)
    {
        return _visitor.VisitTerminal(terminal);
    }

    private TOutput WalkNonTerminal(NonTerminalClause nonTerminal)
    {
        return _visitor.VisitNonTerminal(nonTerminal);
    }

    private TOutput WalkOption(OptionalClause option)
    {
        var clause = Visit(option.Clause); 
        return _visitor.VisitOption(option, clause);
    }

    private TOutput WalkZeroOrMore(ZeroOrMoreClause zeroOrMore)
    {
        var clause = Visit(zeroOrMore.Clause); 
        return _visitor.VisitZeroOrMore(zeroOrMore, clause);
    }
    
    private TOutput WalkOneOrMore(OneOrMoreClause oneOrMore)
    {
        var clause = Visit(oneOrMore.Clause); 
        return _visitor.VisitOneOrMore(oneOrMore, clause);
    }

    private TOutput WalkGroup(GroupClause group)
    {
        var clauses = group.Clauses.Select(x => Visit(x)).ToList();
        return _visitor.VisitGroup(group, clauses);
    }
    
    private TOutput WalkRule(Rule rule)
    {
        var clauses = rule.Clauses.Select(x => Visit(x)).ToList();
        return _visitor.VisitRule(rule, clauses);
    }
    
    private TOutput WalkAlternate(AlternateClause alternate)
    {
        var clauses = alternate.Choices.Select(x => Visit(x)).ToList();
        return _visitor.VisitAlternate(alternate, clauses);
    }
}