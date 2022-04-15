
    using System;
using System.Data;
using System.Text;
using sly.lexer;
using sly.parser.generator;
    namespace ParserTests.Issue251
    {

    public class Issue251Parser
    {
        public enum Issue251Tokens
        {
            [Lexeme("[0-9][0-9]*")] DIGITS,

            [Lexeme("\"[^\"\\\\]*(\\\\.[^\"\\\\])*\"")]
            LITERAL_STRING, // "[^"\\]*(\\.[^"\\]*)*"  , which allowing escaped quotes in string. 
            [Lexeme("[a-zA-Z_][0-9a-zA-Z_]*")] IDENTIFIER,
            [Lexeme("\\+")] OP_PLUS,
            [Lexeme("-")] OP_MINUS,
            [Lexeme("\\*")] OP_MULTIPLY,
            [Lexeme("/")] OP_DIVIDE,
            [Lexeme("%")] OP_MODULE,
            [Lexeme("(")] PARENTHESIS_L,
            [Lexeme(")")] PARENTHESIS_R,

            [Lexeme("[ \\t][ \\t]*", isSkippable: true)]
            IGNORED // simply discarded in lexer
        }

        public class ExprClosure
        {
            public enum Types
            {
                INT,
                STRING
            }

            public Types ResultType;

            // C# doesn't have std::any or std::variant. Fuck it. 
            public int valueInt;
            public string valueString;
        }

        private void SyntaxCheck(bool what, string msg = "")
        {
            if (!what)
                throw new SyntaxErrorException("Syntax error: " + msg);
        }

        /*
         * RnLang is a script language, so the input expressions
         *   are directly evaluated in parser.
         *
         * int ::= DIGITS
         * string ::= LITERAL_STRING
         * expr ::= int | string
         * expr ::= expr OP_PLUS expr
         * expr ::= expr OP_MINUS expr
         * expr ::= expr OP_MULTIPLY expr
         * expr ::= expr OP_DIVIDE expr
         * expr ::= expr OP_MODULE expr
         * expr ::= IDENTIFIER PARENTHESIS_L expr PARENTHESIS_R
         * expr ::= PARENTHESIS_L expr PARENTHESIS_R
         */
        [Production("int: DIGITS")]
        public int exprIntL(Token<Issue251Tokens> expr) => expr.IntValue;

        [Production("string: LITERAL_STRING")]
        public string exprStringL(Token<Issue251Tokens> expr) => 
            expr.StringWithoutQuotes.Replace("\\\"", "\""); // default string delimiter works. 

        [Production("expr: int")]
        public ExprClosure exprInt(int what) =>
            new ExprClosure() {ResultType = ExprClosure.Types.INT, valueInt = what};

        [Production("expr: string")]
        public ExprClosure exprString(string what) =>
            new ExprClosure() {ResultType = ExprClosure.Types.STRING, valueString = what};

        [Production("expr: expr OP_PLUS expr")]
        public ExprClosure exprAdd(ExprClosure l, Token<Issue251Tokens> _, ExprClosure r)
        {
            SyntaxCheck(l.ResultType == r.ResultType, "Type error: expected int + int or string + string");
            switch (l.ResultType)
            {
                case ExprClosure.Types.INT:
                    l.valueInt += r.valueInt;
                    return l;
                case ExprClosure.Types.STRING:
                    l.valueString += r.valueString;
                    return l;
            }
            throw new ArgumentException("Internal Error");
        }

        [Production("expr: expr OP_MINUS expr")]
        public ExprClosure exprMin(ExprClosure l, Token<Issue251Tokens> _, ExprClosure r)
        {
            SyntaxCheck(l.ResultType == ExprClosure.Types.INT, "Type error: expected int - int");
            SyntaxCheck(r.ResultType == ExprClosure.Types.INT, "Type error: expected int - int");
            l.valueInt -= r.valueInt;
            return l;
        }

        [Production("expr: expr OP_MULTIPLY expr")]
        public ExprClosure exprMul(ExprClosure l, Token<Issue251Tokens> _, ExprClosure r)
        {
            SyntaxCheck(r.ResultType == ExprClosure.Types.INT, "Syntax error: expected ? * int");
            switch (l.ResultType)
            {
                case ExprClosure.Types.INT:
                    l.valueInt *= r.valueInt;
                    return l;
                case ExprClosure.Types.STRING:
                    l.valueString = new StringBuilder().Insert(0, l.valueString, r.valueInt).ToString();
                    return l;
            }
            throw new ArgumentException("Internal Error");
        }

        // [Production("expr: int OP_DIVIDE expr")]
        // public ExprClosure exprDiv(int l, Token<LexerTokens> _, ExprClosure r)
        // {
        //     SyntaxCheck(r.ResultType == ExprClosure.Types.INT, "Syntax error: expected int / int, got int / string");
        //     r.valueInt = l / r.valueInt;
        //     return r;
        // }

        // [Production("expr: int OP_MODULE expr")]
        // public ExprClosure exprImod(int l, Token<LexerTokens> _, ExprClosure r)
        // {
        //     SyntaxCheck(r.ResultType == ExprClosure.Types.INT, "Syntax error: expected int % int, got int % string");
        //     r.valueInt = l % r.valueInt;
        //     return r;
        // }
    }
}

