namespace sly.lexer
{
    public class AlphaNumDashIdAttribute : LexemeAttribute
    {
        public AlphaNumDashIdAttribute() : base(GenericToken.Identifier,IdentifierType.AlphaNumericDash)
        {
            
        } 
    }
}