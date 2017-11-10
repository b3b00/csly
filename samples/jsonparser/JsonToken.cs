using System;
using System.Collections.Generic;
using System.Text;
using sly.lexer;

namespace jsonparser
{
    public enum JsonToken
    {
        [Lexeme("(\\\")([^(\\\")]*)(\\\")")]
        STRING = 1,
        [Lexeme("[0-9]+\\.[0-9]+")]
        DOUBLE = 2,
       [Lexeme("[0-9]+")]
        INT = 3,
        [Lexeme("(true|false)")]
        BOOLEAN = 4,
        [Lexeme("{")]
        ACCG = 5,
        [Lexeme("}")]
        ACCD = 6,
        [Lexeme("\\[")]
        CROG = 7,
        [Lexeme("\\]")]
        CROD = 8,
        [Lexeme(",")]
        COMMA = 9,
        [Lexeme(":")]
        COLON = 10,        
        [Lexeme("[ \\t]+", true)]
        WS = 12,
        [Lexeme("[\\n\\r]+", true,true)]
        EOL = 13,
        [Lexeme("(null)")]
        NULL = 14,        
    }

    public enum JsonTokenGeneric
    {
        [Lexeme(GenericToken.String)]
        STRING = 1,
        [Lexeme(GenericToken.Double)]
        DOUBLE = 2,
        [Lexeme(GenericToken.Int)]
        INT = 3,
        [Lexeme(GenericToken.KeyWord,"true","false")]
        BOOLEAN = 4,
        [Lexeme(GenericToken.SugarToken,"{")]
        ACCG = 5,
        [Lexeme(GenericToken.SugarToken, "}")]
        ACCD = 6,
        [Lexeme(GenericToken.SugarToken, "[")]
        CROG = 7,
        [Lexeme(GenericToken.SugarToken, "]")]
        CROD = 8,
        [Lexeme(GenericToken.SugarToken, ",")]
        COMMA = 9,
        [Lexeme(GenericToken.SugarToken, ":")]
        COLON = 10,        
        [Lexeme(GenericToken.KeyWord, "null")]
        NULL = 14,
    }

}
