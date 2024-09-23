using handExpressions.ebnfparser.model;
using Sprache;

namespace handExpressions.ebnfparser;

public class EbnfParser
{
    private List<string> _tokens;

    private Parser<Rule> _rule;
    public void Initialize()
    {

    Parser<string> _id = from leading in Parse.WhiteSpace.Many()
        from first in Parse.Letter.Once().Text()
        from rest in Parse.LetterOrDigit.Many().Text()
        from trailing in Parse.WhiteSpace.Many()
        select first + rest;

    Parser<string> _discard = from leading in Parse.WhiteSpace.Many()
        from open in Parse.Char('[').Once().Text()
        from d in Parse.Char('d').Once().Text()
        from close in Parse.Char(']').Once().Text()
        from trailing in Parse.WhiteSpace.Many()
        select "discard";
    
    Parser<string> _colon =
        from leading in Parse.WhiteSpace.Many()
        from first in Parse.Char(':').Once().Text()
        from trailing in Parse.WhiteSpace.Many()
        select ":";

    Parser<string> _lParen =
        from leading in Parse.WhiteSpace.Many()
        from first in Parse.Char('(').Once().Text()
        from trailing in Parse.WhiteSpace.Many()
        select "(";

    Parser<string> _rParen =
        from leading in Parse.WhiteSpace.Many()
        from first in Parse.Char(')').Once().Text()
        from trailing in Parse.WhiteSpace.Many()
        select ")";

    Parser<string> _question =
        from leading in Parse.WhiteSpace.Many()
        from first in Parse.Char('?').Once().Text()
        from trailing in Parse.WhiteSpace.Many()
        select "?";

    Parser<string> _star =
        from leading in Parse.WhiteSpace.Many()
        from first in Parse.Char('*').Once().Text()
        from trailing in Parse.WhiteSpace.Many()
        select "*";

    Parser<string> _plus =
        from leading in Parse.WhiteSpace.Many()
        from first in Parse.Char('+').Once().Text()
        from trailing in Parse.WhiteSpace.Many()
        select "+";


    Func<string, IOption<string>, IClause> BuildterminalOrNonTerminal = (string id, IOption<string> discard) =>
    {
        if (_tokens.Contains(id))
        {
            var term = new TerminalClause(id);
            term.IsDiscared = discard.IsDefined;
            return term;
        }

        return new NonTerminalClause(id);
    };

    Parser<IClause> _simpleClause = 
        from id in _id
        from discard in _discard.Optional()
        select BuildterminalOrNonTerminal(id, discard); 

    Parser<IClause> _groupClause =
        from open in _lParen
        from clauses in _simpleClause.Many()
        from close in _rParen
        select new GroupClause(clauses.ToList());

    Parser<IClause> _repeatableClause = _simpleClause.Or(_groupClause);

    Parser<IClause> _optionClause =
        from clause in _repeatableClause
        from question in _question
        select new OptionalClause(clause);

    Parser<IClause> _oneOrMore =
        from clause in _repeatableClause
        from plus in _plus
        select new OneOrMoreClause(clause);

    Parser<IClause> _zeroOrMore =
        from clause in _repeatableClause
        from plus in _star
        select new ZeroOrMoreClause(clause);

    Parser<IClause> _clause =
        (_optionClause).Or(_oneOrMore).Or(_zeroOrMore).Or(_simpleClause).Or(_groupClause);

    _rule =
        from id in _id
        from colon in _colon
        from clauses in _clause.Many()
        select new Rule(id, clauses.ToList());

}

public EbnfParser(List<string> tokens) {
        this._tokens = tokens;
        Initialize();
        }
    
    
    public Rule ParseRule(string definition) 
    {
        var rule = _rule.Parse(definition);
        rule.RuleDefinition = definition;
        return rule;
    } 
}