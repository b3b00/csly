using System;
using System.Collections.Generic;
using System.Text;
using sly.lexer;

namespace csly.whileLang.parser
{
    public enum WhileToken
    {   

        [Lexeme("(if)")]
        IF,

        [Lexeme("(then)")]
        THEN,

        [Lexeme("(else)")]
        ELSE,

        [Lexeme("(while)")]
        WHILE,

        [Lexeme("(do)")]
        DO,

        [Lexeme("(skip)")]
        SKIP,

        [Lexeme("(true)")]
        TRUE,

        [Lexeme("(false)")]
        FALSE,
        [Lexeme("(not)")]
        NOT,

        [Lexeme("(and)")]
        AND,

        [Lexeme("(or)")]
        OR,

        [Lexeme(">")]
        GREATER,

        [Lexeme("<")]
        LESSER,

        [Lexeme("==")]
        EQUALS,

        [Lexeme("!=")]
        DIFFERENT,

        [Lexeme("\\.")]
        CONCAT,

        [Lexeme("(print)")]
        PRINT,

        [Lexeme("[a-zA-Z]+")]
        IDENTIFIER,

        [Lexeme("\"[^\"]*\"")]
        STRING,

        [Lexeme(":=")]
        ASSIGN,

        [Lexeme("\\+")]
        PLUS,

        [Lexeme("\\-")]
        MINUS,

        [Lexeme("\\*")]
        TIMES,

        [Lexeme("\\/")]
        DIVIDE,

        [Lexeme("\\(")]
        LPAREN,

        [Lexeme("\\)")]
        RPAREN,

        [Lexeme(";")]
        SEMICOLON,

        [Lexeme("[0-9]+")]
        INT,

        [Lexeme("[ \\t]+", true)]
        WS,

        [Lexeme("[\\n\\r]+", true, true)]
        EOL,
        
        EOF = 0

    }
}
