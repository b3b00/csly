using sly.lexer;

namespace ParserTests.errorAccuracyIssue381;

[Lexer(IgnoreWS = true, IgnoreEOL = true)]
public enum ErrorAccuracyIssue381Token
{
Eof = 0,


        [Lexeme(GenericToken.String, "\"", "\\")]
        String = 1,

        [Lexeme(GenericToken.Double)] Double = 2,

        [Lexeme(GenericToken.Int)] Int = 4,

        [Lexeme(GenericToken.Identifier, IdentifierType.Custom, "_A-Za-z", "---_-_0-9A-Za-z.")]
        Id = 5,

        [Lexeme(GenericToken.KeyWord, "DANS")] In = 15,

        [Lexeme(GenericToken.KeyWord, "VIDE")] Empty = 16,

        [Lexeme(GenericToken.KeyWord, "NON")] Not = 17,

        [Lexeme(GenericToken.KeyWord, "OU")] Or = 18,

        [Lexeme(GenericToken.KeyWord, "ET")] And = 19,

        [Lexeme(GenericToken.KeyWord, "VRAI")] True = 24,

        [Lexeme(GenericToken.KeyWord, "FAUX")] False = 25,

        [Lexeme(GenericToken.SugarToken, "=")] Set = 30,


        [Lexeme(GenericToken.SugarToken, "*")] Times = 32,
        [Lexeme(GenericToken.SugarToken, "+")] Plus = 33,
        [Lexeme(GenericToken.SugarToken, "/")] Div = 34,
        [Lexeme(GenericToken.SugarToken, "-")] Minus = 35,

        [Lexeme(GenericToken.SugarToken, "#")] Hash = 50,
        [Lexeme(GenericToken.SugarToken, ";")] SemiColon = 51,
        [Lexeme(GenericToken.SugarToken, "{")] LBrace = 52,
        [Lexeme(GenericToken.SugarToken, "}")] RBrace = 53,
        [Lexeme(GenericToken.SugarToken, ",")] Comma = 54,
        [Lexeme(GenericToken.SugarToken, ":")] Colon = 55,
        [Lexeme(GenericToken.SugarToken, "(")] Lparen = 56,
        [Lexeme(GenericToken.SugarToken, ")")] Rparen = 57,
        [Lexeme(GenericToken.SugarToken, "[")] Lbrack = 158,
        [Lexeme(GenericToken.SugarToken, "]")] Rbrack = 159,

        [Lexeme(GenericToken.SugarToken, "==")]
        Equal = 58,

        [Lexeme(GenericToken.SugarToken, "!=")]
        [Lexeme(GenericToken.SugarToken, "<>")]
        Diff = 59,
        [Lexeme(GenericToken.SugarToken, "<")] Lesser = 60,
        [Lexeme(GenericToken.SugarToken, ">")] Greater = 61,

        [Lexeme(GenericToken.SugarToken, "<=")]
        LesserEqual = 62,

        [Lexeme(GenericToken.SugarToken, ">=")]
        GreaterEqual = 63,
        [Lexeme(GenericToken.SugarToken, "[")] Lhook = 64,
        [Lexeme(GenericToken.SugarToken, "]")] Rhook = 65,


        [SingleLineComment("//")] SingleCommnent = 100,

        [MultiLineComment("/*", "*/")] MultiCommnent = 101,

       
}