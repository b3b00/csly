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

    public delegate List<Token<IN>> LexerPostProcess<IN>(List<Token<IN>> tokens) where IN : struct;

    public class FSMLexer<N> where N : struct
    {
        public string Mode { get; set; }

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
            WhiteSpaces = new List<char>();
        }

        public bool IgnoreWhiteSpace { get; set; }

        public List<char> WhiteSpaces { get; set; }

        public bool IgnoreEOL { get; set; }

        public bool IndentationAware { get; set; }


        private Dictionary<int, NodeCallback<N>> Callbacks { get; }

        [ExcludeFromCodeCoverage]
        public string ToGraphViz()
        {
            var dump = new StringBuilder();
            dump.AppendLine($"digraph {Mode} {{");

            foreach (var fsmNode in Nodes)
            {
                dump.Append(fsmNode.Value.Id);
                var shape = fsmNode.Value.IsEnd ? "doublecircle" : "circle";
                dump.AppendLine($@"[shape={shape} label=""{fsmNode.Value.GraphVizNodeLabel()}""] ");
            }

            foreach (var transition in Transitions.Values.SelectMany<List<FSMTransition>, FSMTransition>(x => x))
                dump.AppendLine(transition.ToGraphViz<N>(Nodes));

            dump.AppendLine("}");
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
        public char DecimalSeparator { get; set; }

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
            if (!HasState(nodeId)) return null;
            if (Transitions.TryGetValue(nodeId, out var leavingTransitions))
            {
                transition = leavingTransitions.Find(t => t.Match(token));
            }

            return transition;
        }

        public void AddTransition(FSMTransition transition)
        {
            var transitions = new List<FSMTransition>();
            if (Transitions.TryGetValue(transition.FromNode, out var transition1)) transitions = transition1;
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

        public List<char> GetIndentations(ReadOnlyMemory<char> source, int index)
        {
            List<Char> indentations = new List<char>();
            int i = 0;
            if (index >= source.Length)
            {
                return new List<char>();
            }

            char current = source.At<char>(index + i);
            while (i < source.Length && (current == ' ' || current == '\t'))
            {
                indentations.Add(current);
                i++;
                current = source.At<char>(index + i);
            }

            return indentations;
        }


        
        
        public FSMMatch<N> Run(ReadOnlyMemory<char> source, LexerPosition lexerPosition)
        {

            
            
            if (IndentationAware)
            {
                var ind = ConsumeIndents3(source, lexerPosition);
                if (ind != null && !ind.IsNoIndent)
                {
                    return ind;
                }

                if (ind != null && ind.IsNoIndent)
                {
                    if (lexerPosition != ind.NewPosition)
                    {
                        return ind;
                    }
                    lexerPosition = ind.NewPosition;
                }
            }

            var ignoredTokens = ConsumeIgnored(source, lexerPosition);

            if (IndentationAware) // could start of line
            {
                var ind = ConsumeIndents3(source, lexerPosition);
                if (ind != null && !ind.IsNoIndent)
                {
                    return ind;
                }

                if (ind != null && ind.IsNoIndent)
                {
                    if (lexerPosition != ind.NewPosition)
                    {
                        return ind;
                    }
                    lexerPosition = ind.NewPosition;
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
                var currentCharacter = source.At<char>(lexerPosition);
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
                    lexerPosition.Mode = this.Mode;
                    result = new FSMMatch<N>(true, currentNode.Value, currentValue, position, currentNode.Id,
                        lexerPosition, currentNode.IsLineEnding, currentNode.IsPopModeNode, currentNode.IsPushModeNode,
                        currentNode.PushToMode, DecimalSeparator);
                }

                lexerPosition.Index++;
                lexerPosition.Column++;
            }

            if (result != null)
            {
                if ((result.Result as Token<GenericToken>).TokenID == GenericToken.UpTo)
                {
                    if (ignoredTokens.Count > 0)
                    {
                        int ignoredLength = ignoredTokens.Select(x => x.SpanValue.Length).Sum();
                        int start = result.Result.Position.Index - ignoredLength; 
                        result.Result.Position.Index  -= ignoredLength;
                        var value = source.Slice(start, ignoredLength+result.Result.SpanValue.Length);
                        result.Result.SpanValue = value;
                    }
                }
                // Backtrack
                var length = result.Result.Value.Length;
                lexerPosition.Index = result.Result.Position.Index + length;
                lexerPosition.Column = result.Result.Position.Column + length;
                result.IgnoredTokens = ignoredTokens;

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
            var ko = new FSMMatch<N>(false, default(N), errorChar, lexerPosition, -1, lexerPosition, false);
            return ko;
        }

        private FSMMatch<N> ConsumeIndents3(ReadOnlyMemory<char> source, LexerPosition lexerPosition)
        {
            if (lexerPosition.IsStartOfLine)
            {
                var shifts = GetIndentations(source, lexerPosition.Index);
                string currentShift = string.Join("", shifts);
                lexerPosition.Indentation = lexerPosition.Indentation ?? new LexerIndentation();
                var indentation = lexerPosition.Indentation.Indent(currentShift);
                switch (indentation.type)
                {
                    case LexerIndentationType.Indent:
                    {
                        var position = lexerPosition.Clone();
                        position.IsPush = false;
                        position.IsPop = false;
                        position.Mode = null;
                        var indent = FSMMatch<N>.Indent(lexerPosition.Indentation.CurrentLevel);
                        indent.Result = new Token<N>
                        {
                            IsIndent = true,
                            IsUnIndent = false,
                            IsNoIndent = false,
                            Position = position
                        };
                        indent.IsNoIndent = false;
                        indent.IsIndent = true;
                        indent.IsUnIndent = false;
                        indent.NewPosition = position;
                        indent.NewPosition.Index += currentShift.Length;
                        indent.NewPosition.Column += currentShift.Length;
                        return indent;
                    }
                    case LexerIndentationType.UIndent:
                    {
                        var uIndent = FSMMatch<N>.UIndent(lexerPosition.Indentation.CurrentLevel);
                        var position = lexerPosition.Clone();
                        position.IsPush = false;
                        position.IsPop = false;
                        position.Mode = null;
                        uIndent.Result = new Token<N>
                        {
                            IsIndent = false,
                            IsUnIndent = true,
                            IsNoIndent = false,
                            Position = position
                        };
                        uIndent.IsNoIndent = false;
                        uIndent.IsIndent = false;
                        uIndent.IsUnIndent = true;
                        uIndent.NewPosition = position;
                        uIndent.NewPosition.Index += currentShift.Length;
                        uIndent.NewPosition.Column += currentShift.Length;
                        return uIndent;
                    }
                    case LexerIndentationType.None:
                    {
                        var noIndent = FSMMatch<N>.Indent(lexerPosition.Indentation.CurrentLevel);
                        noIndent.IsNoIndent = true;
                        noIndent.IsIndent = false;
                        noIndent.IsUnIndent = false;
                        noIndent.NewPosition = lexerPosition.Clone();
                        noIndent.NewPosition.Index += currentShift.Length;
                        noIndent.NewPosition.Column += currentShift.Length;
                        return noIndent;
                    }
                    case LexerIndentationType.Error:
                    {
                        var ko = new FSMMatch<N>(false, default(N), " ", lexerPosition, -1, lexerPosition,
                            false)
                        {
                            IsNoIndent = false,
                            IsIndentationError = true
                        };
                        return ko;
                    }
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

        /// <summary>
        /// read source and consume ignored characters. position parameter is modified to shift the lexer position after
        /// all ignored characters 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="position">current lexer position. modified</param>
        /// <returns>list of ignored tokens : 1 token for each ignored char.</returns>
        private List<Token<N>> ConsumeIgnored(ReadOnlyMemory<char> source, LexerPosition position)
        {
            bool eolReached = false;
            List<Token<N>> ignoredTokens = new List<Token<N>>();
            while (position.Index < source.Length && !(eolReached && IndentationAware))
            {
                if (IgnoreWhiteSpace)
                {
                    var currentCharacter = source.At<char>(position.Index);
                    if (WhiteSpaces.Contains(currentCharacter))
                    {
                        var whiteToken = new Token<N>(default(N), source.Slice(position.Index, 1), position,
                            CommentType.No,
                            Channels.WhiteSpaces, isWhiteSpace: true, decimalSeparator: DecimalSeparator);
                        ignoredTokens.Add(whiteToken);
                        whiteToken.IsWhiteSpace = true;
                        position.Index++;
                        position.Column++;
                        continue;
                    }
                }

                if (IgnoreEOL)
                {
                    var eol = EOLManager.IsEndOfLine(source, position.Index);
                    if (eol != EolType.No)
                    {
                        position.Index += eol == EolType.Windows ? 2 : 1;
                        position.Column = 0;
                        position.Line++;
                        eolReached = true;
                        while (position.Index < source.Length && eol != EolType.No)
                        {
                            eol = EOLManager.IsEndOfLine(source, position.Index);
                            if (eol != EolType.No)
                            {
                                position.Index += eol == EolType.Windows ? 2 : 1;
                                position.Column = 0;
                                position.Line++;
                            } 
                        }
                        continue;
                    }
                }

                break;
            }

            return ignoredTokens;
        }

        #endregion
    }
}