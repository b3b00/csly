using System;

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
        public ValueOption()
        {
            IsNone = true;
        }

        public ValueOption(T value)
        {
            IsNone = false;
            Value = value;
        }

        private T Value { get; }

        public bool IsNone { get; }
        public bool IsSome => !IsNone;

        public T Match(Func<T, T> some, Func<T> none)
        {
            if (IsSome)
                return some(Value);
            return none();
        }
    }
}