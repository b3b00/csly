using System;

namespace sly.lexer
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class MultiLineCommentAttribute : CommentAttribute
    {
        public MultiLineCommentAttribute(string start, string end) : base(null, start, end)
        { }
    }
}