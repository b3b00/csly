namespace sly.lexer
{
    public class AlphaNumIdAttribute : LexemeAttribute
    {
        public AlphaNumIdAttribute() : base(GenericToken.Identifier,IdentifierType.AlphaNumeric)
        {
            
        } 
    }
}