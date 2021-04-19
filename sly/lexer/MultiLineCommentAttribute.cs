using System;

namespace sly.lexer
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class MultiLineCommentAttribute : CommentAttribute
    {
        public MultiLineCommentAttribute(string start, string end, bool doNotIgnore = false) : base(null, start, end, doNotIgnore)
        { }
    }
}