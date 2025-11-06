using System;
using System.Collections.Generic;
using System.Linq;
using sly.lexer;
using sly.parser;
using sly.parser.syntax.grammar;
using sly.parser.syntax.tree;

namespace staticParsing;

public enum Tok
{
    T1,
    T2,
    T3,
    ID
}
public class StaticParser 
{
    public Dictionary<Tok, Dictionary<string, string>> LexemeLabels { get; set; }
    
    public string I18n { get; set; }

    public SyntaxParseResult<Tok, string> parseTerminal(List<Token<Tok>> tokens, Tok expected, int position,
        bool discarded = false)
    {
        var result = new SyntaxParseResult<Tok, string>();
        var token = tokens[position];
        result.IsError = expected != tokens[position].TokenID;
        result.EndingPosition = !result.IsError ? position + 1 : position;
        
        if (result.IsError)
        {
            result.AddError(new UnexpectedTokenSyntaxError<Tok>(token, LexemeLabels, I18n, new LeadingToken<Tok>(expected)));
        }
        else
        {
           
            token.Discarded = discarded;
            token.IsExplicit = false;
            result.Root = new SyntaxLeaf<Tok, string>(token, discarded);
            result.HasByPassNodes = false;
        }

        return result;
    }

    public SyntaxParseResult<Tok, string> parseExplicitTerminal(List<Token<Tok>> tokens, string expected, int position,
        bool discarded = false)
    {
        var result = new SyntaxParseResult<Tok, string>();

        result.EndingPosition = !result.IsError ? position + 1 : position;

        var leading = new LeadingToken<Tok>(default(Tok), expected);

        result.IsError = !leading.Match(tokens[position]);
        var token = tokens[position];

        if (result.IsOk)
        {
            token.Discarded = discarded;
            token.IsExplicit = false;
            result.Root = new SyntaxLeaf<Tok, string>(token, discarded);
            result.HasByPassNodes = false;
        }
        else
        {
            result.AddError(new UnexpectedTokenSyntaxError<Tok>(token, LexemeLabels, I18n, leading));
        }

        return result;
    }
    
    public SyntaxParseResult<Tok,string> ParseTerminal_T1(List<Token<Tok>> tokens ,int position, bool discarded = false) 
        => parseTerminal(tokens,Tok.T1,position,discarded);
    
    public SyntaxParseResult<Tok,string> ParseTerminal_T2(List<Token<Tok>> tokens ,int position, bool discarded = false) 
        => parseTerminal(tokens,Tok.T2,position,discarded);

    public SyntaxParseResult<Tok,string> ParseTerminal_T3(List<Token<Tok>> tokens ,int position, bool discarded = false) 
        => parseTerminal(tokens,Tok.T3,position,discarded);

    public SyntaxParseResult<Tok, string> ParseExplicitTerminal_explicit(List<Token<Tok>> tokens, int position, bool discarded = false)
     => parseExplicitTerminal(tokens,"explicit",position,discarded);
    // root : T1 items
    public SyntaxParseResult<Tok, string> ParseRule_root(List<Token<Tok>> tokens, int position)
    {
        var result = new SyntaxParseResult<Tok, string>();
        
        
        
        var r2 = ParseNonTerminal_items(tokens,position);
        if (r2.IsError)
        {
            return r2;
        }
        position = r2.EndingPosition;
        var r3 = ParseTerminal_T1(tokens, position);
        if (r3.IsError)
        {
            return r3;
        }
        
        

        var tree = new SyntaxNode<Tok, string>("root", new List<ISyntaxNode<Tok, string>>() { r2.Root, r3.Root},
            null);
        result.Root = tree;
        result.IsError = false;
        result.EndingPosition = r2.EndingPosition;
        // result.AddErrors(r3.GetErrors());
        // result.AddErrors(r2.GetErrors());
        return result;
    }
    
