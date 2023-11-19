namespace sly.lexer
{
    public class AlphaIdAttribute : LexemeAttribute
    {
        public AlphaIdAttribute() : base(GenericToken.Identifier,IdentifierType.Alpha)
        {
            
        } 
    }
}