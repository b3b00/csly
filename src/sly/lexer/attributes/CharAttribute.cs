namespace sly.lexer
{
    public class CharacterAttribute : LexemeAttribute
    {
        public CharacterAttribute(string delimiter = "'", string escape = "\\") : base(GenericToken.String, delimiter, escape)
        {   
        } 
    }
}