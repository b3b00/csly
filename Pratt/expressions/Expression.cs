using System.Text;

namespace com.stuffwithstuff.bantam.expressions
{

    /**
     * Interface for all expression AST node classes.
     */
    public interface Expression
    {
        /**
         * Pretty-print the expression to a string.
         */
        void print(StringBuilder builder);
    }

}
