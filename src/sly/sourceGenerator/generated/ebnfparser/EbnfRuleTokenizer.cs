using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using sly.lexer;

namespace sly.sourceGenerator.generated.ebnfparser;

public class EbnfRuleTokenizer
{

    private int _position = 0;
    
    private string _text;

    private List<Func<TokenizerMatch>> _tokenizers;

    private Func<TokenizerMatch> _colon;
    private Func<TokenizerMatch> _lcrog;
    private Func<TokenizerMatch> _rcrog;
    private Func<TokenizerMatch> _lparen;
    private Func<TokenizerMatch> _rparen;
    private Func<TokenizerMatch> _zeroOrMore;
    private Func<TokenizerMatch> _oneOrMore;
    private Func<TokenizerMatch> _option;
    private Func<TokenizerMatch> _discard;
    public EbnfRuleTokenizer()
    {

        _colon = GetSugar(EbnfRuleToken.COLON, ":");
        _lcrog = GetSugar(EbnfRuleToken.LCROG, "[");
        _rcrog = GetSugar(EbnfRuleToken.RCROG, "]");
        _lparen = GetSugar(EbnfRuleToken.LPAREN, "(");
        _rparen = GetSugar(EbnfRuleToken.RPAREN, ")");
        _oneOrMore = GetSugar(EbnfRuleToken.ONEORMORE, "+");
        _zeroOrMore = GetSugar(EbnfRuleToken.ZEROORMORE, "*");
        _option = GetSugar(EbnfRuleToken.OPTION, "?");
        _discard = GetSugar(EbnfRuleToken.DISCARD, "[d]");
    
    }

    public IList<Token<EbnfRuleToken>> Tokenize(string text)
    {
        _position = 0;    
        _text = text;
        var tokens = new List<Token<EbnfRuleToken>>();
        

        bool found = true;
        while (_position < _text.Length && found)
        {
            var match = IterateMatchers();
            if (match.Matches)
            {
                found = true;
                if (!match.IsWhiteSpace)
                {
                    tokens.Add(match.Token);
                }

                _position = match.NewPosition;
            }
            else
            {
                found = false;
            }
        }

        return tokens;
    }

    public TokenizerMatch IterateMatchers()
    {
        var match = WhiteSpace();
        if (match.Matches)
        {
            return match;
        }
        match = Identifier();
        if (match.Matches)
        {
            return match;
        }
        match = String();
        if (match.Matches)
        {
            return match;
        }
        match = _colon();
        if (match.Matches)
        {
            return match;
        }
        match = _discard();
        if (match.Matches)
        {
            return match;
        }
        match = _lparen();
        if (match.Matches)
        {
            return match;
        }
        match = _rparen();
        if (match.Matches)
        {
            return match;
        }
        match = _zeroOrMore();
        if (match.Matches)
        {
            return match;
        }
        match = _oneOrMore();
        if (match.Matches)
        {
            return match;
        }
        match = _option();
        if (match.Matches)
        {
            return match;
        }
        
        
        

        return TokenizerMatch.NoMatch();
    }
 

    private TokenizerMatch WhiteSpace()
    {
        if (char.IsWhiteSpace(_text[_position]))
        {
            int position = this._position;
            while (position < _text.Length && char.IsWhiteSpace(_text[position]))
            {
                position++;
            }
            return TokenizerMatch.WhiteSpace(position);
        }

        return TokenizerMatch.NoMatch();
    }

    private TokenizerMatch String()
    {
        if (_text[_position] == '\'')
        {
            string pattern = "'([^']|\\\\')*'";
            var stringRegex = new Regex(pattern);
            var r = stringRegex.Match(_text, _position);
            if (r.Success)
            {
                var value = r.Groups[0].Value;
                LexerPosition tokenPosition = new LexerPosition(_position, 0, _position);
                Token<EbnfRuleToken> token = new Token<EbnfRuleToken>(
                    EbnfRuleToken.STRING,value, tokenPosition);
                return TokenizerMatch.Found(token,_position+value.Length);
            }
        }
        return TokenizerMatch.NoMatch();
    }

    private TokenizerMatch Identifier()
    {
        if (char.IsLetter(_text[_position]) || _text[_position] == '_' )
        {
            int position = this._position;
            while (position < _text.Length && (char.IsLetterOrDigit(_text[position]) || _text[position] == '_' || _text[position] == '-'))
            {
                position++;
            }
            LexerPosition tokenPosition = new LexerPosition(_position, 0, _position);
            Token<EbnfRuleToken> token = new Token<EbnfRuleToken>(
                EbnfRuleToken.IDENTIFIER,_text.Substring(this._position,position-_position), tokenPosition);
            return TokenizerMatch.Found(token,position);
        }
        return TokenizerMatch.NoMatch();
    }
    private Func<TokenizerMatch> GetSugar(EbnfRuleToken tokenId, string sugarToken)
    {
        return () =>
        {
            int position = _position;
            int i = 0;
            
            while ( i < sugarToken.Length && position < _text.Length && sugarToken[i] == _text[_position + i])
            {
                i++;
            }

            if (i == sugarToken.Length)
            {
                LexerPosition tokenPosition = new LexerPosition(_position, 0, _position);
                Token<EbnfRuleToken> token = new Token<EbnfRuleToken>(
                    tokenId, _text.Substring(_position, i), tokenPosition);
                    
                return TokenizerMatch.Found(token,position+i);
            }
            
            return TokenizerMatch.NoMatch();
        };
    }
}