using sly.lexer.fsm.transitioncheck;
using System;
using System.Collections.Generic;
using System.Linq;

namespace sly.lexer.fsm
{

    public class FSMMatch<I,N>
    {

        public bool IsSuccess { get; set; }

        public N Result { get; set; }

        public List<I> Path { get; set; }

        public FSMMatch(bool success, N result = default(N), List<I> path = null)
        {
            IsSuccess = success;
            Result = result;
            Path = path;
        }
    }

    public class FSMLexer<I,T,N> where I : struct, IComparable
    {

        private Dictionary<int, List<FSMTransition<I, T>>> Transitions;

        private Dictionary<int, FSMNode<N>> Nodes;


        public FSMLexer()
        {
            Nodes = new Dictionary<int, FSMNode<N>>();
            Transitions = new Dictionary<int, List<FSMTransition<I, T>>>();
        }

        #region accessors

        internal bool HasState(int state)
        {
            return Nodes.ContainsKey(state);
        }

        internal FSMNode<N> GetNode(int state)
        {
            FSMNode<N> node = null;
            Nodes.TryGetValue(state, out node);
            return node;
        }


        internal int NewNodeId=> Nodes.Count;


        #endregion

        #region build

        public void AddTransition(FSMTransition<I, T> transition)
        {   
            var transitions = new List<FSMTransition<I, T>>();
            if (Transitions.ContainsKey(transition.FromNode))
            {
                transitions = Transitions[transition.FromNode];
            }
            transitions.Add(transition);
            Transitions[transition.FromNode] = transitions;            
        }

        public FSMNode<N> AddNode(N value)
        {
            FSMNode<N> node = new FSMNode<N>(value);
            node.Id = Nodes.Count;
            Nodes[node.Id] = node;
            return node;
        }

        public FSMNode<N> AddNode()
        {
            FSMNode<N> node = new FSMNode<N>(default(N));
            node.Id = Nodes.Count;
            Nodes[node.Id] = node;
            return node;
        }

        public FSMNode<N> AddFinalNode(N value)
        {
            FSMNode<N> node = AddNode(value);
            node.IsEnd = true;
            return node;
        }

        public FSMNode<N> AddStartNode(N value)
        {
            FSMNode<N> node = AddNode(value);
            node.IsEnd = true;
            return node;
        }

        public void Prepare()
        {
            // TODO ranger les transitions dans 1 ou des dictionnaires
        }

        #endregion


        #region run


        public FSMMatch<I,N> Run(List<I> source, int start)
        {
            var path = new List<I>();
            var result = new FSMMatch<I, N>(false);
            Stack<N> successes = new Stack<N>();
            int position = start;
            // pile de succes
            I currentToken = source[position];
            FSMNode<N> currentNode = Nodes[0];
            currentNode = Move(currentNode,currentToken);
            while(position < source.Count  && currentNode != null) {
                path.Add(currentToken); 
                if (currentNode.IsEnd)
                {
                    successes.Push(currentNode.Value);
                }

                position++;
                currentToken = source[position];
                currentNode = Move(currentNode,currentToken);
            }
            
            if (successes.Any())
            {
                result.IsSuccess = true;
                result.Result = successes.Pop();
                result.Path = path;
            }

            return result;

        }

        protected FSMNode<N> Move(FSMNode<N> from, I token)
        {
            FSMNode<N> next = null;
            if (Transitions.ContainsKey(from.Id))
            {
                var transitions = Transitions[from.Id];
                if (transitions.Any())
                {
                    int i = 0;
                    bool match = false;
                    var transition = transitions[i];
                    match = transition.Match(token);
                    while (i < transitions.Count && !match)
                    {                        
                        i++;
                        transition = transitions[i];
                        match = transition.Match(token);
                    }
                    if (match)
                    {
                        next = Nodes[transition.ToNode];
                    }
                }
            }
            return next;
        }

        #endregion

    }
}