    // non terminal
    public SyntaxParseResult<Tok, string> ParseNonTerminal_items(List<Token<Tok>> tokens, int position)
    {
        var result = new SyntaxParseResult<Tok, string>();
        var token = tokens[position];
        var results = new List<SyntaxParseResult<Tok, string>>();
        
        var r1Leadings = new[]
        {
            new LeadingToken<Tok>(Tok.T2)
        };
        if (r1Leadings.Any(x => x.Match(token))) {
            var r1 = ParseRule_items_item_items(tokens, position);
            if (r1.IsOk)
            {
                return r1;
            }
            results.Add(r1);
        }
        var r2Leadings = new[]
        {
            new LeadingToken<Tok>(Tok.T2)
        };
        if (r2Leadings.Any(x => x.Match(token)))
        {
            var r2 = ParseRule_items_item(tokens, position);
            if (r2.IsOk)
            {
                return r2;
            }
            results.Add(r2);
        }

        result.IsError = true;
        result.AddErrors(results.SelectMany(x => x.Errors != null ? x.GetErrors() : new List<UnexpectedTokenSyntaxError<Tok>>()).ToList());
        return result;
    }
    
    public SyntaxParseResult<Tok, string> ParseRule_items_item_items(List<Token<Tok>> tokens, int position)
    {
        var result = new SyntaxParseResult<Tok, string>();
        
        
        var r1 = ParseNonTerminal_item(tokens,position);
        if (r1.IsError)
        {
            return r1;
        }

        position = r1.EndingPosition;
        
        var r2 = ParseNonTerminal_items(tokens,position);
        if (r2.IsError)
        {
            return r2;
        }

        result.IsError = false;
        result.EndingPosition = r2.EndingPosition;
        result.Root = new SyntaxNode<Tok, string>("items", new List<ISyntaxNode<Tok, string>>() { r1.Root,r2.Root});
        // result.AddErrors(r1.GetErrors());
        // result.AddErrors(r2.GetErrors());
        
        return result;
    }
    
    public SyntaxParseResult<Tok, string> ParseRule_items_item(List<Token<Tok>> tokens, int position)
    {
        var result = new SyntaxParseResult<Tok, string>();
        
        var r1 = ParseNonTerminal_item(tokens,position);
        if (r1.IsError)
        {
            return r1;
        }

        result.IsError = false;
        result.EndingPosition = r1.EndingPosition;
        result.Root = new SyntaxNode<Tok, string>("items", new List<ISyntaxNode<Tok, string>>() { r1.Root});
        // result.AddErrors(r1.GetErrors());
        
        return result;
    }

    
    public SyntaxParseResult<Tok, string> ParseNonTerminal_item(List<Token<Tok>> tokens, int position)
    {
        return ParseRule_item_T2_T3_explicit(tokens, position);
    }

    public SyntaxParseResult<Tok, String> ParseRule_item_T2_T3_explicit(List<Token<Tok>> tokens, int position)
    {
        var result = new SyntaxParseResult<Tok, string>();
        
        var r1 = ParseTerminal_T2(tokens,position);
        if (r1.IsError)
        {
            return r1;
        }
        position = r1.EndingPosition;
        var r2=  ParseTerminal_T3(tokens,position, discarded: true);
        if (r2.IsError)
        {
            return r2;
        }
        position = r2.EndingPosition;
        var r3=  ParseExplicitTerminal_explicit(tokens,position);
        if (r3.IsError)
        {
            return r3;
        }
        var tree = new SyntaxNode<Tok, string>("item", new List<ISyntaxNode<Tok, string>>() { r1.Root, r2.Root, r3.Root },
            null);
        result.Root = tree;
        result.IsError = false;
        result.EndingPosition = r3.EndingPosition;
        // result.AddErrors(r1.GetErrors());
        // result.AddErrors(r2.GetErrors());
        // result.AddErrors(r3.GetErrors());
        return result;
    }
}