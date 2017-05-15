using System;
using System.Collections.Generic;
using System.Text;

namespace sly.parser.generator
{
    public enum EbnfToken
    {
        IDENTIFIER = 1,
        COLON = 2,
        ZEROORMORE = 3,
        ONEORMORE = 4,

        WS = 5,
        EOL = 6

    }

}
