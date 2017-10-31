using System;
using System.Collections.Generic;
using System.Linq;

namespace sly.buildresult
{
    public class BuildResult<R>
    {
        public List<InitializationError> Errors { get; set; }

        public R Result { get; set; }

        public bool IsError => Errors.Any();


        public BuildResult() : this(default(R))
        {
        }

        public BuildResult(R result)
        {
            Result = result;
            Errors = new List<InitializationError>();
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
