using System;
using System.Collections.Generic;
using com.stuffwithstuff.bantam.expressions;
using com.stuffwithstuff.bantam.parselets;


namespace  com.stuffwithstuff.bantam {

public class Parser {
  public Parser(List<Token> tokens) {
    mTokens = tokens;
  }
  
  public void register(TokenType token, PrefixParselet parselet) {
    mPrefixParselets.put(token, parselet);
  }
  
  public void register(TokenType token, InfixParselet parselet) {
    mInfixParselets.put(token, parselet);
  }
  
  public Expression parseExpression(int precedence) {
    Token token = consume();
    PrefixParselet prefix = mPrefixParselets.get(token.getType());
    
    if (prefix == null) throw new ParseException("Could not parse \"" +
        token.getText() + "\".");
    
    Expression left = prefix.parse(this, token);
    
    while (precedence < getPrecedence()) {
      token = consume();
      
      InfixParselet infix = mInfixParselets.get(token.getType());
      left = infix.parse(this, left, token);
    }
    
    return left;
  }
  
  public Expression parseExpression() {
    return parseExpression(0);
  }
  
  public boolean match(TokenType expected) {
    Token token = lookAhead(0);
    if (token.getType() != expected) {
      return false;
    }
    
    consume();
    return true;
  }
  
  public Token consume(TokenType expected) {
    Token token = lookAhead(0);
    if (token.getType() != expected) {
      throw new Exception("Expected token " + expected +
          " and found " + token.getType());
    }
    
    return consume();
  }

        public Token consume()
        {
            // Make sure we've read the token.
            lookAhead(0);
            Token t = mRead[0];
            mRead.RemoveAt(0);
            return t;
        }
  
  private Token lookAhead(int distance) {
    // Read in as many as needed.
    while (distance >= mRead.Count) {
      mRead.Add(mTokens.next());
    }

    // Get the queued token.
    return mRead.get(distance);
  }

        private int getPrecedence()
        {
            Token next = lookAhead(0);
            TokenType toktyp = next.getType();
            InfixParselet parser = mInfixParselets.ContainsKey(toktyp) ? mInfixParselets[toktyp] : null;
            if (parser != null) return parser.getPrecedence();

            return 0;
        }
  
  private  List<Token> mTokens;
  private  List<Token> mRead = new List<Token>();
  private  Dictionary<TokenType, PrefixParselet> mPrefixParselets =
      new Dictionary<TokenType, PrefixParselet>();
  private Dictionary<TokenType, InfixParselet> mInfixParselets =
      new Dictionary<TokenType, InfixParselet>();
}

}