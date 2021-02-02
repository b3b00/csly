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
        
        public bool IndentationAware { get; set; }
        
        public string Indentation { get; set; }


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


        public int ComputeIndentationSize(ReadOnlyMemory<char> source, int index)
        {
            int count = 0;
            if (index + Indentation.Length > source.Length)
            {
                return 0;
            }
            string id = source.Slice(index, Indentation.Length).ToString();
            while (id == Indentation)
            {
                count++;
                index += Indentation.Length;
                id = source.Slice(index, Indentation.Length).ToString();
            }
            return count;
        }

        public List<char> GetIndentations(ReadOnlyMemory<char> source, int index)
        {
            List<Char> indentations = new List<char>();
            int i = 0;
            if (index + i >= source.Length)
            {
                return new List<char>();
                ;
            }
            char current = source.At(index+i);
            while (i < source.Length && (current == ' ' || current == '\t' ))
            {
                indentations.Add(current);
                i++;
                current = source.At(index+i);
            }

            return indentations;
        }
        
      
        
        public FSMMatch<N> Run(ReadOnlyMemory<char> source, LexerPosition lexerPosition)
        {

            if (IndentationAware)
            {
                var ind = ConsumeIndents(source, lexerPosition); 
                if (ind != null)
                {
                    return ind;
                }
            }
            // TODO here : indented language  
            // if line start :
            
            // consume tabs and count them
            // if count = previousCount +1 => add an indent token
            // if count = previousCount -1 => add an unindent token
            // else ....
            
            ConsumeIgnored(source,lexerPosition);

            if (IndentationAware) // could start of line
            {
                var ind = ConsumeIndents(source, lexerPosition);
                if (ind != null)
                {
                    return ind;
                }
            }

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
                    result = new FSMMatch<N>(true, currentNode.Value, currentValue, position, currentNode.Id,lexerPosition, currentNode.IsLineEnding);
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
            var ko = new FSMMatch<N>(false, default(N), errorChar, lexerPosition, -1,lexerPosition, false);
            return ko;
        }

        private FSMMatch<N> ConsumeIndents(ReadOnlyMemory<char> source, LexerPosition lexerPosition)
        {
            
            if (lexerPosition.IsStartOfLine)
            {
                var indents = GetIndentations(source, lexerPosition.Index);
                
                var currentIndentations = lexerPosition.Indentations.ToList();

                int uIndentCount = 0;
                
                int indentPosition = 0;
                if (currentIndentations.Any())
                {
                    int i = 0;
                    int indentCharCount = 0;
                    while (i < currentIndentations.Count && indentPosition < indents.Count)
                    {
                        var current = currentIndentations[currentIndentations.Count-i-1];
                        bool ok = true;
                        int j = 0;
                        if (indentPosition + current.Length > indents.Count)
                        {
                            // TODO : erreur d'indentation
                            var ko = new FSMMatch<N>(false, default(N), " ", lexerPosition, -1, lexerPosition, false)
                            {
                                IsIndentationError = true
                            };
                            return ko;
                        }
                        else
                        {
                            while (j < current.Length && indentPosition + j < indents.Count)
                            {
                                var reference = current[j];
                                var actual = indents[j + indentPosition];
                                if (actual != reference)
                                {
                                    var ko = new FSMMatch<N>(false, default(N), " ", lexerPosition, -1, lexerPosition, false)
                                    {
                                        IsIndentationError = true
                                    };
                                    return ko;
                                }

                                indentCharCount++;

                                j++;
                            }
                        }

                        if (indentCharCount < indents.Count)
                        {
                            var t = indents.Skip(indentCharCount).ToArray();
                            var newTab = new string(t);
                            var indent = FSMMatch<N>.Indent(lexerPosition.Indentations.Count()+1);
                            indent.NewPosition = lexerPosition.Clone();
                            indent.NewPosition.Index += indents.Count;
                            indent.NewPosition.Column += indents.Count;
                            indent.NewPosition.Indentations = indent.NewPosition.Indentations.Push(newTab);
                            return indent;
                            
                        }
                        indentPosition += current.Length;
                        i++;
                    }

                    if (i < currentIndentations.Count)
                    {
                        // TODO : UINDENT : combien ?
                        uIndentCount = currentIndentations.Count - i;

                        // 
                        currentIndentations.Reverse();
                        var unindented = currentIndentations.Take(i).ToList();
                        var spaces = unindented.Select(x => x.Length).Sum();
                        
                        var uIndent = FSMMatch<N>.UIndent(uIndentCount,uIndentCount);
                        uIndent.NewPosition = lexerPosition.Clone();
                        uIndent.NewPosition.Index += spaces;
                        uIndent.NewPosition.Column += spaces;
                        uIndent.NewPosition.CurrentIndentation = i;
                        for (int iu = 0; iu < uIndentCount; iu++)
                        {
                            uIndent.NewPosition.Indentations = uIndent.NewPosition.Indentations.Pop();
                        }
                        return uIndent;
                    }
                    else
                    {
                        if (i == currentIndentations.Count)
                        {
                            lexerPosition.Column += indents.Count;
                            lexerPosition.Index += indents.Count;
                            return null;
                        }
                    }
                }
                else
                {
                    if (indents.Any())
                    {
                        var indent = FSMMatch<N>.Indent(1);
                        indent.NewPosition = lexerPosition.Clone();
                        indent.NewPosition.Index += indents.Count;
                        indent.NewPosition.Column += indents.Count;
                        indent.NewPosition.Indentations = indent.NewPosition.Indentations.Push(new string(indents.ToArray()));
                        return indent;
                    }
                    ;
                    return null;
                }
            }

            return null;
        }

        private FSMNode<N> Move(FSMNode<N> from, char token, ReadOnlyMemory<char> value)
        {
            if (from != null && Transitions.TryGetValue(from.Id, out var transitions))
            {
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

        public FSMNode<N> GetNext(int from, char token)
        {
            var node = Nodes[from];
            
            return Move(node, token, "".AsMemory());
        }

        private void ConsumeIgnored(ReadOnlyMemory<char> source, LexerPosition position)
        {

            bool eolReached = false;
            while (position.Index < source.Length && !(eolReached && IndentationAware))
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
                        eolReached = true;
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }

                break;
            }
        }

        #endregion
        
    }
}