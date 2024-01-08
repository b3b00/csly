using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace sly.buildresult
{
    public class BuildResult<R>
    {
        public BuildResult() : this(default)
        { }

        public BuildResult(R result)
        {
            Result = result;
            Errors = new List<InitializationError>();
        }

        public List<InitializationError> Errors { get; }

        public R Result { get; set; }

        public bool IsError
        {
            get { return Errors.Any(e => e.Level != ErrorLevel.WARN); }
        }

        public bool IsOk => !IsError;

        public void AddError(InitializationError error)
        {
            Errors.Add(error);
        }

        public void AddInitializationError(ErrorLevel level, string message, ErrorCodes errorCode)
        {
            Errors.Add(new InitializationError(level,message,errorCode));
        }

        public void AddErrors(IEnumerable<InitializationError> errors)
        {
            Errors.AddRange(errors);
        }


        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            if (IsOk)
            {
                return $"parser is ok {typeof(R)}";
            }

            var error = new StringBuilder();
            error.AppendLine("parser is KO");
            foreach (var initializationError in Errors)
            {
                error.AppendLine(
                    $"{initializationError.Level} - {initializationError.Code} : {initializationError.Message}");
            }

            return error.ToString();
        }
    }
}