package com.stuffwithstuff.bantam.parselets;

import com.stuffwithstuff.bantam.Parser;
import com.stuffwithstuff.bantam.Token;
import com.stuffwithstuff.bantam.expressions.Expression;
import com.stuffwithstuff.bantam.expressions.PostfixExpression;

/**
 * Generic infix parselet for an unary arithmetic operator. Parses postfix
 * unary "?" expressions.
 */
public class PostfixOperatorParselet implements InfixParselet {
  public PostfixOperatorParselet(int precedence) {
    mPrecedence = precedence;
  }
  
  public Expression parse(Parser parser, Expression left, Token token) {
    return new PostfixExpression(left, token.getType());
  }

  public int getPrecedence() {
    return mPrecedence;
  }
  
  private final int mPrecedence;
}