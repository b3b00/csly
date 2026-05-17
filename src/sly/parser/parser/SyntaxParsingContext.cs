using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.ObjectPool;
using sly.lexer;
using sly.parser.syntax.grammar;
using sly.parser.syntax.tree;

namespace sly.parser
{
    
    public class SyntaxParsingContext<IN, OUT> where IN : struct, Enum
    {
        private readonly Dictionary<string, SyntaxParseResult<IN, OUT>> _memoizedNonTerminalResults = new Dictionary<string, SyntaxParseResult<IN, OUT>>();

        private readonly ObjectPool<ParseResult<IN, OUT>> results;
        
        private readonly ObjectPool<SyntaxLeaf<IN, OUT>> _leafs;
        
        private readonly ObjectPool<SyntaxNode<IN, OUT>> _nodes;
        
        private readonly ObjectPool<ManySyntaxNode<IN, OUT>> _manyNodes;
        
        private readonly ObjectPool<GroupSyntaxNode<IN, OUT>> _groupNodes;
        
        private readonly ObjectPool<OptionSyntaxNode<IN, OUT>> _optionNodes;

        private readonly bool _useMemoization = false;

        private readonly bool _usePool = false;
        public SyntaxParsingContext(bool useMemoization, bool usePool)
        {
            _useMemoization = useMemoization;
            _usePool = usePool;
        }

        private string GetKey(IClause<IN, OUT> clause, int position)
        {
            return $"{clause.Dump()} -- @{position}";
        }
        
        public void Memoize(IClause<IN, OUT> clause, int position, SyntaxParseResult<IN, OUT> result)
        {
            if (_useMemoization)
            {
                _memoizedNonTerminalResults[GetKey(clause, position)] = result;
            }
        }

        public bool TryGetParseResult(IClause<IN, OUT> clause, int position, out SyntaxParseResult<IN, OUT> result)
        {
            if (!_useMemoization)
            {
                result = null;
                return false;
            }
            bool found = _memoizedNonTerminalResults.TryGetValue(GetKey(clause, position), out result);
            return found;
        }

        public ParseResult<IN, OUT> RentResult() => results.Get();
        
        public void ReleaseResult(ParseResult<IN, OUT> result) => results.Return(result);

        public SyntaxLeaf<IN, OUT> RentLeaf(Token<IN> token, bool discarded)
        {
            if (_usePool)
            {
                var leaf = _leafs.Get();
                return leaf;
            }
            return new SyntaxLeaf<IN, OUT>(token,discarded);
        } 
        
        public void ReleaseLeaf(SyntaxLeaf<IN,OUT> lea) => _leafs.Return(lea);

        public SyntaxNode<IN, OUT> RentNode(string name, List<ISyntaxNode<IN, OUT>> children = null, MethodInfo visitor = null)
        {
            if (_usePool)
            {
                var node = _nodes.Get();
                node.Initialize(name,children, visitor);
            }
            return new SyntaxNode<IN, OUT>(name, children, visitor);
        }
        
        public void ReleaseNode(SyntaxNode<IN,OUT> node) => _nodes.Return(node);
        
        public void ReleaseManyNode(ManySyntaxNode<IN,OUT> node) => _manyNodes.Return(node);
        
        public ManySyntaxNode<IN, OUT> RentManyNode(string name)
        {
            if (_usePool)
            {
                var node = _manyNodes.Get();
                node.Initialize(name);
            }
            return new ManySyntaxNode<IN, OUT>(name);
        }
        
        public void ReleaseGroupNode(GroupSyntaxNode<IN,OUT> node) => _groupNodes.Return(node);
        
        public GroupSyntaxNode<IN, OUT> RentGroupNode(string name)
        {
            if (_usePool)
            {
                var node = _groupNodes.Get();
                node.Initialize(name);
            }
            return new GroupSyntaxNode<IN, OUT>(name);
        }
        
        public void ReleaseOptionNode(OptionSyntaxNode<IN,OUT> node) => _optionNodes.Return(node);
        
        public OptionSyntaxNode<IN, OUT> RentOptionNode(string name)
        {
            if (_usePool)
            {
                var node = _optionNodes.Get();
                node.Initialize(name);
            }
            return new OptionSyntaxNode<IN, OUT>(name);
        }
        
        
        
    }
}