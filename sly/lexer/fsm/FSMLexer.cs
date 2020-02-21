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
        
        public static T At<T>(this ReadOnlyMemory<T> memory, LexerPosition position)
        {
            return memory.Span[position.Index];
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

        
        public FSMMatch<N> Run(string source, LexerPosition position)
        {
            return Run(new ReadOnlyMemory<char>(source.ToCharArray()), position);
        }

        public FSMMatch<N> Run(ReadOnlyMemory<char> source, LexerPosition lexerPosition)
        {
            ConsumeIgnored(source,lexerPosition);

            // End of token stream
            if (lexerPosition.Index >= source.Length)
            {
                return new FSMMatch<N>(false);
            }

            // Make a note of where current token starts
            var position = lexerPosition.Clone();

            FSMMatch<N> result = null;
            var currentNode = Nodes[0];
            while (lexerPosition.Index < source.Length)
            {
                var currentCharacter = source.At(lexerPosition);
                var currentValue = source.Slice(position.Index, lexerPosition.Index - position.Index + 1);
                currentNode = Move(currentNode, currentCharacter, currentValue);
                if (currentNode == null)
                {
                    // No more viable transitions, so exit loop
                    break;
                }

                if (currentNode.IsEnd)
                {
                    // Remember the possible match
                    result = new FSMMatch<N>(true, currentNode.Value, currentValue, position, currentNode.Id,lexerPosition);
                }

                lexerPosition.Index++;
                lexerPosition.Column++;
            }

            if (result != null)
            {
                // Backtrack
                var length = result.Result.Value.Length;
                lexerPosition.Index = result.Result.Position.Index + length;
                lexerPosition.Column = result.Result.Position.Column + length;

                if (HasCallback(result.NodeId))
                {
                    result = Callbacks[result.NodeId](result);
                }

                return result;
            }

            if (lexerPosition.Index >= source.Length)
            {
                // Failed on last character, so need to backtrack
                lexerPosition.Index -= 1;
                lexerPosition.Column -= 1;
            }

            var errorChar = source.Slice(lexerPosition.Index, 1);
            var ko = new FSMMatch<N>(false, default(N), errorChar, lexerPosition, -1,lexerPosition);
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

        private void ConsumeIgnored(ReadOnlyMemory<char> source, LexerPosition position)
        {
            
            while (position.Index < source.Length)
            {
                if (IgnoreWhiteSpace)
                {
                    var currentCharacter = source.At(position.Index);
                    if (WhiteSpaces.Contains(currentCharacter))
                    {
                        position.Index++;
                        position.Column++;
                        continue;
                    }
                }

                if (IgnoreEOL)
                {
                    var eol = EOLManager.IsEndOfLine(source, position.Index);
                    if (eol != EOLType.No)
                    {
                        position.Index += eol == EOLType.Windows ? 2 : 1;
                        position.Column = 0;
                        position.Line++;
                        continue;
                    }
                }

                break;
            }
        }

        #endregion
        
    }
}