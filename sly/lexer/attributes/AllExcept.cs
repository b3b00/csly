namespace sly.lexer
{
    public class AllExceptAttribute : LexemeAttribute
    {
        public AllExceptAttribute(string exceptions) : base(GenericToken.AllExcept, exceptions)
        {
        }
    }
}