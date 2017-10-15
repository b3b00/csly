

using System.Text;
using com.stuffwithstuff.bantam;


namespace com.stuffwithstuff.bantam.expressions { 
/**
 * A binary arithmetic expression like "a + b" or "c ^ d".
 */
public class OperatorExpression : Expression {
  public OperatorExpression(Expression left, TokenType oper, Expression right) {
    mLeft = left;
    mOperator = oper;
    mRight = right;
  }
  
  public void print(StringBuilder builder) {
    builder.Append("(");
    mLeft.print(builder);
    builder.Append(" ").Append(TokenHelper.punctuator(mOperator)).Append(" ");
    mRight.print(builder);
    builder.Append(")");
  }

  private  Expression mLeft;
  private  TokenType  mOperator;
  private  Expression mRight;
}
}