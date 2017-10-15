

using com.stuffwithstuff.bantam;
using com.stuffwithstuff.bantam.expressions;


namespace com.stuffwithstuff.bantam.parselets { 

/**
 * Simple parselet for a named variable like "abc".
 */
public class NameParselet : PrefixParselet {

        public Expression parse(Parser parser, Token token)
        {
            return new NameExpression(token.getText());
        }
    }
}