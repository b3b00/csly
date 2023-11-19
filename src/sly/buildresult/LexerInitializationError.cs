namespace sly.buildresult
{
    public class LexerInitializationError : InitializationError
    {
        public LexerInitializationError(ErrorLevel level, string message, ErrorCodes code ) : base(level, message,code)
        {
        }
    }
}