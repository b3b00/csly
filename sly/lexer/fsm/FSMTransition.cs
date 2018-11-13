using sly.lexer.fsm.transitioncheck;

namespace sly.lexer.fsm
{
    public class FSMTransition
    {
        public int FromNode;

        public int ToNode;

        internal FSMTransition(AbstractTransitionCheck check, int from, int to)
        {
            Check = check;
            FromNode = from;
            ToNode = to;
        }

        public AbstractTransitionCheck Check { get; set; }


        public override string ToString()
        {
            return $"{FromNode} - {Check} -> {ToNode}";
        }


        internal bool Match(char token, string value)
        {
            return Check.Check(token, value);
        }

        internal bool Match(char token)
        {
            return Check.Match(token);
        }
    }
}