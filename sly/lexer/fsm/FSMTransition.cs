using sly.lexer.fsm.transitioncheck;
using System;
using System.Collections.Generic;
using System.Text;

namespace sly.lexer.fsm
{
    public class FSMTransition
    {
        public AbstractTransitionCheck Check { get; set; }


        public int FromNode;

        public int ToNode;

        internal FSMTransition(AbstractTransitionCheck check, int from ,int to)
        {
            Check = check;
            FromNode = from;
            ToNode = to;
        }


        public override string ToString() {
            return $"{FromNode} - {Check.ToString()} -> {ToNode}";
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
