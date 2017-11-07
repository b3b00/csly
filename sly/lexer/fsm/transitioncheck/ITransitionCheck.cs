using System;
using System.Collections.Generic;
using System.Text;

namespace sly.lexer.fsm.transitioncheck
{
    public interface ITransitionCheck<I> where I : struct, IComparable
    {
        bool Match(I input);
    }
}
