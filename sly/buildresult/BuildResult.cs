using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sly.buildresult
{
    public class BuildResult<R>
    {
        public BuildResult() : this(default(R))
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
            get { return Errors.Any<InitializationError>(e => e.Level != ErrorLevel.WARN); }
        }

        public bool IsOk => !IsError;

        public void AddError(InitializationError error)
        {
            Errors.Add(error);
        }

        public void AddErrors(List<InitializationError> errors)
        {
            Errors.AddRange(errors);
        }

        public override string ToString()
        {
            if (IsOk)
            {
                return $"parser is ok {typeof(R)}";
            }
            else
            {
                StringBuilder error = new StringBuilder();
                error.AppendLine("parser is KO");
                foreach (var initializationError in Errors)
                {
                    error.AppendLine($"{initializationError.Level} - {initializationError.Code} : {initializationError.Message}");
                }

                return error.ToString();
            }
        }
    }
}