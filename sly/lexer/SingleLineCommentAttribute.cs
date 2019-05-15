using System;

namespace sly.lexer
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class SingleLineCommentAttribute : CommentAttribute
    {
        public SingleLineCommentAttribute(string start) : base(start, null, null)
        { }
    }
}