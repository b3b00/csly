using sly.i18n;
using sly.lexer;

namespace ParserTests.lexer;

public enum DuplicateLabels
{
    [Sugar("(")]
    [LexemeLabel("en","left paranthesis")]
    [LexemeLabel("fr","paranthèse ouvrante")]
    LeftPar,
        
    [Sugar(")")]
    [LexemeLabel("en","left paranthesis")]
    [LexemeLabel("fr","paranthèse ouvrante")]
    RightPar,
        
}