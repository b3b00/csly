using System;
using System.Collections.Generic;
using sly.lexer;

namespace rules.model;

public static class PositionExtensions
{
    public static Position SourcePosition(this LexerPosition position)
    {
        return new Position(position.Line, position.Column);
    }

    public static Position SourcePosition<IN>(this Token<IN> token) where IN : struct, Enum
    {
        return token.Position.SourcePosition();
    }
}


public interface IRule
{
    Position Position { get; set; }
    
    string Dump();

}
