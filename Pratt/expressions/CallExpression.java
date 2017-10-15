package com.stuffwithstuff.bantam.expressions;

import java.util.List;

/**
 * A function call like "a(b, c, d)".
 */
public class CallExpression implements Expression {
  public CallExpression(Expression function, List<Expression> args) {
    mFunction = function;
    mArgs = args;
  }
  
  public void print(StringBuilder builder) {
    mFunction.print(builder);
    builder.append("(");
    for (int i = 0; i < mArgs.size(); i++) {
      mArgs.get(i).print(builder);
      if (i < mArgs.size() - 1) builder.append(", ");
    }
    builder.append(")");
  }

  private final Expression       mFunction;
  private final List<Expression> mArgs;
}
