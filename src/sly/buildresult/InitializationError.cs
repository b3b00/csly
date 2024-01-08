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

        public ErrorLevel Level { get;  }

        public string Message { get;  }
        
        public ErrorCodes Code {get; }
    }
}