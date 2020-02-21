using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace sly.lexer.fsm
{
    public static class MemoryExtensions
    {
        public static T At<T>(this ReadOnlyMemory<T> memory, int index)
        {
            return memory.Span[index];
        }
    }

    public delegate void BuildExtension<IN>(IN token, LexemeAttribute lexem, GenericLexer<IN> lexer) where IN : struct;

    public class FSMLexer<N>
    {
        private readonly Dictionary<int, FSMNode<N>> Nodes;

        public char StringDelimiter = '"';

        private readonly Dictionary<int, List<FSMTransition>> Transitions;

        public FSMLexer()
        {
            Nodes = new Dictionary<int, FSMNode<N>>();
            Transitions = new Dictionary<int, List<FSMTransition>>();
            Callbacks = new Dictionary<int, NodeCallback<N>>();
            IgnoreWhiteSpace = false;
            IgnoreEOL = false;
            AggregateEOL = false;
            WhiteSpaces = new List<char>();
        }

        public bool IgnoreWhiteSpace { get; set; }

        public List<char> WhiteSpaces { get; set; }

        public bool IgnoreEOL { get; set; }

        public bool AggregateEOL { get; set; }


        private Dictionary<int, NodeCallback<N>> Callbacks { get; }

        [ExcludeFromCodeCoverage]
        public string ToGraphViz()
        {
            var dump = new StringBuilder();
            foreach (var transitions in Transitions.Values)
                foreach (var transition in transitions)
                    dump.AppendLine(transition.ToGraphViz(Nodes));
            return dump.ToString();
        }

        #region accessors

        internal bool HasState(int state)
        {
            return Nodes.ContainsKey(state);
        }

        internal FSMNode<N> GetNode(int state)
        {
            Nodes.TryGetValue(state, out var node);
            return node;
        }

        internal int NewNodeId => Nodes.Count;

        internal bool HasCallback(int nodeId)
        {
            return Callbacks.ContainsKey(nodeId);
        }

        internal void SetCallback(int nodeId, NodeCallback<N> callback)
        {
            Callbacks[nodeId] = callback;
        }

        #endregion

        #region build

        public FSMTransition GetTransition(int nodeId, char token)
        {
            FSMTransition transition = null;
            if (HasState(nodeId))
                if (Transitions.ContainsKey(nodeId))
                {
                    var leavingTransitions = Transitions[nodeId];
                    transition = leavingTransitions.FirstOrDefault(t => t.Match(token));
                }

            return transition;
        }


        public void AddTransition(FSMTransition transition)
        {
            var transitions = new List<FSMTransition>();
            if (Transitions.ContainsKey(transition.FromNode)) transitions = Transitions[transition.FromNode];
            transitions.Add(transition);
            Transitions[transition.FromNode] = transitions;
        }


        public FSMNode<N> AddNode(N value)
        {
            var node = new FSMNode<N>(value);
            node.Id = Nodes.Count;
            Nodes[node.Id] = node;
            return node;
        }

        public FSMNode<N> AddNode()
        {
            var node = new FSMNode<N>(default(N));
            node.Id = Nodes.Count;
            Nodes[node.Id] = node;
            return node;
        }

        #endregion

        #region run

        public int CurrentPosition { get; private set; }
        public int CurrentColumn { get; private set; }
        public int CurrentLine { get; private set; }

        public void MovePosition(int newPosition, int newLine, int newColumn)
        {
            CurrentPosition = newPosition;
            CurrentLine = newLine;
            CurrentColumn = newColumn;
        }

        public FSMMatch<N> Run(string source)
        {
            return Run(source, CurrentPosition);
        }

        public FSMMatch<N> Run(ReadOnlyMemory<char> source)
        {
            return Run(source, CurrentPosition);
        }

        public FSMMatch<N> Run(string source, int start)
        {
            return Run(new ReadOnlyMemory<char>(source.ToCharArray()), start);
        }

        public FSMMatch<N> Run(ReadOnlyMemory<char> source, int start)
        {
            CurrentPosition = start;
            ConsumeIgnored(source);

            // End of token stream
            if (CurrentPosition >= source.Length)
            {
                return new FSMMatch<N>(false);
            }

            // Make a note of where current token starts
            var position = new TokenPosition(CurrentPosition, CurrentLine, CurrentColumn);

            FSMMatch<N> result = null;
            var currentNode = Nodes[0];
            while (CurrentPosition < source.Length)
            {
                var currentCharacter = source.At(CurrentPosition);
                var currentValue = source.Slice(position.Index, CurrentPosition - position.Index + 1);
                currentNode = Move(currentNode, currentCharacter, currentValue);
                if (currentNode == null)
                {
                    // No more viable transitions, so exit loop
                    break;
                }

                if (currentNode.IsEnd)
                {
                    // Remember the possible match
                    result = new FSMMatch<N>(true, currentNode.Value, currentValue, position, currentNode.Id);
                }

                CurrentPosition++;
                CurrentColumn++;
            }

            if (result != null)
            {
                // Backtrack
                var length = result.Result.Value.Length;
                CurrentPosition = result.Result.Position.Index + length;
                CurrentColumn = result.Result.Position.Column + length;

                if (HasCallback(result.NodeId))
                {
                    result = Callbacks[result.NodeId](result);
                }

                return result;
            }

            if (CurrentPosition >= source.Length)
            {
                // Failed on last character, so need to backtrack
                CurrentPosition -= 1;
                CurrentColumn -= 1;
            }

            var errorChar = source.Slice(CurrentPosition, 1);
            var errorPosition = new TokenPosition(CurrentPosition, CurrentLine, CurrentColumn);
            var ko = new FSMMatch<N>(false, default(N), errorChar, errorPosition, -1);
            return ko;
        }

        private FSMNode<N> Move(FSMNode<N> from, char token, ReadOnlyMemory<char> value)
        {
            if (from != null && Transitions.TryGetValue(from.Id, out var transitions))
            {
                // Do NOT use Linq, increases allocations AND running time
                for (var i = 0; i < transitions.Count; ++i)
                {
                    var transition = transitions[i];
                    if (transition.Match(token, value))
                    {
                        return Nodes[transition.ToNode];
                    }
                }
            }

            return null;
        }

        private void ConsumeIgnored(ReadOnlyMemory<char> source)
        {
            while (CurrentPosition < source.Length)
            {
                if (IgnoreWhiteSpace)
                {
                    var currentCharacter = source.At(CurrentPosition);
                    if (WhiteSpaces.Contains(currentCharacter))
                    {
                        CurrentPosition++;
                        CurrentColumn++;
                        continue;
                    }
                }

                if (IgnoreEOL)
                {
                    var eol = EOLManager.IsEndOfLine(source, CurrentPosition);
                    if (eol != EOLType.No)
                    {
                        CurrentPosition += eol == EOLType.Windows ? 2 : 1;
                        CurrentColumn = 0;
                        CurrentLine++;
                        continue;
                    }
                }

                break;
            }
        }

        #endregion
        
        public void Reset()
        {
            CurrentColumn = 0;
            CurrentLine = 0;
            CurrentPosition = 0;
        }
    }
}