using sly.lexer.fsm.transitioncheck;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sly.lexer.fsm
{

    public delegate FSMMatch<IN> NodeCallback<IN>(FSMMatch<IN> node);
    public class FSMLexerBuilder<T, N> 
    {

        public FSMLexer<T, N> Fsm { get; private set; }

        private int CurrentState;

        private Dictionary<string, int> Marks;

        



        public FSMLexerBuilder()
        {
            Fsm = new FSMLexer<T, N>();
            CurrentState = 0;
            Marks = new Dictionary<string, int>();
            Fsm.AddNode(default(N));
            Fsm.GetNode(0).IsStart = true;
        }

        #region MARKS

        public FSMLexerBuilder<T,N> GoTo(int state)
        {
            if (Fsm.HasState(state))
            {
                CurrentState = state;
            }
            else
            {
                throw new ArgumentException($"state {state} does not exist in lexer FSM");
            }
            return this;
        }
        public FSMLexerBuilder<T, N> GoTo(string mark)
        {
            if (Marks.ContainsKey(mark))
            {
                GoTo(Marks[mark]);
            }
            else
            {
                throw new ArgumentException($"mark {mark} does not exist in current builder");
            }
            return this;
        }


        public FSMLexerBuilder<T, N> Mark(string mark)
        {
            Marks[mark] = CurrentState;
            Fsm.GetNode(CurrentState).Mark = mark;
            return this;
        }

        public FSMNode<N> GetNode(int nodeId)
        {
            return Fsm.GetNode(nodeId);
        }

        public FSMNode<N> GetNode(string mark)
        {
            FSMNode<N> node = null;
            if (Marks.ContainsKey(mark))
            {
                node = GetNode(Marks[mark]);
            }
            return node;
        }
        #endregion

        #region special chars

        public  FSMLexerBuilder<T, N> IgnoreWS()
        {
            Fsm.IgnoreWhiteSpace = true;
            return this;
        }

        public FSMLexerBuilder<T, N> IgnoreEOL()
        {
            Fsm.IgnoreEOL = true;
            return this;
        }

        public FSMLexerBuilder<T, N> UseEnvironmentEOL()
        {
            Fsm.EOL = Environment.NewLine;
            return this;
        }

        public FSMLexerBuilder<T, N> UseWindowsEOL()
        {
            Fsm.EOL = "\r\n";
            return this;
        }

        public FSMLexerBuilder<T, N> UseNixEOL()
        {
            Fsm.EOL = "\n";
            return this;
        }


        public FSMLexerBuilder<T, N> WhiteSpace(char spacechar)
        {
            Fsm.WhiteSpaces.Add(spacechar);
            return this;
        }

        


        #endregion


        #region NODES


        public FSMLexerBuilder<T,N> End(N nodeValue)
        {
            if (Fsm.HasState(CurrentState))
            {
                FSMNode<N> node = Fsm.GetNode(CurrentState);

                node.IsEnd = true;
                node.Value = nodeValue;

            }
            return this;
        }

        public FSMLexerBuilder<T, N> CallBack(NodeCallback<N> callback)
        {
            if (Fsm.HasState(CurrentState))
            {
                Fsm.SetCallback(CurrentState, callback);
            }

            return this;
        }

        #endregion


        #region TRANSITIONS

        public FSMLexerBuilder<T,N> Transition(char input, params T[] transitionData)
        {            
            return TransitionTo(input, Fsm.NewNodeId, transitionData);
            return this;
        }

        public FSMLexerBuilder<T, N> RangeTransition(char start, char end, params T[] transitionData)
        {
            return RangeTransitionTo(start, end, Fsm.NewNodeId, transitionData);
        }

        public FSMLexerBuilder<T, N> ExceptTransition(char[] exceptions, params T[] transitionData)
        {
            return ExceptTransitionTo(exceptions, Fsm.NewNodeId, transitionData);
        }

        public FSMLexerBuilder<T, N> AnyTransition(char input, params T[] transitionData)
        {
            return AnyTransitionTo(input, Fsm.NewNodeId, transitionData);
        }

        public FSMLexerBuilder<T, N> TransitionTo(char input, int toNode, params T[] transitionData)
        {
            ITransitionCheck checker = new TransitionSingle(input);
            if (!Fsm.HasState(toNode))
            {
                Fsm.AddNode();
            }
            var transition = new FSMTransition<T>(checker, CurrentState, toNode, transitionData.ToList<T>());
            Fsm.AddTransition(transition);
            CurrentState = toNode;
            return this;
        }

        public FSMLexerBuilder<T, N> RangeTransitionTo(char start, char end, int toNode, params T[] transitionData)
        {
            ITransitionCheck checker = new TransitionRange(start, end);
            if (!Fsm.HasState(toNode))
            {
                Fsm.AddNode();
            }
            var transition = new FSMTransition<T>(checker, CurrentState, toNode, transitionData.ToList<T>());
            Fsm.AddTransition(transition);
            CurrentState = toNode;
            return this;
        }

        public FSMLexerBuilder<T, N> ExceptTransitionTo(char[] exceptions, int toNode, params T[] transitionData)
        {
            ITransitionCheck checker = new TransitionAnyExcept(exceptions);
            if (!Fsm.HasState(toNode))
            {
                Fsm.AddNode();
            }
            var transition = new FSMTransition<T>(checker, CurrentState, toNode, transitionData.ToList<T>());
            Fsm.AddTransition(transition);
            CurrentState = toNode;
            return this;
        }

        public FSMLexerBuilder<T, N> AnyTransitionTo(char input, int toNode, params T[] transitionData)
        {
            ITransitionCheck checker = new TransitionAny(input);
            if (!Fsm.HasState(toNode))
            {
                Fsm.AddNode();
            }
            var transition = new FSMTransition<T>(checker, CurrentState, toNode, transitionData.ToList<T>());
            Fsm.AddTransition(transition);
            CurrentState = toNode;
            return this;
        }

            public FSMLexerBuilder<T, N> TransitionTo(char input, string toNodeMark, params T[] transitionData)
        {
            int toNode = Marks[toNodeMark];
            return TransitionTo(input,toNode,transitionData);
        }

        public FSMLexerBuilder<T, N> RangeTransitionTo(char start, char end, string toNodeMark, params T[] transitionData)
        {
            int toNode = Marks[toNodeMark];
            return RangeTransitionTo(start, end, toNode, transitionData);
        }

        public FSMLexerBuilder<T, N> ExceptTransitionTo(char[] exceptions, string toNodeMark, params T[] transitionData)
        {
            int toNode = Marks[toNodeMark];
            return ExceptTransitionTo(exceptions, toNode, transitionData);            
        }

        public FSMLexerBuilder<T, N> AnyTransitionTo(char input, string toNodeMark, params T[] transitionData)
        {
            int toNode = Marks[toNodeMark];
            return AnyTransitionTo(input, toNode, transitionData);
        }

    }


    #endregion

    
}

