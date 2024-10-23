namespace sly.lexer
{
    public class KeywordAttribute : LexemeAttribute
    {
        public KeywordAttribute(string keyword, int channel = Channels.Main) : base(GenericToken.KeyWord, keyword)
        {
            
        }
    }
}