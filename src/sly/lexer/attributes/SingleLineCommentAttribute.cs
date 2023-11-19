using System;

namespace sly.lexer
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class SingleLineCommentAttribute : CommentAttribute
    {
        public SingleLineCommentAttribute(string start, bool doNotIgnore = false, int channel = Channels.Comments) : base(start, null, null,doNotIgnore,channel)
        { }
    }
}