using handExpressions.ebnfparser.model;
using Microsoft.Diagnostics.Tracing.Parsers.IIS_Trace;
using Pidgin;
using static Pidgin.Parser;
using static Pidgin.Parser<char>;

namespace handExpressions.ebnfparser;

public class EbnfParser
{
    private static readonly Parser<char, char> _colon = Char(':');
    
    private static readonly Parser<char, char> _star = Char('*');
    
    private static readonly Parser<char, char> _plus = Char('+');
    
    private static readonly Parser<char, char> _question = Char('?');
    
    private static readonly Parser<char, char> _lParen = Char('(');
    
    private static readonly Parser<char, char> _rParen = Char(')');
    
    private static readonly Parser<char, char> _quote = Char('\'');
    
    private static readonly Parser<char, string> _explicit =
        Token(c => c != '\'')
            .ManyString()
            .Between(_quote);

    private static readonly Parser<char, string> _identifier = Token(c => System.Char.IsLetterOrDigit(c))
        .ManyString();

    private static readonly Parser<char, Rule> _rule =
        from id in _identifier
        from colon in _colon
        select new Rule(id, null, new List<IClause>());

    private static readonly Parser<char, IClause> _clause =
        // TODO NT or T according to enum
        from id in _identifier select new TerminalClause(id) as IClause; 

    private static readonly Parser<char, IClause> _zeroOrMore =
        from clause in _clause
        from star in _star 
        select new ZeroOrMoreClause(clause) as IClause;
    
    private static readonly Parser<char, IClause> _oneOrMore =
        from clause in _clause
        from star in _star 
         select new OneOrMoreClause(clause) as IClause;
    
    private static readonly Parser<char, IClause> _option =
        from clause in _clause
        from star in _question 
        select new OneOrMoreClause(clause) as IClause;
    
    private static readonly Parser<char, IClause> _group =
        from open in _lParen
        from clauses in _clause
        from close in _rParen 
        select new OneOrMoreClause(clauses) as IClause;
}