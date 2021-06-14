namespace sly.lexer
{
    public class KeywordAttribute : LexemeAttribute
    {
        public KeywordAttribute(string keyword) : base(GenericToken.KeyWord, keyword)
        {
            
        }
    }
}