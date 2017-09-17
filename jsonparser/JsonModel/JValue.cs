

namespace jsonparser.JsonModel
{
    public class JValue : JSon
    {

        public override bool IsValue => true;
        
        private object value;

        public JValue(object val)
        {
            value = val;
        }

        public T GetValue<T>() => (T) value;

        public bool IsString => value is string;
        
        public bool IsInt => value is int;
        
        public bool IsDouble => value is double;
        
        public bool IsBool => value is bool;


    }
}