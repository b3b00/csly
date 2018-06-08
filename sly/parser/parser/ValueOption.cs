using System;
using System.Collections.Generic;
using System.Text;

namespace sly.parser.parser
{
    public static class ValueOptionConstructors
    {
        public static ValueOption<T> None<T>()
        {
            return new ValueOption<T>();
        }

        public static ValueOption<T> Some<T>(T value)
        {
            return new ValueOption<T>(value);
        }
    }

    public class ValueOption<T>
    {
        T Value { get; }


        public ValueOption()
        {
            IsNone = true;
        }
        public ValueOption(T value)
        {
            IsNone = false;
            Value = value;
        }

        public bool IsNone { get; private set; }
        public bool IsSome => !IsNone;

        public T Match(Func<T,T> some, Func<T> none)
        {            
            if (IsSome)
            {
                return some(Value);
            }
            else
            {
                return none();
            }
        }
    }
}
