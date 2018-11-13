using System.Collections.Generic;
using System.Linq;

namespace sly.buildresult
{
    public class BuildResult<R>
    {
        public BuildResult() : this(default(R))
        {
        }

        public BuildResult(R result)
        {
            Result = result;
            Errors = new List<InitializationError>();
        }

        public List<InitializationError> Errors { get; set; }

        public R Result { get; set; }


        public bool IsError
        {
            get { return Errors.Where(e => e.Level != ErrorLevel.WARN).Any(); }
            set { }
        }

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