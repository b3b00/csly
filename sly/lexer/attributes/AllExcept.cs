namespace sly.lexer
{
    public class AllExceptAttribute : LexemeAttribute
    {
        public AllExceptAttribute(params string[] exceptions) : base(GenericToken.AllExcept, exceptions)
        {
        }
    }
}