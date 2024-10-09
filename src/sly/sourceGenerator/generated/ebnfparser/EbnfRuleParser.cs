using System;
using System.Collections.Generic;
using System.Linq;
using sly.lexer;
using sly.parser.parser;
using sly.parser.syntax.tree;
using sly.sourceGenerator.generated.ebnfparser.model;

namespace sly.sourceGenerator.generated.ebnfparser;

public class EbnfRuleParser : BaseParser<EbnfRuleToken, IGrammarNode>
{

    private List<string> _tokens;
    
    private EbnfRuleTokenizer _tokenizer;
    public EbnfRuleParser(List<string> tokens)
    {
        _tokens = tokens;
        _tokenizer = new EbnfRuleTokenizer();
    }
    
    
    
    public IClause BuildterminalOrNonTerminal(string id, bool discard = false)
    {
        if (_tokens.Contains(id) || (id.StartsWith("'") && id.EndsWith("'")))
        {
            var term = new TerminalClause(id)
            {
                IsDiscared = discard
            };
            return term;
        }
        
        return new NonTerminalClause(id);
    }

    public Match<EbnfRuleToken,IGrammarNode> Parse(string rule)
    {   
        var tokenized = _tokenizer.Tokenize(rule);
        var result = Root(tokenized,0);
        return result;
    }
    
    public Match<EbnfRuleToken,IGrammarNode> Root(IList<Token<EbnfRuleToken>> tokens, int position) {
       
        Func<object[],IGrammarNode> visitor = (object[] args) =>
        {
            var name = ((Token<EbnfRuleToken>)args[0]).Value;
            var clauses = (List<IGrammarNode>)args[1];
            return new Rule(name, clauses.Cast<IClause>().ToList());
        };
        var parser = Sequence(
            TerminalParser(expectedTokens:EbnfRuleToken.IDENTIFIER),
            DiscardedTerminalParser(expectedTokens:EbnfRuleToken.COLON),
            OneOrMoreValue(Clause)
            );
        var result = parser(tokens,position);
        if (result.Matched &&  result.Node is SyntaxNode<EbnfRuleToken,IGrammarNode> node)
        {
            node.Name = "root";
            node.LambdaVisitor = visitor;
        }
        return result;
    }

    public Match<EbnfRuleToken, IGrammarNode> Clause(IList<Token<EbnfRuleToken>> tokens, int position)
    {
        Func<object[],IGrammarNode> visitor = (object[] args) =>
        {
            return (IClause)args[0];
        };
        var parser = Alternate(Explicit, ClauseOption,ClauseZeroOrMore,ClauseOneOrMore, Simple, Group);
        var result = parser(tokens,position);
        if (result.Matched &&  result.Node is SyntaxNode<EbnfRuleToken,IGrammarNode> node)
        {
            SyntaxNode<EbnfRuleToken, IGrammarNode> clauseNode = new SyntaxNode<EbnfRuleToken, IGrammarNode>("clause");
            clauseNode.Children.Add(node);
            clauseNode.LambdaVisitor = visitor;
            var clauseResult = new Match<EbnfRuleToken, IGrammarNode>();
            clauseResult.Node = clauseNode;
            clauseResult.Matched = true;
            clauseResult.Position = result.Position;
            return clauseResult;
        }
        return new Match<EbnfRuleToken, IGrammarNode>();
    }

    public Match<EbnfRuleToken, IGrammarNode> ClauseZeroOrMore(IList<Token<EbnfRuleToken>> tokens, int position)
    {
        Func<object[],IGrammarNode> visitor = (object[] args) =>
        {
            return new ZeroOrMoreClause((IClause)args[0]);
        };
        var parser = Sequence(Repeatable,TerminalParser(expectedTokens:EbnfRuleToken.ZEROORMORE));
        var result = parser(tokens,position);
        if (result.Matched &&  result.Node is SyntaxNode<EbnfRuleToken,IGrammarNode> node)
        {
            node.Name = "zero or more";
            node.LambdaVisitor = visitor;
        }
        return result;
    }
    
