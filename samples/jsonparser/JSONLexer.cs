using sly.lexer;
using System;
using System.Collections.Generic;
using System.Text;

namespace jsonparser
{
    public class JSONLexer : ILexer<JsonToken>
    {
        public void AddDefinition(TokenDefinition<JsonToken> tokenDefinition)
        {
        }




        public IEnumerable<Token<JsonToken>> Tokenize(string source)
        {
            var tokens = new List<Token<JsonToken>>();
            int position = 0;
            int currentLine = 1;
            int currentColumn = 1;
            int length = source.Length;

            int currentTokenLine = 1;
            int currentTokenColumn = 1;
            int currentTokenPosition = 1;
            string currentValue = "";

            bool InString = false;
            bool InNum = false;
            bool InNull = false;
            bool InTrue = false;
            bool InFalse = false;
            bool NumIsDouble = false;


            Func<JsonToken, Token<JsonToken>> NewToken = (JsonToken tok) =>
                {
                    Token<JsonToken> token = new Token<JsonToken>();
                    token.Position = new TokenPosition(currentTokenPosition, currentTokenLine, currentTokenColumn);
                    token.Value = currentValue;
                    token.TokenID = tok;
                    tokens.Add(token);
                    //currentColumn += currentValue.Length;
                    //position++;
                    currentValue = "";
                    //Console.WriteLine(token.ToString());
                    //Console.WriteLine($"{tok} /{token.Value}/ @{token.Position}");
                    return token;
                };


            while (position < length)
            {
                char current = source[position];
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
                    else if (Char.IsDigit(current))
                    {
                        currentValue += current;
                        var type = NumIsDouble ? JsonToken.DOUBLE : JsonToken.INT;
                        if (position == length-1 )
                        {
                            NewToken(type);
                        }
                    }
                    else
                    {
                        InNum = false;
                        var type = NumIsDouble ? JsonToken.DOUBLE : JsonToken.INT;
                        NewToken(type);
                        position--; // "deconsomation" du prochain char
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


            return tokens;
        }
    }
}
