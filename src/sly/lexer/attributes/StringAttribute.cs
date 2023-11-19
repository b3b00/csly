namespace sly.lexer
{
    public class StringAttribute : LexemeAttribute
    {
        public StringAttribute(string delimiter = "\"", string escape = "\\") : base(GenericToken.String, delimiter, escape)
        {   
        } 
    }
}