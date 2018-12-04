namespace sly.buildresult
{
    public class ParserInitializationError : InitializationError
    {
        public ParserInitializationError(ErrorLevel level, string message) : base(level, message)
        {
        }
    }
}