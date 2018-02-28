using System;
using System.Collections.Generic;
using System.Text;

namespace sly.lexer
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class CommentAttribute : Attribute
    {

        public string SingleLineCommentStart;

        public string MultiLineCommentStart;

        public string MultiLineCommentEnd;

        public CommentAttribute(string singleLineStart, string MultiLineStart, string multiLineEnd)
        {
            SingleLineCommentStart = singleLineStart;
            MultiLineCommentStart = MultiLineStart;
            MultiLineCommentEnd = multiLineEnd;
        }




    }


}

