using sly.lexer.fsm.transitioncheck;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sly.lexer.fsm
{
    public class FSMLexerBuilder<I, T, N> where I : struct, IComparable
    {

        public FSMLexer<I, T, N> Fsm { get; private set; }

        private int CurrentState;

        private Dictionary<string, int> Marks;

        public FSMLexerBuilder()
        {
            Fsm = new FSMLexer<I, T, N>();
            CurrentState = 0;
            Marks = new Dictionary<string, int>();
            Fsm.AddNode(default(N));
            Fsm.GetNode(0).IsStart = true;
        }

        #region MARKS

        public FSMLexerBuilder<I,T,N> GoTo(int state)
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
        public FSMLexerBuilder<I, T, N> GoTo(string mark)
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


        public FSMLexerBuilder<I, T, N> Mark(string mark)
        {
            Marks[mark] = CurrentState;
            return this;
        }

        #endregion


        #region NODES


        public FSMLexerBuilder<I,T,N> End(N nodeValue)
        {
            if (Fsm.HasState(CurrentState))
            {
                FSMNode<N> node = Fsm.GetNode(CurrentState);

                node.IsEnd = true;
                node.Value = nodeValue;

            }
            return this;
        }

        #endregion


        #region TRANSITIONS

        public FSMLexerBuilder<I,T,N> Transition(I input, params T[] transitionData)
        {            
            return TransitionTo(input, Fsm.NewNodeId, transitionData);
            return this;
        }

        public FSMLexerBuilder<I, T, N> RangeTransition(I start, I end, params T[] transitionData)
        {
            return RangeTransitionTo(start, end, Fsm.NewNodeId, transitionData);
        }

        public FSMLexerBuilder<I, T, N> ExceptTransition(I input, params T[] transitionData)
        {
            return ExceptTransitionTo(input, Fsm.NewNodeId, transitionData);
        }

        public FSMLexerBuilder<I, T, N> TransitionTo(I input, int toNode, params T[] transitionData)
        {
            ITransitionCheck<I> checker = new TransitionSingle<I>(input);
            if (!Fsm.HasState(toNode))
            {
                Fsm.AddNode();
            }
            var transition = new FSMTransition<I, T>(checker, CurrentState, toNode, transitionData.ToList<T>());
            Fsm.AddTransition(transition);
            CurrentState = toNode;
            return this;
        }

        public FSMLexerBuilder<I, T, N> RangeTransitionTo(I start, I end, int toNode, params T[] transitionData)
        {
            ITransitionCheck<I> checker = new TransitionRange<I>(start, end);
            if (!Fsm.HasState(toNode))
            {
                Fsm.AddNode();
            }
            var transition = new FSMTransition<I, T>(checker, CurrentState, toNode, transitionData.ToList<T>());
            Fsm.AddTransition(transition);
            CurrentState = toNode;
            return this;
        }

        public FSMLexerBuilder<I, T, N> ExceptTransitionTo(I input, int toNode, params T[] transitionData)
        {
            ITransitionCheck<I> checker = new TransitionAnyExcept<I>(input);
            if (!Fsm.HasState(toNode))
            {
                Fsm.AddNode();
            }
            var transition = new FSMTransition<I, T>(checker, CurrentState, Fsm.NewNodeId, transitionData.ToList<T>());
            Fsm.AddTransition(transition);
            CurrentState = toNode;
            return this;
        }


        public FSMLexerBuilder<I, T, N> TransitionTo(I input, string toNodeMark, params T[] transitionData)
        {
            int toNode = Marks[toNodeMark];
            return TransitionTo(input,toNode,transitionData);
        }

        public FSMLexerBuilder<I, T, N> RangeTransitionTo(I start, I end, string toNodeMark, params T[] transitionData)
        {
            int toNode = Marks[toNodeMark];
            return RangeTransitionTo(start, end, Fsm.NewNodeId, transitionData);
        }

        public FSMLexerBuilder<I, T, N> ExceptTransitionTo(I input, string toNodeMark, params T[] transitionData)
        {
            int toNode = Marks[toNodeMark];
            return ExceptTransitionTo(input, toNode, transitionData);            
        }



    }


    #endregion

    
}

