using System;
using System.Collections.Generic;
using System.Text;
using sly.lexer;

namespace sly.pratt.parselets
{

    public delegate OUT UnaryExpressionBuilder<IN, OUT>(Token<IN> token, OUT expression);

    public delegate OUT BinaryExpressionBuilder<IN, OUT>(Token<IN> token, OUT left, OUT right);

    public abstract class Parselet<IN,OUT> where IN : struct
    {

        public int Precedence { get; private set; }

        public IN Operator { get; private set; }
        

        public Parselet(int precedence, IN oper)
        {
            Precedence = precedence;
            Operator = oper;            
        }

        
    }
}
