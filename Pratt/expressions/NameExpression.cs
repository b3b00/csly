using System.Text;

namespace com.stuffwithstuff.bantam.expressions
{

    /**
     * A simple variable name expression like "abc".
     */
    public class NameExpression : Expression
    {
        public NameExpression(string name)
        {
            mName = name;
        }

        public string getName() { return mName; }

        public void print(StringBuilder builder)
        {
            builder.Append(mName);
        }

        private string mName;
    }

}