using System;
using System.Collections.Generic;
using System.Linq;
using sly.lexer.fsm.transitioncheck;

namespace sly.lexer.fsm
{
    public delegate FSMMatch<IN> NodeCallback<IN>(FSMMatch<IN> node)  where IN : struct;

    public delegate bool TransitionPrecondition(ReadOnlyMemory<char> value);

    public class FSMLexerBuilder<N>  where N : struct
    {
        private int CurrentState;

        internal readonly Dictionary<string, int> Marks;


        public FSMLexerBuilder()
        {
            Fsm = new FSMLexer<N>();
            CurrentState = 0;
            Marks = new Dictionary<string, int>();
            Fsm.AddNode(default(N));
        }

        public FSMLexer<N> Fsm { get; }

        #region MARKS

        public FSMLexerBuilder<N> GoTo(int state)
        {
            if (Fsm.HasState(state))
                CurrentState = state;
            else
                throw new ArgumentException($"state {state} does not exist in lexer FSM");
            return this;
        }

        public FSMLexerBuilder<N> GoTo(string mark)
        {
            if (Marks.TryGetValue(mark, out var mark1))
                GoTo(mark1);
            else
                throw new ArgumentException($"mark {mark} does not exist in current builder");
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
            if (Marks.TryGetValue(mark, out var mark1)) node = GetNode(mark1);
            return node;
        }
        
        #endregion

        #region special chars

        public FSMLexerBuilder<N> IgnoreWS(bool ignore = true)
        {
            Fsm.IgnoreWhiteSpace = ignore;
            return this;
        }

        public FSMLexerBuilder<N> IgnoreEOL(bool ignore = true)
        {
            Fsm.IgnoreEOL = ignore;
            return this;
        }

        public FSMLexerBuilder<N> WhiteSpace(params char[] spaceChars)
        {
            if (spaceChars != null)
            {
                foreach (var spaceChar in spaceChars)
                {
                    Fsm.WhiteSpaces.Add(spaceChar);
                }
            }

            return this;
        }
        
        public FSMLexerBuilder<N> Indentation(bool indentAware, string indentation)
        {
            Fsm.IndentationAware = indentAware;
            return this;
        }

        #endregion


        #region NODES

        public FSMLexerBuilder<N> End(N nodeValue, bool isLineEnding = false)
        {
            if (Fsm.HasState(CurrentState))
            {
                var node = Fsm.GetNode(CurrentState);

                node.IsEnd = true;
                node.Value = nodeValue;
                node.IsLineEnding = isLineEnding;
            }

            return this;
        }
        
        
        public FSMLexerBuilder<N> Push(string toMode)
        {
            if (Fsm.HasState(CurrentState))
            {
                var node = Fsm.GetNode(CurrentState);

                node.IsPushModeNode = true;
                node.PushToMode = toMode;
            }

            return this;
        }
        
        
        public FSMLexerBuilder<N> Pop()
        {
            if (Fsm.HasState(CurrentState))
            {
                var node = Fsm.GetNode(CurrentState);

                node.IsPopModeNode = true;
            }

            return this;
        }

        public FSMLexerBuilder<N> CallBack(NodeCallback<N> callback)
        {
            if (Fsm.HasState(CurrentState)) Fsm.SetCallback(CurrentState, callback);

            return this;
        }

      

        #endregion


        #region TRANSITIONS

        public FSMLexerBuilder<N> SafeTransition(char input)
        {
            var transition = Fsm.GetTransition(CurrentState, input);
            if (transition != null)
                CurrentState = transition.ToNode;
            else
                return TransitionTo(input, Fsm.NewNodeId);
            return this;
        }

        public FSMTransition GetTransition(char input)
        {
            var transition = Fsm.GetTransition(CurrentState, input);
            return transition;
        }

        public FSMLexerBuilder<N> SafeTransition(char input, TransitionPrecondition precondition)
        {
            var transition = Fsm.GetTransition(CurrentState, input);
            if (transition != null)
                CurrentState = transition.ToNode;
            else
                return TransitionTo(input, Fsm.NewNodeId, precondition);
            return this;
        }


        public FSMLexerBuilder<N> Transition(char input)
        {
            var next = Fsm.GetNext(CurrentState, input);
            if (next == null)
            {
                return TransitionTo(input, Fsm.NewNodeId);
            }
            CurrentState = next.Id;
            return this;
        }

        
        
