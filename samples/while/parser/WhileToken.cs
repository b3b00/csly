using System;
using System.Collections.Generic;
using System.Text;
using sly.lexer;

namespace csly.whileLang.parser
{
    public enum WhileToken
    {

        #region keywords 0 -> 19

        [Lexeme("(if)")]
        IF = 1,

        [Lexeme("(then)")]
        THEN = 2,

        [Lexeme("(else)")]
        ELSE = 3,

        [Lexeme("(while)")]
        WHILE = 4,

        [Lexeme("(do)")]
        DO = 5,

        [Lexeme("(skip)")]
        SKIP = 6,

        [Lexeme("(true)")]
        TRUE = 7,

        [Lexeme("(false)")]
        FALSE = 8,
        [Lexeme("(not)")]
        NOT = 9,

        [Lexeme("(and)")]
        AND = 10,

        [Lexeme("(or)")]
        OR = 11,

        [Lexeme("(print)")]
        PRINT = 12,

        #endregion

        #region literals 20 -> 29

        [Lexeme("[a-zA-Z]+")]
        IDENTIFIER = 20,

        [Lexeme("\"[^\"]*\"")]
        STRING = 21,

        [Lexeme("[0-9]+")]
        INT = 22,

        #endregion

        #region operators 30 -> 49

        [Lexeme(">")]
        GREATER = 30,

        [Lexeme("<")]
        LESSER = 31,

        [Lexeme("==")]
        EQUALS = 32,

        [Lexeme("!=")]
        DIFFERENT = 33,

        [Lexeme("\\.")]
        CONCAT = 34,
        
        [Lexeme(":=")]
        ASSIGN = 35,

        [Lexeme("\\+")]
        PLUS = 36,

        [Lexeme("\\-")]
        MINUS = 37,


        [Lexeme("\\*")]
        TIMES = 38,

        [Lexeme("\\/")]
        DIVIDE = 39,

        #endregion 

        #region sugar 50 ->

        [Lexeme("\\(")]
        LPAREN = 50,

        [Lexeme("\\)")]
        RPAREN = 51,

        [Lexeme(";")]
        SEMICOLON = 52,

        [Lexeme("[ \\t]+", true)]
        WS = 53,

        [Lexeme("[\\n\\r]+", true, true)]
        EOL = 54,

        EOF = 0

        #endregion

    }

    public enum WhileTokenGeneric
    {

        #region keywords 0 -> 19

        [Lexeme(GenericToken.KeyWord,"if")]
        IF = 1,

        [Lexeme(GenericToken.KeyWord, "then")]
        THEN = 2,

        [Lexeme(GenericToken.KeyWord, "else")]
        ELSE = 3,

        [Lexeme(GenericToken.KeyWord, "while")]
        WHILE = 4,

        [Lexeme(GenericToken.KeyWord, "do")]
        DO = 5,

        [Lexeme(GenericToken.KeyWord, "skip")]
        SKIP = 6,

        [Lexeme(GenericToken.KeyWord, "true")]
        TRUE = 7,

        [Lexeme(GenericToken.KeyWord, "false")]
        FALSE = 8,
        [Lexeme(GenericToken.KeyWord, "not")]
        NOT = 9,

        [Lexeme(GenericToken.KeyWord, "and")]
        AND = 10,

        [Lexeme(GenericToken.KeyWord, "or")]
        OR = 11,

        [Lexeme(GenericToken.KeyWord, "(print)")]
        PRINT = 12,

        #endregion

        #region literals 20 -> 29

        [Lexeme(GenericToken.Identifier)]
        IDENTIFIER = 20,

        [Lexeme(GenericToken.String)]
        STRING = 21,

        [Lexeme(GenericToken.Int)]
        INT = 22,

        #endregion

        #region operators 30 -> 49

        [Lexeme(GenericToken.SugarToken,">")]
        GREATER = 30,

        [Lexeme(GenericToken.SugarToken, "<")]
        LESSER = 31,

        [Lexeme(GenericToken.SugarToken, "==")]
        EQUALS = 32,

        [Lexeme(GenericToken.SugarToken, "!=")]
        DIFFERENT = 33,

        [Lexeme(GenericToken.SugarToken, ".")]
        CONCAT = 34,

        [Lexeme(GenericToken.SugarToken, "=")]
        ASSIGN = 35,

        [Lexeme(GenericToken.SugarToken, "+")]
        PLUS = 36,

        [Lexeme(GenericToken.SugarToken, "-")]
        MINUS = 37,


        [Lexeme(GenericToken.SugarToken, "*")]
        TIMES = 38,

        [Lexeme(GenericToken.SugarToken, "/")]
        DIVIDE = 39,

        #endregion 

        #region sugar 50 ->

        [Lexeme(GenericToken.SugarToken, "(")]
        LPAREN = 50,

        [Lexeme(GenericToken.SugarToken, ")")]
        RPAREN = 51,

        [Lexeme(GenericToken.SugarToken, ";")]
        SEMICOLON = 52,

    

        EOF = 0

        #endregion

    }
}
