using System;
using System.Collections.Generic;
using System.Text;
using sly.lexer;

namespace csly.whileLang.parser
{
    public enum WhileToken
    {   

        [Lexeme("(if)")]
        IF = 0,

        [Lexeme("(then)")]
        THEN = 1,

        [Lexeme("(else)")]
        ELSE = 2,

        [Lexeme("(while)")]
        WHILE = 3,

        [Lexeme("(do)")]
        DO = 4,

        [Lexeme("(skip)")]
        SKIP = 5,

        [Lexeme("(true)")]
        TRUE = 6,

        [Lexeme("(false)")]
        FALSE = 7,
        [Lexeme("(not)")]
        NOT = 8,

        [Lexeme("(and)")]
        AND = 9,

        [Lexeme("(or)")]
        OR = 10,



        [Lexeme(">")]
        GREATER = 11,

        [Lexeme("<")]
        LESSER = 12,

        [Lexeme("==")]
        EQUALS = 13,

        [Lexeme("\\.")]
        CONCAT = 14,

        [Lexeme("(print)")]
        PRINT = 15,

        [Lexeme("[a-zA-Z]+")]
        IDENTIFIER = 16,

        [Lexeme("\"[^\"]*\"")]
        STRING = 17,

        [Lexeme(":=")]
        ASSIGN = 18,

        [Lexeme("\\+")]
        PLUS = 19,

        [Lexeme("\\-")]
        MINUS = 20,

        [Lexeme("\\*")]
        TIMES = 21,

        [Lexeme("\\/")]
        DIVIDE = 22,

        [Lexeme("\\(")]
        LPAREN = 23,

        [Lexeme("\\)")]
        RPAREN = 24,

        [Lexeme(";")]
        SEMICOLON = 25,

        [Lexeme("[0-9]+")]
        INT = 26,



        [Lexeme("[ \\t]+", true)]
        WS = 27,

        [Lexeme("[\\n\\r]+", true, true)]
        EOL = 28

    }
}
