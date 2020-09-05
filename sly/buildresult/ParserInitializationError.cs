namespace sly.buildresult
{
    public class ParserInitializationError : InitializationError
    {
        public ParserInitializationError(ErrorLevel level, string message, ErrorCodes code) : base(level, message, code)
        {
        }
    }
}