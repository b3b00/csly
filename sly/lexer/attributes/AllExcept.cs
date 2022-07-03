namespace sly.lexer
{
    public class UpToAttribute : LexemeAttribute
    {
        public UpToAttribute(params string[] exceptions) : base(GenericToken.UpTo, exceptions)
        {
        }
    }
}