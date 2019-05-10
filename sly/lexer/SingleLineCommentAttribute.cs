using System;

namespace sly.lexer
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class SingleLineCommentAttribute : CommentAttribute
    {
        private new string MultiLineCommentEnd;

        private new string MultiLineCommentStart;

        public string SingleLineCommentStart;

        public SingleLineCommentAttribute(string start) : base(start, null, null)
        { }
    }
}