using System;
using System.Collections.Generic;
using sly.lexer;
using sly.lexer.fsm;

namespace jsonparser
{
    public class JSONLexer : ILexer<JsonToken>
    {
        public void AddDefinition(TokenDefinition<JsonToken> tokenDefinition)
        {
        }

        public LexerResult<JsonToken> Tokenize(string source)
        {
            return Tokenize(new ReadOnlyMemory<char>(source.ToCharArray()));
        }

        public LexerResult<JsonToken> Tokenize(ReadOnlyMemory<char> source)
        {
            var tokens = new List<Token<JsonToken>>();
            var position = 0;
            var currentLine = 1;
            var currentColumn = 1;
            var length = source.Length;

            var currentTokenLine = 1;
            var currentTokenColumn = 1;
            var currentTokenPosition = 1;
            var currentValue = "";

            var InString = false;
            var InNum = false;
            var InNull = false;
            var InTrue = false;
            var InFalse = false;
            var NumIsDouble = false;

            int tokenStartIndex = 0;
            int tokenLength = 0;

            Func<JsonToken, Token<JsonToken>> NewToken = tok =>
            {
                var token = new Token<JsonToken>();
                token.Position = new LexerPosition(currentTokenPosition, currentTokenLine, currentTokenColumn);
                token.SpanValue = source.Slice(tokenStartIndex,tokenLength);
                tokenStartIndex = tokenStartIndex + tokenLength;
                token.TokenID = tok;
                tokens.Add(token);
                currentValue = "";
                return token;
            };


            while (position < length)
            {
                var current = source.At(position);
                if (InString)
                {
                    currentValue += current;
                    if (current == '"')
                    {
                        InString = false;

                        NewToken(JsonToken.STRING);
                        position++;
                    }
                    else
                    {
                        position++;
                    }
                }
                else if (InNum)
                {
                    if (current == '.')
                    {
                        NumIsDouble = true;
                        currentValue += current;
                    }
                    else if (char.IsDigit(current))
                    {
                        currentValue += current;
                        var type = NumIsDouble ? JsonToken.DOUBLE : JsonToken.INT;
                        if (position == length - 1) NewToken(type);
                    }
                    else
                    {
                        InNum = false;
                        var type = NumIsDouble ? JsonToken.DOUBLE : JsonToken.INT;
                        NewToken(type);
                        position--;
                    }

                    position++;
                }
                else if (InNull)
                {
                    if (current == "null"[currentValue.Length])
                    {
                        currentValue += current;
                        if (currentValue.Length == 4)
                        {
                            NewToken(JsonToken.NULL);
                            InNull = false;
                        }
                    }

                    position++;
                }
                else if (InFalse)
                {
                    if (current == "false"[currentValue.Length])
                    {
                        currentValue += current;
                        if (currentValue.Length == 5)
                        {
                            NewToken(JsonToken.BOOLEAN);
                            InFalse = false;
                        }
                        else
                        {
                            position++;
                        }
                    }
                }
                else if (InTrue)
                {
                    if (current == "true"[currentValue.Length])
                    {
                        currentValue += current;
                        if (currentValue.Length == 5)
                        {
                            NewToken(JsonToken.BOOLEAN);
                            InFalse = false;
                        }
                    }

                    position++;
                }
                else
                {
                    currentValue += current;
                    if (current == '"')
                    {
                        InString = true;
                        currentValue += current;
                        currentTokenColumn = currentColumn;
                        currentTokenLine = currentLine;
                    }
                    else if (char.IsDigit(current))
                    {
                        InNum = true;
                    }
                    else if (current == 't')
                    {
                        InTrue = true;
                    }
                    else if (current == 'f')
                    {
                        InFalse = true;
                    }
                    else if (current == 'n')
                    {
                        InNull = true;
                    }
                    else if (current == '[')
                    {
                        NewToken(JsonToken.CROG);
                    }
                    else if (current == ']')
                    {
                        NewToken(JsonToken.CROD);
                    }
                    else if (current == '{')
                    {
                        NewToken(JsonToken.ACCG);
                    }
                    else if (current == '}')
                    {
                        NewToken(JsonToken.ACCD);
                    }
                    else if (current == ':')
                    {
                        NewToken(JsonToken.COLON);
                    }
                    else if (current == ',')
                    {
                        NewToken(JsonToken.COMMA);
                    }
                    else if (char.IsWhiteSpace(current))
                    {
                        currentValue = "";
                    }
                    else if (current == '\n' || current == '\r')
                    {
                        currentLine++;
                        currentValue = ";;";
                        currentColumn = 1;
                    }

                    currentColumn++;
                    position++;
                }
            }
            return new LexerResult<JsonToken>(tokens);
        }
    }
}