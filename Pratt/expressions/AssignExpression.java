package com.stuffwithstuff.bantam.expressions;

/**
 * An assignment expression like "a = b".
 */
public class AssignExpression implements Expression {
  public AssignExpression(String name, Expression right) {
    mName = name;
    mRight = right;
  }
  
  public void print(StringBuilder builder) {
    builder.append("(").append(mName).append(" = ");
    mRight.print(builder);
    builder.append(")");
  }

  private final String     mName;
  private final Expression mRight;
}
