using sly.lexer.fsm.transitioncheck;
using System;
using System.Collections.Generic;
using System.Text;

namespace sly.lexer.fsm
{
    public class FSMTransition<T> 
    {
        public AbstractTransitionCheck Check { get; set; }

        public List<T> TransitionValues { get; set; }

        public int FromNode;

        public int ToNode;

        internal FSMTransition(AbstractTransitionCheck check, int from ,int to, List<T> values )
        {
            Check = check;
            TransitionValues = values;
            FromNode = from;
            ToNode = to;
        }


        internal bool Match(char token, string value)
        {
            return Check.Check(token,value);
        }

         internal bool Match(char token)
        {
            return Check.Match(token);
        }

    }
}
