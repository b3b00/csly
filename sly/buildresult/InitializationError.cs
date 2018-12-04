namespace sly.buildresult
{
    public class InitializationError
    {
        public InitializationError(ErrorLevel level, string message)
        {
            Message = message;
            Level = level;
        }

        public ErrorLevel Level { get; set; }

        public string Message { get; set; }
    }
}