        public FSMLexerBuilder<N> Transition(char[] inputs)
        {
            return TransitionTo(inputs, Fsm.NewNodeId);
        }

        public FSMLexerBuilder<N> Transition(char input, TransitionPrecondition precondition)
        {
            return TransitionTo(input, Fsm.NewNodeId, precondition);
        }
        
        public FSMLexerBuilder<N> Transition(char[] inputs, TransitionPrecondition precondition)
        {
            return TransitionTo(inputs, Fsm.NewNodeId, precondition);
        }

        public FSMLexerBuilder<N> ConstantTransition(string constant, TransitionPrecondition precondition = null)
        {
            var c = constant[0];
            SafeTransition(c, precondition);
            for (var i = 1; i < constant.Length; i++)
            {
                c = constant[i];
                SafeTransition(c);
            }

            return this;
        }


        private (string constant, List<(char start, char end)> ranges) ParseRepeatedPattern(string pattern)
        {
            string toParse = pattern;
            if (toParse.StartsWith("[") && toParse.EndsWith("]"))
            {
                bool isPattern = true;
                List<(char start, char end)> ranges = new List<(char start, char end)>();
                toParse = toParse.Substring(1, toParse.Length - 2);
                var rangesItems = toParse.Split(new char[]{','});
                int i = 0;
                while (i < rangesItems.Length && isPattern)
                {
                    var item = rangesItems[i];
                    isPattern = item.Length == 3 && item[1] == '-';
                    if (isPattern)
                    {
                        ranges.Add((item[0],item[2]));
                    }
                    i++;
                }

                if (isPattern)
                {
                    return (null, ranges);
                }
                
            }
            return (pattern, null);
        }
        
