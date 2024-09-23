using handExpressions.ebnfparser.model;
using Sprache;

namespace handExpressions.ebnfparser;

public class EbnfParser
{
    private static Parser<string> _id = from leading  in Parse.WhiteSpace.Many()
    from first in Parse.Letter.Once().Text()
    from rest in Parse.LetterOrDigit.Many().Text()
    from trailing in Parse.WhiteSpace.Many()
    select first+rest;
    
   private static Parser<string> _colon = 
     from leading  in Parse.WhiteSpace.Many()
        from first in Parse.Char(':').Once().Text()
        from trailing in Parse.WhiteSpace.Many()
    select ":";
    
    private static Parser<string> _lParen = 
         from leading  in Parse.WhiteSpace.Many()
            from first in Parse.Char('(').Once().Text()
            from trailing in Parse.WhiteSpace.Many()
        select "(";
    
    private static Parser<string> _rParen = 
             from leading  in Parse.WhiteSpace.Many()
                from first in Parse.Char(')').Once().Text()
                from trailing in Parse.WhiteSpace.Many()
            select ")";
    
    private static Parser<string> _question = 
             from leading  in Parse.WhiteSpace.Many()
                from first in Parse.Char('?').Once().Text()
                from trailing in Parse.WhiteSpace.Many()
            select "?";
    
        private static Parser<string> _star = 
                 from leading  in Parse.WhiteSpace.Many()
                    from first in Parse.Char('*').Once().Text()
                    from trailing in Parse.WhiteSpace.Many()
                select "*";
        
            private static Parser<string> _plus = 
                     from leading  in Parse.WhiteSpace.Many()
                        from first in Parse.Char('+').Once().Text()
                        from trailing in Parse.WhiteSpace.Many()
                    select "+";
    
    
    
    private static Parser<IClause> _simpleClause = from id in _id select new TerminalClause(id); // TODO
    
    private static Parser<IClause> _groupClause = 
        from open in _lParen
        from clauses in _simpleClause.Many()
        from close in _rParen
        select new GroupClause(clauses.ToList());
    
    private static Parser<IClause> _repeatableClause = _simpleClause.Or(_groupClause);
    
    private static Parser<IClause> _optionClause = 
        from clause in _repeatableClause
        from question in _question
        select new OptionalClause(clause);
    
    private static Parser<IClause> _oneOrMore = 
            from clause in _repeatableClause
            from plus in _plus
            select new OneOrMoreClause(clause);
    
    private static Parser<IClause> _zeroOrMore = 
                from clause in _repeatableClause
                from plus in _star
                select new ZeroOrMoreClause(clause);
    
    private static Parser<IClause> _clause = (_optionClause).Or(_oneOrMore).Or(_zeroOrMore).Or(_simpleClause).Or(_groupClause);
    
    private static Parser<Rule> _rule = 
        from id in _id
        from colon in _colon
        from clauses in _clause.Many()
        select new Rule(id,clauses.ToList());
    
    public EbnfParser() {
        }
    
    
    public object ParseRule(string definition) 
    {
        var rule = _rule.Parse(definition);
        return rule;
    } 
}