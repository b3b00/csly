namespace sly.lexer
{
    public class StringAttribute : LexemeAttribute
    {
        public StringAttribute(string delimiter = null, string escape = null) : base(GenericToken.String, delimiter, escape)
        {
            
        } 
    }
}