using System;
using System.Diagnostics.CodeAnalysis;
using sly.lexer;

namespace sly.parser.parser
{
    public class GroupItem<IN, OUT> where IN : struct
    {
        public string Name;


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

        [ExcludeFromCodeCoverage]
        public static implicit operator OUT(GroupItem<IN, OUT> item)
        {
            return item.Match((name, token) => default, (name, value) => item.Value);
        }

        [ExcludeFromCodeCoverage]
        public static implicit operator Token<IN>(GroupItem<IN, OUT> item)
        {
            return item.Match((name, token) => item.Token, (name, value) => default);
        }

        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return IsValue ? ((OUT) this).ToString() : ((Token<IN>) this).Value;
        }
    }
}