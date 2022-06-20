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


   
        
        public string ToGraphViz<N>(Dictionary<int, FSMNode<N>> nodes)
        {
            // string f = "\""+nodes[FromNode].GraphVizNodeLabel<N>()+"\"";
            // string t = "\""+nodes[ToNode].GraphVizNodeLabel<N>()+"\"";
            return $"{nodes[FromNode].Id} -> {nodes[ToNode].Id} {Check.ToGraphViz()}";
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