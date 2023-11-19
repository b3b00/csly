﻿using sly.lexer;

namespace ParserTests.Issue259
{
    public enum Issue259ExpressionToken
    {
        [Lexeme("L:[A-Za-z0-9_]+")]
        LVAR = 50,

        [Lexeme("A:[:A-Za-z0-9 ]+,\\s*([A-Za-z0-9 ]+)")]
        SIMVAR = 52,

        // [Lexeme("\\[[A-Z_0-9 ]+:[A-Z_0-9 ]+\\]")]
        [Lexeme("\\[[A-Za-z_0-9 ]+:[A-Za-z_0-9 ]+\\]")]
        DCS_VAR = 53,

        [Lexeme("OFF")]
        OFF = 0,

        [Lexeme("ON")]
        ON = 1,

        // Hex/decimal prefix?
        [Lexeme("0[xX][0-9a-fA-F]+")]
        HEX_NUMBER = 2,

        [Lexeme("[0-9]+(\\.[0-9]+)?")]
        DECIMAL_NUMBER = 3,

        [Lexeme("\\+")]
        PLUS = 4,

        [Lexeme("-")]
        MINUS = 5,

        [Lexeme("\\*")]
        TIMES = 6,

        [Lexeme("/")]
        DIVIDE = 7,

        [Lexeme("\\|")]
        BITWISE_OR = 8,

        [Lexeme("&")]
        BITWISE_AND = 9,

        [Lexeme("[ \\t]+", isSkippable: true)]
        WHITESPACE = 20,

        [Lexeme("OR")]
        LOGICAL_OR = 10,
        [Lexeme("AND")]
        LOGICAL_AND = 11,

        [Lexeme("NOT")]
        NOT = 12,

        [Lexeme("(<=?)|(==)|(!=)|(>=?)")]
        COMPARISON = 13,

        [Lexeme("\\(")]
        LPAREN = 30,

        [Lexeme("\\)")]
        RPAREN = 31
    }
}