using sly.lexer.fsm.transitioncheck;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sly.lexer.fsm
{

    public delegate FSMMatch<IN> NodeCallback<IN>(FSMMatch<IN> node);

    public delegate string NodeAction(string value);

    public delegate bool TransitionPrecondition(string value);
    public class FSMLexerBuilder<N>
    {

        public FSMLexer<N> Fsm { get; private set; }

        private int CurrentState;

        private Dictionary<string, int> Marks;





        public FSMLexerBuilder()
        {
            Fsm = new FSMLexer<N>();
            CurrentState = 0;
            Marks = new Dictionary<string, int>();
            Fsm.AddNode(default(N));
            Fsm.GetNode(0).IsStart = true;
        }

        #region MARKS

        public FSMLexerBuilder<N> GoTo(int state)
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
        public FSMLexerBuilder<N> GoTo(string mark)
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


        public FSMLexerBuilder<N> Mark(string mark)
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

        public FSMLexerBuilder<N> IgnoreWS()
        {
            Fsm.IgnoreWhiteSpace = true;
            return this;
        }

        public FSMLexerBuilder<N> IgnoreEOL()
        {
            Fsm.IgnoreEOL = true;
            return this;
        }



        public FSMLexerBuilder<N> WhiteSpace(char spacechar)
        {
            Fsm.WhiteSpaces.Add(spacechar);
            return this;
        }




        #endregion


        #region NODES


        public FSMLexerBuilder<N> End(N nodeValue)
        {
            if (Fsm.HasState(CurrentState))
            {
                FSMNode<N> node = Fsm.GetNode(CurrentState);

                node.IsEnd = true;
                node.Value = nodeValue;

            }
            return this;
        }

        public FSMLexerBuilder<N> CallBack(NodeCallback<N> callback)
        {
            if (Fsm.HasState(CurrentState))
            {
                Fsm.SetCallback(CurrentState, callback);
            }

            return this;
        }

        public FSMLexerBuilder<N> Action(NodeAction action) {
            if (Fsm.HasState(CurrentState))
            {
                Fsm.SetAction(CurrentState, action);
            }

            return this;
        }

        #endregion


        #region TRANSITIONS



        public FSMLexerBuilder<N> SafeTransition(char input)
        {
            var transition = Fsm.GetTransition(CurrentState, input);
            if (transition != null)
            {
                CurrentState = transition.ToNode;
            }
            else
            {
                return TransitionTo(input, Fsm.NewNodeId);
            }
            return this;
        }

        public FSMLexerBuilder<N> SafeTransition(char input, TransitionPrecondition precondition)
        {
            var transition = Fsm.GetTransition(CurrentState, input);
            if (transition != null)
            {
                CurrentState = transition.ToNode;
            }
            else
            {
                return TransitionTo(input, Fsm.NewNodeId, precondition);
            }
            return this;
        }



        public FSMLexerBuilder<N> Transition(char input)
        {
            return TransitionTo(input, Fsm.NewNodeId);
        }

        public FSMLexerBuilder<N> Transition(char input, TransitionPrecondition precondition)
        {
            return TransitionTo(input, Fsm.NewNodeId, precondition);
        }

        public FSMLexerBuilder<N> ConstantTransition(string constant, TransitionPrecondition precondition = null)
        {
            char c = constant[0];
            this.SafeTransition(c, precondition);
            for (int i = 1; i < constant.Length; i++)
            {
                c = constant[i];
                this.SafeTransition(c);
            }
            return this;
        }

        public FSMLexerBuilder<N> RepetitionTransition(int count, string pattern, TransitionPrecondition precondition = null)
        {
            if (count > 0 && !string.IsNullOrEmpty(pattern))
            {
                if (pattern.StartsWith("[") && pattern.EndsWith("]") && pattern.Contains("-") && pattern.Length == 5)
                {
                    char start = pattern[1];
                    char end = pattern[3];
                    RangeTransition(start, end, precondition);
                    for (int i = 1; i < count; i++)
                    {
                        RangeTransition(start, end);
                    }
                }
                else
                {
                    ConstantTransition(pattern, precondition);
                    for (int i = 1; i < count; i++)
                    {
                        ConstantTransition(pattern);
                    }
                    ConstantTransition(pattern, precondition);
                }
            }
            return this;
        }


        public FSMLexerBuilder<N> RangeTransition(char start, char end)
        {
            return RangeTransitionTo(start, end, Fsm.NewNodeId);
        }

        public FSMLexerBuilder<N> RangeTransition(char start, char end, TransitionPrecondition precondition)
        {
            return RangeTransitionTo(start, end, Fsm.NewNodeId, precondition);
        }


      




        public FSMLexerBuilder<N> TransitionTo(char input, int toNode)
        {
            AbstractTransitionCheck checker = new TransitionSingle(input);
            if (!Fsm.HasState(toNode))
            {
                Fsm.AddNode();
            }
            var transition = new FSMTransition(checker, CurrentState, toNode);
            Fsm.AddTransition(transition);
            CurrentState = toNode;
            return this;
        }


        public FSMLexerBuilder<N> TransitionTo(char input, int toNode, TransitionPrecondition precondition)
        {
            AbstractTransitionCheck checker = new TransitionSingle(input, precondition);
            if (!Fsm.HasState(toNode))
            {
                Fsm.AddNode();
            }
            var transition = new FSMTransition(checker, CurrentState, toNode);
            Fsm.AddTransition(transition);
            CurrentState = toNode;
            return this;
        }



        public FSMLexerBuilder<N> RangeTransitionTo(char start, char end, int toNode)
        {
            AbstractTransitionCheck checker = new TransitionRange(start, end);
            if (!Fsm.HasState(toNode))
            {
                Fsm.AddNode();
            }
            var transition = new FSMTransition(checker, CurrentState, toNode);
            Fsm.AddTransition(transition);
            CurrentState = toNode;
            return this;
        }

        public FSMLexerBuilder<N> RangeTransitionTo(char start, char end, int toNode, TransitionPrecondition precondition)
        {
            AbstractTransitionCheck checker = new TransitionRange(start, end, precondition);
            if (!Fsm.HasState(toNode))
            {
                Fsm.AddNode();
            }
            var transition = new FSMTransition(checker, CurrentState, toNode);
            Fsm.AddTransition(transition);
            CurrentState = toNode;
            return this;
        }


        public FSMLexerBuilder<N> ExceptTransitionTo(char[] exceptions, int toNode)
        {
            AbstractTransitionCheck checker = new TransitionAnyExcept(exceptions);
            if (!Fsm.HasState(toNode))
            {
                Fsm.AddNode();
            }
            var transition = new FSMTransition(checker, CurrentState, toNode);
            Fsm.AddTransition(transition);
            CurrentState = toNode;
            return this;
        }

        public FSMLexerBuilder<N> ExceptTransitionTo(char[] exceptions, int toNode, TransitionPrecondition precondition)
        {
            AbstractTransitionCheck checker = new TransitionAnyExcept(precondition, exceptions);
            if (!Fsm.HasState(toNode))
            {
                Fsm.AddNode();
            }
            var transition = new FSMTransition(checker, CurrentState, toNode);
            Fsm.AddTransition(transition);
            CurrentState = toNode;
            return this;
        }

        public FSMLexerBuilder<N> AnyTransitionTo(char input, int toNode)
        {
            AbstractTransitionCheck checker = new TransitionAny(input);
            if (!Fsm.HasState(toNode))
            {
                Fsm.AddNode();
            }
            var transition = new FSMTransition(checker, CurrentState, toNode);
            Fsm.AddTransition(transition);
            CurrentState = toNode;
            return this;
        }

        public FSMLexerBuilder<N> AnyTransitionTo(char input, int toNode, TransitionPrecondition precondition)
        {
            AbstractTransitionCheck checker = new TransitionAny(input, precondition);
            if (!Fsm.HasState(toNode))
            {
                Fsm.AddNode();
            }
            var transition = new FSMTransition(checker, CurrentState, toNode);
            Fsm.AddTransition(transition);
            CurrentState = toNode;
            return this;
        }

        public FSMLexerBuilder<N> TransitionTo(char input, string toNodeMark)
        {
            int toNode = Marks[toNodeMark];
            return TransitionTo(input, toNode);
        }



        public FSMLexerBuilder<N> RangeTransitionTo(char start, char end, string toNodeMark)
        {
            int toNode = Marks[toNodeMark];
            return RangeTransitionTo(start, end, toNode);
        }

       

        public FSMLexerBuilder<N> ExceptTransitionTo(char[] exceptions, string toNodeMark)
        {
            int toNode = Marks[toNodeMark];
            return ExceptTransitionTo(exceptions, toNode);
        }

     

        public FSMLexerBuilder<N> AnyTransitionTo(char input, string toNodeMark)
        {
            int toNode = Marks[toNodeMark];
            return AnyTransitionTo(input, toNode);
        }

    


    }


    #endregion


}

