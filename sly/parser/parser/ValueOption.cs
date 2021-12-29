using System;

namespace sly.parser.parser
{
    public static class ValueOptionConstructors
    {
        public static ValueOption<IN> None<IN>()
        {
            return new ValueOption<IN>();
        }

        public static ValueOption<Group<IN, OUT>> NoneGroup<IN, OUT>()
        {
            var noneGroup = new ValueOption<Group<IN,OUT>>(true);
            return noneGroup;
        }

        public static ValueOption<OUT> Some<OUT>(OUT value)
        {
            return  new ValueOption<OUT>(value);
        }
    }

    public class ValueOption<OUT>
    {
        public ValueOption()
        {
            IsNone = true;
        }
        
        public ValueOption(bool none)
        {
            IsNone = none;
        }

        public ValueOption(OUT value)
        {
            IsNone = false;
            Value = value;
        }

        private OUT Value { get; }

        public bool IsNone { get; }
        public bool IsSome => !IsNone;

        public OUT Match(Func<OUT, OUT> some, Func<OUT> none)
        {
            if (IsSome)
                return some(Value);
            return none();
        }
    }
}