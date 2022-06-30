using System;

namespace SimpleTemplate.model
{
    public class TemplateEvaluationException : Exception
    {
        public TemplateEvaluationException(string error) : base(error)
        {
        }
    }
}