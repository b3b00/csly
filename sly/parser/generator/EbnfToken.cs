using System;
using System.Collections.Generic;
using System.Text;

namespace sly.parser.generator
{
    public enum EbnfToken
    {
        IDENTIFIER = 1,
        COLON = 2,
        STAR = 3,
        PLUS = 4,

        WS = 5,
        EOL = 6

    }

}
