namespace com.stuffwithstuff.bantam {

    /**
     * Defines the different precendence levels used by the infix parsers. These
     * determine how a series of infix expressions will be grouped. For example,
     * "a + b * c - d" will be parsed as "(a + (b * c)) - d" because "*" has higher
     * precedence than "+" and "-". Here, bigger numbers mean higher precedence.
     */
    public class Precedence {
        // Ordered in increasing precedence.
        public static int ASSIGNMENT = 1;
        public static int CONDITIONAL = 2;
        public static int SUM = 3;
        public static int PRODUCT = 4;
        public static int EXPONENT = 5;
        public static int PREFIX = 6;
        public static int POSTFIX = 7;
        public static int CALL = 8;
    }
}
