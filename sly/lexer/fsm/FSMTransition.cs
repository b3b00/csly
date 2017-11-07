using sly.lexer.fsm.transitioncheck;
using System;
using System.Collections.Generic;
using System.Text;

namespace sly.lexer.fsm
{
    public class FSMTransition<I, T> where I : struct, IComparable
    {
        public ITransitionCheck<I> Check { get; set; }

        public List<T> TransitionValues { get; set; }

        public int FromNode;

        public int ToNode;

        internal FSMTransition(ITransitionCheck<I> check, int from ,int to, List<T> values )
        {
            Check = check;
            TransitionValues = values;
            FromNode = from;
            ToNode = to;
        }

        internal bool Match(I token)
        {
            return Check.Match(token);
        }

    }
}
