using System;
using System.Diagnostics.CodeAnalysis;
using sly.lexer;

namespace sly.parser.parser
{
    public class GroupItem<IN, OUT>
    {
        public string Name;

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

        public Token<IN> Token { get; }

        public bool IsToken { get; set; }

        public OUT Value { get; set; }

        public bool IsValue { get; }


        public X Match<X>(Func<string, Token<IN>, X> fToken, Func<string, OUT, X> fValue)
        {
            if (IsToken)
                return fToken(Name, Token);
            return fValue(Name, Value);
        }

        public static implicit operator OUT(GroupItem<IN, OUT> item)
        {
            return item.Match((name, token) => default(OUT), (name, value) => item.Value);
        }

        public static implicit operator Token<IN>(GroupItem<IN, OUT> item)
        {
            return item.Match((name, token) => item.Token, (name, value) => default(Token<IN>));
        }

        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return IsValue ? ((OUT) this).ToString() : ((Token<IN>) this).Value;
        }
    }
}