using System;
using System.Collections.Generic;
using System.Text;
using sly.lexer;

namespace sly.parser.parser
{


    public class GroupItem<IN, OUT>
    {
        public string Name;

        public Token<IN> Token { get; }

        public bool IsToken { get; set; }

        public OUT Value { get; set; }

        public bool IsValue { get; }

        public GroupItem(string name)
        {
            Name = name;
            IsToken = false;
            IsValue = false;
        }
        public GroupItem(string name, Token<IN> token)
        {
            Name = name;
            IsToken = true;
            Token = token;
        }

        public GroupItem(string name, OUT value)
        {
            Name = name;
            IsValue = true;
            Value = value;
        }


        public X Match<X>(Func<string, Token<IN>, X> fToken, Func<string, OUT, X> fValue)
        {
            if (IsToken)
            {
                return fToken(Name, Token);
            }
            else
            {
                return fValue(Name, Value);
            }
        }

        public static implicit operator OUT(GroupItem<IN,OUT> item)   {
            return item.Match((string name, Token<IN> token) => default(OUT), (string name, OUT value) => item.Value);
        }

        public static implicit operator Token<IN>(GroupItem<IN,OUT> item)   {
            return item.Match((string name, Token<IN> token) => item.Token, (string name, OUT value) => default(Token<IN>));
        }

        public string ToString()
        {
            return IsValue ? ((OUT)this).ToString() : ((Token<IN>)this).Value;
        }
    }
}
