using System;

namespace sly.lexer
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class MultiLineCommentAttribute : CommentAttribute
    {
        public string MultiLineCommentEnd;

        public string MultiLineCommentStart;

        private new string SingleLineCommentStart;

        public MultiLineCommentAttribute(string start, string end) : base(null, start, end)
        { }
    }
}