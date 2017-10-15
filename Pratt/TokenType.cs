package com.stuffwithstuff.bantam;

public enum TokenType {
  LEFT_PAREN,
  RIGHT_PAREN,
  COMMA,
  ASSIGN,
  PLUS,
  MINUS,
  ASTERISK,
  SLASH,
  CARET,
  TILDE,
  BANG,
  QUESTION,
  COLON,
  NAME,
  EOF;
  
  /**
   * If the TokenType represents a punctuator (i.e. a token that can split an
   * identifier like '+', this will get its text.
   */
 
  }


public static class TokenHelper
{
    public static char punctuator(TokenType tok)
    {
        switch (tok)
        {
            case TokenType.LEFT_PAREN: return '(';
            case TokenType.RIGHT_PAREN: return ')';
            case TokenType.COMMA: return ',';
            case TokenType.ASSIGN: return '=';
            case TokenType.PLUS: return '+';
            case TokenType.MINUS: return '-';
            case TokenType.ASTERISK: return '*';
            case TokenType.SLASH: return '/';
            case TokenType.CARET: return '^';
            case TokenType.TILDE: return '~';
            case TokenType.BANG: return '!';
            case TokenType.QUESTION: return '?';
            case TokenType.COLON: return ':';
            default: return ' ';
        }
    }
}
