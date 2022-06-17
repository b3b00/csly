using sly.lexer.fsm.transitioncheck;
using System.Collections.Generic;
using System;

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


        private string GraphVizNodeLabel<N>(FSMNode<N> node)
        {
            var push = node.IsPushModeNode ? "Push(" + node.PushToMode + ")" : "";
            var pop = node.IsPopModeNode ? "Pop()" : "";
            return (node.Mark ?? "")+ push+ " "+pop+" #"+node.Id;
        }
        
        public string ToGraphViz<N>(Dictionary<int, FSMNode<N>> nodes)
        {
            string f = "\""+GraphVizNodeLabel(nodes[FromNode])+"\"";
            string t = "\""+GraphVizNodeLabel(nodes[ToNode])+"\"";
            return $"{f} -> {t} {Check.ToGraphViz()}";
        }


        internal bool Match(char token, ReadOnlyMemory<char> value)
        {
            return Check.Check(token, value);
        }

        internal bool Match(char token)
        {
            return Check.Match(token);
        }
    }
}