package com.stuffwithstuff.bantam.expressions;

/**
 * A ternary conditional expression like "a ? b : c".
 */
public class ConditionalExpression implements Expression {
  public ConditionalExpression(
      Expression condition, Expression thenArm, Expression elseArm) {
    mCondition = condition;
    mThenArm   = thenArm;
    mElseArm   = elseArm;
  }
  
  public void print(StringBuilder builder) {
    builder.append("(");
    mCondition.print(builder);
    builder.append(" ? ");
    mThenArm.print(builder);
    builder.append(" : ");
    mElseArm.print(builder);
    builder.append(")");
  }

  private final Expression mCondition;
  private final Expression mThenArm;
  private final Expression mElseArm;
}
