namespace sly.buildresult
{
    public class InitializationError
    {
        public InitializationError(ErrorLevel level, string message, ErrorCodes code)
        {
            Message = message;
            Level = level;
            Code = code;
        }

        public ErrorLevel Level { get; set; }

        public string Message { get; set; }
        
        public ErrorCodes Code {get; set;}
    }
}