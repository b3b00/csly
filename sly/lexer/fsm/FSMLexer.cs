using sly.lexer.fsm.transitioncheck;
using System;
using System.Collections.Generic;
using System.Linq;

namespace sly.lexer.fsm
{

    public class FSMMatch<N>
    {

        public Dictionary<string,object> Properties { get; set; }

        public bool IsSuccess { get; set; }

        public Token<N> Result { get; set; }
        
        // public FSMMatch(bool success, N result = default(N), string value = null, int position = 0, int line = 0, int column = 0)
        // {
        //     Properties = new Dictionary<string, object>();
        //     IsSuccess = success;
        //     Result = new Token<N>(result,value,new TokenPosition(position,line,column));
        // }

        public FSMMatch(bool success, N result = default(N), string value = null, TokenPosition position = null)
        {
            Properties = new Dictionary<string, object>();
            IsSuccess = success;
            Result = new Token<N>(result,value,position);
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

        public string EOL { get; set; }

        private Dictionary<int, NodeCallback<N>> Callbacks { get; set; }

        public FSMLexer()
        {
            Nodes = new Dictionary<int, FSMNode<N>>();
            Transitions = new Dictionary<int, List<FSMTransition<T>>>();
            Callbacks = new Dictionary<int, NodeCallback<N>>();
            IgnoreWhiteSpace = false;
            IgnoreEOL = false;
            AggregateEOL = false;
            EOL = "";
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


        internal bool HasCallback(int nodeId)
        {
            return Callbacks.ContainsKey(nodeId);            
        }

        internal void SetCallback(int nodeId, NodeCallback<N> callback) {
            Callbacks[nodeId] = callback;
        }


        #endregion


        #region  special conf
 

        #endregion

        #region build

        

        public FSMTransition<T> GetTransition(int nodeId, char token)
        {
            FSMTransition<T> transition = null;
            if (HasState(nodeId))
            {
                if (Transitions.ContainsKey(nodeId))
                {
                    var leavingTransitions = Transitions[nodeId];
                    transition = leavingTransitions.FirstOrDefault((FSMTransition<T> t) => t.Match(token));
                }
            }
            return transition;
        }

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

        

        #endregion


        #region run

        int CurrentPosition = 0;
        int CurrentColumn = 0;
        int CurrentLine = 0;


        public FSMMatch<N> Run(string source)
        {
            return Run(source, CurrentPosition);
        }

        public FSMMatch<N> Run(string source, int start)
        {
            string value = "";
            var result = new FSMMatch<N>(false);
            var successes = new Stack<FSMMatch<N>>();
            CurrentPosition = start;
            FSMNode<N> currentNode = Nodes[0];
            int lastNode = 0;

            bool tokenStarted = false;
        

            if (CurrentPosition < source.Length)
            {
                char currentToken = source[CurrentPosition];


                while (CurrentPosition < source.Length && currentNode != null)
                {
                    currentToken = source[CurrentPosition];

                    bool consumeSkipped = true;

                    while (consumeSkipped && !tokenStarted)
                    {
                        currentToken = source[CurrentPosition];
                        if (IgnoreWhiteSpace && WhiteSpaces.Contains(currentToken))
                        {
                            if (successes.Any())
                            {
                                currentNode = null;
                            }
                            else
                            {
                                currentNode = Nodes[0];
                            }
                            CurrentPosition++;
                            CurrentColumn++;
                        }
                        else
                        {
                            bool newLine = true;
                            int i = 0;
                            while (newLine && i < EOL.Length && CurrentPosition+i < source.Length)
                            {
                                newLine = newLine && source[CurrentPosition + i] == EOL[i];
                                i++;
                            }
                            if (IgnoreEOL && newLine)
                            {
                                if (successes.Any())
                                {
                                    currentNode = null;
                                }
                                else
                                {
                                    currentNode = Nodes[0];
                                }
                                CurrentPosition += EOL.Length;
                                CurrentColumn = 0;
                                CurrentLine++;
                            }
                            else
                            {
                                consumeSkipped = false;
                            }
                        }
                    }

                    currentNode = Move(currentNode, currentToken);
                    if (currentNode != null)
                    {
                        lastNode = currentNode.Id;
                        value += currentToken;
                        TokenPosition position = null;
                        if (!tokenStarted)
                        {
                            tokenStarted = true;                            
                            position = new TokenPosition(CurrentPosition,CurrentLine,CurrentColumn);
                        }
                        if (currentNode.IsEnd)
                        {
                            var resultInter = new FSMMatch<N>(true, currentNode.Value, value, position);
                            successes.Push(resultInter);                            
                        }
                        CurrentPosition++;
                        CurrentColumn += value.Length;
                    }

                }
            }


            if (successes.Any())
            {
                result = successes.Pop(); 
                if (HasCallback(lastNode))
                {
                    result = Callbacks[lastNode](result);
                }
            }
            return result;

        }

        protected FSMNode<N> Move(FSMNode<N> from, char token)
        {
            FSMNode<N> next = null;
            if (from != null)
            {               
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
            }
            return next;
        }



        #endregion

    }
}