    public Match<EbnfRuleToken, IGrammarNode> ClauseOneOrMore(IList<Token<EbnfRuleToken>> tokens, int position)
    {
        
        Func<object[],IGrammarNode> visitor = (object[] args) =>
        {
            return new OneOrMoreClause((IClause)args[0]);
        };
        var parser = Sequence(Repeatable,TerminalParser(expectedTokens:EbnfRuleToken.ONEORMORE));
        var result = parser(tokens,position);
        if (result.Matched &&  result.Node is SyntaxNode<EbnfRuleToken,IGrammarNode> node) {
            node.Name = "one or more";
            node.LambdaVisitor = visitor;
        }
        return result;
    }
    

    
    public Match<EbnfRuleToken, IGrammarNode> ClauseOption(IList<Token<EbnfRuleToken>> tokens, int position)
    {
        
        Func<object[],IGrammarNode> visitor = (object[] args) =>
        {
            return new OptionalClause((IClause)args[0]);
        };
        var parser = Sequence(Repeatable,TerminalParser(expectedTokens:EbnfRuleToken.OPTION));
        var result = parser(tokens,position);
        if (result.Matched &&  result.Node is SyntaxNode<EbnfRuleToken,IGrammarNode> node)
        {
            node.Name = "optional";
            node.LambdaVisitor = visitor;
        }
        return result;
    }
    
   
    
    
    public Match<EbnfRuleToken, IGrammarNode> Repeatable(IList<Token<EbnfRuleToken>> tokens, int position)
    {
        Func<object[],IGrammarNode> visitor = (object[] args) =>
        {
            return (IGrammarNode)args[0];

        };
        var parser = Alternate(Group, Simple, Explicit);
        var result = parser(tokens,position);
        if (result.Matched &&  result.Node is SyntaxNode<EbnfRuleToken,IGrammarNode> node) {
            
            SyntaxNode<EbnfRuleToken, IGrammarNode> clauseNode = new SyntaxNode<EbnfRuleToken, IGrammarNode>("repeatable");
            clauseNode.Children.Add(node);
            clauseNode.LambdaVisitor = visitor;
            var clauseResult = new Match<EbnfRuleToken, IGrammarNode>();
            clauseResult.Node = clauseNode;
            clauseResult.Matched = true;
            clauseResult.Position = result.Position;
            return clauseResult;
            
        }
        return new Match<EbnfRuleToken, IGrammarNode>();
    }
    
    public Match<EbnfRuleToken, IGrammarNode> Simple(IList<Token<EbnfRuleToken>> tokens, int position)
    {
        Func<object[],IGrammarNode> visitor = (object[] args) =>
        {
            var name = ((Token<EbnfRuleToken>)args[0]).Value;
            bool discard = false;
            if (args[1] is ValueOption<IGrammarNode> option)
            {
                discard = option.IsSome;
            }
            if (args[1] is Token<EbnfRuleToken> token)
            {
                discard = !token.IsEmpty;
            }
            var clause = BuildterminalOrNonTerminal(name, discard);
            return clause;
        };
        var parser = Sequence(TerminalParser(expectedTokens:EbnfRuleToken.IDENTIFIER),Option(TerminalParser(expectedTokens:EbnfRuleToken.DISCARD)));
        var result = parser(tokens,position);
        if (result.Matched &&  result.Node is SyntaxNode<EbnfRuleToken,IGrammarNode> node)
        {
            node.Name = "simple";
            node.LambdaVisitor = visitor;
        }
        return result;
    }
    
    public Match<EbnfRuleToken, IGrammarNode> Explicit(IList<Token<EbnfRuleToken>> tokens, int position)
    {
        Func<object[],IGrammarNode> visitor = (object[] args) =>
        {
            var name = ((Token<EbnfRuleToken>)args[0]).Value;
            bool discard = false;
            if (args[1] is ValueOption<IGrammarNode> option)
            {
                discard = option.IsSome;
            }
            if (args[1] is Token<EbnfRuleToken> token)
            {
                discard = !token.IsEmpty;
            }
            var clause = BuildterminalOrNonTerminal(name, discard);
            return clause;
        };
        var parser = Sequence(TerminalParser(expectedTokens:EbnfRuleToken.STRING),Option(TerminalParser(expectedTokens:EbnfRuleToken.DISCARD)));
        var result = parser(tokens,position);
        if (result.Matched &&  result.Node is SyntaxNode<EbnfRuleToken,IGrammarNode> node)
        {
            node.Name = "explicit";
            node.LambdaVisitor = visitor;
        }
        return result;
    }
    
    public Match<EbnfRuleToken, IGrammarNode> Group(IList<Token<EbnfRuleToken>> tokens, int position)
    {
        Func<object[],IGrammarNode> visitor = (object[] args) =>
        {
            var clauses = (args[0] as List<IGrammarNode>)?.Cast<IClause>().ToList();
            return new GroupClause(clauses);
        };
        var parser = Sequence(
            DiscardedTerminalParser(expectedTokens: EbnfRuleToken.LPAREN),
            OneOrMoreValue(Alternate(Simple, Explicit)),
            DiscardedTerminalParser(expectedTokens: EbnfRuleToken.RPAREN));
            
        var result = parser(tokens,position);
        if (result.Matched &&  result.Node is SyntaxNode<EbnfRuleToken,IGrammarNode> node)
        {
            node.Name = "group";
            node.LambdaVisitor = visitor;
        }
        return result;
    }
    
    
    // DONE ? : explicit tokens 
    // TODO : INDENT and UINDENT (cf simpleclause)   




}