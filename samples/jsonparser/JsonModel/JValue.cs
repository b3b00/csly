namespace jsonparser.JsonModel
{
    public class JValue : JSon
    {
        private readonly object value;

        public JValue(object val)
        {
            value = val;
        }

        public override bool IsValue => true;

        public bool IsString => value is string;

        public bool IsInt => value is int;

        public bool IsDouble => value is double;

        public bool IsBool => value is bool;

        public T GetValue<T>()
        {
            return (T) value;
        }
    }
}