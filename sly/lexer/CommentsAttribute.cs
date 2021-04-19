using System;

namespace sly.lexer
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class CommentAttribute : Attribute
    {
        public string MultiLineCommentEnd;

        public string MultiLineCommentStart;

        public string SingleLineCommentStart;

        public bool DoNotIgnore = false;

        public CommentAttribute(string singleLineStart, string multiLineStart, string multiLineEnd, bool doNotIgnore = false)
        {
            SingleLineCommentStart = singleLineStart;
            MultiLineCommentStart = multiLineStart;
            MultiLineCommentEnd = multiLineEnd;
            DoNotIgnore = doNotIgnore;
        }
    }
}