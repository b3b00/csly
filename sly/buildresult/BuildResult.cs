using System.Collections.Generic;
using System.Linq;

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
            get { return Errors.Any(e => e.Level != ErrorLevel.WARN); }
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
    }
}