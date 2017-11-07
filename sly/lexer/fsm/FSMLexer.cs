using sly.lexer.fsm.transitioncheck;
using System;
using System.Collections.Generic;
using System.Linq;

namespace sly.lexer.fsm
{

    public class FSMMatch<N>
    {

        public bool IsSuccess { get; set; }

        public Token<N> Result { get; set; }
        
        public FSMMatch(bool success, N result = default(N), string value = null, int position = 0, int line = 0, int column = 0)
        {
            IsSuccess = success;
            Result = new Token<N>(result,value,new TokenPosition(position,line,column));
        }
    }

    public class FSMLexer<T, N>
    {

        private Dictionary<int, List<FSMTransition<T>>> Transitions;

        private Dictionary<int, FSMNode<N>> Nodes;

        public bool IgnoreWhiteSpace { get; set; }

        public List<char> WhiteSpaces { get; set; }

        public bool IgnoreEOL { get; set;}

        public bool  AggregateEOL { get; set; }

        public List<char> EOLs { get; set; }


        public FSMLexer()
        {
            Nodes = new Dictionary<int, FSMNode<N>>();
            Transitions = new Dictionary<int, List<FSMTransition<T>>>();
            IgnoreWhiteSpace = false;
            IgnoreEOL = false;
            AggregateEOL = false;
            EOLs = new List<char>();
            WhiteSpaces = new List<char>();
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


        #region  special conf
 

        #endregion

        #region build

        public void AddTransition(FSMTransition<T> transition)
        {   
            var transitions = new List<FSMTransition<T>>();
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

        int CurrentPosition = 0;


        public FSMMatch<N> Run(string source)
        {
            return Run(source, CurrentPosition);
        }

        public FSMMatch<N> Run(string source, int start)
        {
            string value = "";
            var result = new FSMMatch<N>(false);
            Stack<N> successes = new Stack<N>();
            CurrentPosition = start;
            if (CurrentPosition < source.Length)
            {
                char currentToken = source[CurrentPosition];
                FSMNode<N> currentNode = Nodes[0];
                while (CurrentPosition < source.Length && currentNode != null)
                {
                    currentToken = source[CurrentPosition];
                    currentNode = Move(currentNode, currentToken);
                    if (currentNode != null)
                    {
                        value += currentToken;
                        if (currentNode.IsEnd)
                        {
                            successes.Push(currentNode.Value);
                        }
                        CurrentPosition++;
                    }
                }
            }
            
            if (successes.Any())
            {
                result = new FSMMatch<N>(true, successes.Pop(), value,CurrentPosition); // TODO get line and column
            }
            return result;

        }

        protected FSMNode<N> Move(FSMNode<N> from, char token)
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
                        transition = transitions[i];                        
                        match = transition.Match(token);
                        i++;
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
