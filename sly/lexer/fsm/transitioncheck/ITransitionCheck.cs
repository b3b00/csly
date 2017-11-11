using System;
using System.Collections.Generic;
using System.Text;

namespace sly.lexer.fsm.transitioncheck
{
    public interface ITransitionCheck
    {
        bool Match(char input);
    }
}