        public FSMLexerBuilder<N> RepetitionTransition(int count, string pattern,
            TransitionPrecondition precondition = null)
        {
            var parsedPattern = ParseRepeatedPattern(pattern);
            
            if (count > 0 && !string.IsNullOrEmpty(pattern))
            {
                if (parsedPattern.ranges != null && parsedPattern.ranges.Any<(char start, char end)>())
                {
                    for (int i = 0; i < count; i++)
                    {
                        MultiRangeTransition(precondition, parsedPattern.ranges.ToArray());
                    }
                }
                else
                {
                    ConstantTransition(pattern, precondition);
                    for (var i = 1; i < count; i++) ConstantTransition(pattern);
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

        public FSMLexerBuilder<N> MultiRangeTransition(params (char start, char end)[] ranges)
        {
            return MultiRangeTransitionTo(Fsm.NewNodeId,ranges);
        }

        public FSMLexerBuilder<N> MultiRangeTransition(TransitionPrecondition precondition , params (char start, char end)[] ranges)
        {
            return MultiRangeTransitionTo(Fsm.NewNodeId, precondition, ranges);
        }
        
        

        public FSMLexerBuilder<N> ExceptTransition(char[] exceptions)
        {
            return ExceptTransitionTo(exceptions, Fsm.NewNodeId);
        }

        public FSMLexerBuilder<N> ExceptTransition(char[] exceptions, TransitionPrecondition precondition)
        {
            return ExceptTransitionTo(exceptions, Fsm.NewNodeId, precondition);
        }

        public FSMLexerBuilder<N> AnyTransition()
        {
            return AnyTransitionTo(Fsm.NewNodeId);
        }

        public FSMLexerBuilder<N> AnyTransition(TransitionPrecondition precondition)
        {
            return AnyTransitionTo(Fsm.NewNodeId, precondition);
        }

        #endregion
        
        #region DIRECTED TRANSITIONS

        public FSMLexerBuilder<N> TransitionTo(char input, int toNode)
        {
            AbstractTransitionCheck checker = new TransitionSingle(input);
            if (!Fsm.HasState(toNode)) Fsm.AddNode();
            var transition = new FSMTransition(checker, CurrentState, toNode);
            Fsm.AddTransition(transition);
            CurrentState = toNode;
            return this;
        }
        
        public FSMLexerBuilder<N> TransitionTo(char[] inputs, int toNode)
        {
            AbstractTransitionCheck checker = new TransitionMany(inputs);
            if (!Fsm.HasState(toNode)) Fsm.AddNode();
            var transition = new FSMTransition(checker, CurrentState, toNode);
            Fsm.AddTransition(transition);
            CurrentState = toNode;
            return this;
        }


        public FSMLexerBuilder<N> TransitionTo(char input, int toNode, TransitionPrecondition precondition)
        {
            AbstractTransitionCheck checker = new TransitionSingle(input, precondition);
            if (!Fsm.HasState(toNode)) Fsm.AddNode();
            var transition = new FSMTransition(checker, CurrentState, toNode);
            Fsm.AddTransition(transition);
            CurrentState = toNode;
            return this;
        }
        
        public FSMLexerBuilder<N> TransitionTo(char[] inputs, int toNode, TransitionPrecondition precondition)
        {
            AbstractTransitionCheck checker = new TransitionMany(inputs, precondition);
            if (!Fsm.HasState(toNode)) Fsm.AddNode();
            var transition = new FSMTransition(checker, CurrentState, toNode);
            Fsm.AddTransition(transition);
            CurrentState = toNode;
            return this;
        }

        public FSMLexerBuilder<N> RepetitionTransitionTo(string toNodeMark, int count, string pattern,
            TransitionPrecondition precondition = null)
        {
            var toNode = Marks[toNodeMark];
            return RepetitionTransitionTo(toNode, count,pattern,precondition);
        }

        public FSMLexerBuilder<N> RepetitionTransitionTo(int toNode, int count, string pattern,
            TransitionPrecondition precondition = null)
        {
            var parsedPattern = ParseRepeatedPattern(pattern);
            
            if (count > 0 && !string.IsNullOrEmpty(pattern))
            {
                if (parsedPattern.ranges != null && parsedPattern.ranges.Any<(char start, char end)>())
                {
                    for (int i = 0; i < count-1; i++)
                    {
                        MultiRangeTransition(precondition, parsedPattern.ranges.ToArray());
                    }
                    MultiRangeTransitionTo(toNode, precondition, parsedPattern.ranges.ToArray());
                }
                else
                {
                    ConstantTransition(pattern, precondition);
                    for (var i = 1; i < count; i++) ConstantTransition(pattern);
                    ConstantTransition(pattern, precondition);
                }
            }

            return this;
        }
        
        

        public FSMLexerBuilder<N> RangeTransitionTo(char start, char end, int toNode)
        {
            AbstractTransitionCheck checker = new TransitionRange(start, end);
            if (!Fsm.HasState(toNode)) Fsm.AddNode();
            var transition = new FSMTransition(checker, CurrentState, toNode);
            Fsm.AddTransition(transition);
            CurrentState = toNode;
            return this;
        }

        public FSMLexerBuilder<N> RangeTransitionTo(char start, char end, int toNode,
            TransitionPrecondition precondition)
        {
            AbstractTransitionCheck checker = new TransitionRange(start, end, precondition);
            if (!Fsm.HasState(toNode)) Fsm.AddNode();
            var transition = new FSMTransition(checker, CurrentState, toNode);
            Fsm.AddTransition(transition);
            CurrentState = toNode;
            return this;
        }
        
        #region multi range directed
        
        public FSMLexerBuilder<N> MultiRangeTransitionTo(int toNode, params (char start, char end)[] ranges)
        {
            AbstractTransitionCheck checker = new TransitionMultiRange(ranges);
            if (!Fsm.HasState(toNode)) Fsm.AddNode();
            var transition = new FSMTransition(checker, CurrentState, toNode);
            Fsm.AddTransition(transition);
            CurrentState = toNode;
            return this;
        }

        public FSMLexerBuilder<N> MultiRangeTransitionTo(int toNode,
            TransitionPrecondition precondition, params (char start, char end)[] ranges)
        {
            AbstractTransitionCheck checker = new TransitionMultiRange(precondition, ranges);
            if (!Fsm.HasState(toNode)) Fsm.AddNode();
            var transition = new FSMTransition(checker, CurrentState, toNode);
            Fsm.AddTransition(transition);
            CurrentState = toNode;
            return this;
        }
        
        public FSMLexerBuilder<N> MultiRangeTransitionTo(string toNodeMark, params (char start, char end)[] ranges)
        {
            var toNode = Marks[toNodeMark];
            return MultiRangeTransitionTo(toNode,ranges);
        }

        
        #endregion


        public FSMLexerBuilder<N> ExceptTransitionTo(char[] exceptions, int toNode)
        {
            AbstractTransitionCheck checker = new TransitionAnyExcept(exceptions);
            if (!Fsm.HasState(toNode)) Fsm.AddNode();
            var transition = new FSMTransition(checker, CurrentState, toNode);
            Fsm.AddTransition(transition);
            CurrentState = toNode;
            return this;
        }

        public FSMLexerBuilder<N> ExceptTransitionTo(char[] exceptions, int toNode, TransitionPrecondition precondition)
        {
            AbstractTransitionCheck checker = new TransitionAnyExcept(precondition, exceptions);
            if (!Fsm.HasState(toNode)) Fsm.AddNode();
            var transition = new FSMTransition(checker, CurrentState, toNode);
            Fsm.AddTransition(transition);
            CurrentState = toNode;
            return this;
        }

        public FSMLexerBuilder<N> AnyTransitionTo(int toNode)
        {
            AbstractTransitionCheck checker = new TransitionAny();
            if (!Fsm.HasState(toNode)) Fsm.AddNode();
            var transition = new FSMTransition(checker, CurrentState, toNode);
            Fsm.AddTransition(transition);
            CurrentState = toNode;
            return this;
        }

        public FSMLexerBuilder<N> AnyTransitionTo(int toNode, TransitionPrecondition precondition)
        {
            AbstractTransitionCheck checker = new TransitionAny(precondition);
            if (!Fsm.HasState(toNode)) Fsm.AddNode();
            var transition = new FSMTransition(checker, CurrentState, toNode);
            Fsm.AddTransition(transition);
            CurrentState = toNode;
            return this;
        }

        public FSMLexerBuilder<N> TransitionTo(char input, string toNodeMark)
        {
            var toNode = Marks[toNodeMark];
            return TransitionTo(input, toNode);
        }
        
        public FSMLexerBuilder<N> TransitionTo(char[] inputs, string toNodeMark)
        {
            var toNode = Marks[toNodeMark];
            return TransitionTo(inputs, toNode);
        }
        
        public FSMLexerBuilder<N> TransitionToAndMark(char[] inputs, string toNodeMark)
        {
            int toNode = -1;
            if (Marks.TryGetValue(toNodeMark, out toNode))
            {
                TransitionTo(inputs, toNode);
            }
            else
            {
                Transition(inputs);
                Mark(toNodeMark);
            }

            return this;
        }


        public FSMLexerBuilder<N> TransitionTo(char input, string toNodeMark, TransitionPrecondition precondition)
        {
            var toNode = Marks[toNodeMark];
            return TransitionTo(input, toNode, precondition);
        }
        
        public FSMLexerBuilder<N> TransitionTo(char[] inputs, string toNodeMark, TransitionPrecondition precondition)
        {
            var toNode = Marks[toNodeMark];
            return TransitionTo(inputs, toNode, precondition);
        }

        public FSMLexerBuilder<N> RangeTransitionTo(char start, char end, string toNodeMark)
        {
            var toNode = Marks[toNodeMark];
            return RangeTransitionTo(start, end, toNode);
        }

        public FSMLexerBuilder<N> RangeTransitionTo(char start, char end, string toNodeMark,
            TransitionPrecondition precondition)
        {
            var toNode = Marks[toNodeMark];
            return RangeTransitionTo(start, end, toNode, precondition);
        }

        public FSMLexerBuilder<N> ExceptTransitionTo(char[] exceptions, string toNodeMark)
        {
            var toNode = Marks[toNodeMark];
            return ExceptTransitionTo(exceptions, toNode);
        }
      
        public FSMLexerBuilder<N> ExceptTransitionTo(char[] exceptions, string toNodeMark,
            TransitionPrecondition precondition)
        {
            var toNode = Marks[toNodeMark];
            return ExceptTransitionTo(exceptions, toNode, precondition);
        }

        public FSMLexerBuilder<N> AnyTransitionTo(string toNodeMark)
        {
            var toNode = Marks[toNodeMark];
            return AnyTransitionTo(toNode);
        }

        public FSMLexerBuilder<N> AnyTransitionTo(string toNodeMark, TransitionPrecondition precondition)
        {
            var toNode = Marks[toNodeMark];
            return AnyTransitionTo(toNode, precondition);
        }
        
        #endregion
    }

    
}
