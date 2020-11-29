using System;

namespace sly.lexer
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class MultiLineCommentAttribute : CommentAttribute
    {
        public MultiLineCommentAttribute(string start, string end, bool doNotIgnore = false, int channel = Channels.Comments) : base(null, start, end, doNotIgnore,channel)
        { }
    }
}