﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using sly.parser.syntax.tree;
using sly.parser.generator.visitor;

namespace sly.parser
{
    public class ParseResult<IN, OUT> where IN : struct, Enum
    {
        public OUT Result { get; set; }
        
        /// <summary>
        /// Syntax tree forest (for ambiguous grammars). 
        /// </summary>
        public ParseForest<IN, OUT> Forest { get; set; }
        
        public ISyntaxNode<IN, OUT> SyntaxTree 
        { 
            get => Forest?.MainTree; 
            set 
            {
                if (Forest == null)
                    Forest = new ParseForest<IN, OUT> { Trees = new List<ISyntaxNode<IN, OUT>>() };
                if (Forest.Trees == null)
                    Forest.Trees = new List<ISyntaxNode<IN, OUT>>();
                if (Forest.Trees.Count == 0)
                    Forest.Trees.Add(value);
                else
                    Forest.Trees[0] = value;
            }
        }
        
        /// <summary>
        /// True if parsing resulted in multiple valid syntax trees (i.e., the grammar is ambiguous for the given input).
        /// </summary>
        public bool IsAmbiguous => Forest?.IsAmbiguous ?? false;

        public bool IsError { get; set; }

        public bool IsOk => !IsError;

        public List<ParseError> Errors { get; set; }
        
        /// <summary>
        /// Visit all syntax trees in the forest and return their results.
        /// If the grammar is not ambiguous, returns a list with a single element (the result of the main tree).
        /// </summary>
        public List<OUT> VisitAllTrees(SyntaxTreeVisitor<IN, OUT> visitor, object context = null)
        {
            if (!IsAmbiguous)
                return new List<OUT> { Result };
                
            return Forest.Trees.Select(tree => visitor.VisitSyntaxTree(tree, context)).ToList();
        }
        
        /// <summary>
        /// Access a specific syntax tree in the forest by index and visit it.
        /// </summary>
        public OUT SelectTree(int index, SyntaxTreeVisitor<IN, OUT> visitor, object context = null)
        {
            if (Forest == null || index >= Forest.Count)
                throw new ArgumentOutOfRangeException(nameof(index));
                
            return visitor.VisitSyntaxTree(Forest.Trees[index], context);
        }
        
        /// <summary>
        /// Resolve ambiguity by applying a user-defined selector function to choose one of the syntax trees in the forest, then visit the selected tree.
        /// </summary>
        public OUT ResolveAmbiguity(Func<List<ISyntaxNode<IN, OUT>>, ISyntaxNode<IN, OUT>> selector,
            SyntaxTreeVisitor<IN, OUT> visitor, object context = null)
        {
            if (!IsAmbiguous)
                return Result;
                
            var selectedTree = selector(Forest.Trees);
            return visitor.VisitSyntaxTree(selectedTree, context);
        }

        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            if (IsOk)
            {
                if (IsAmbiguous)
                {
                    return $"parse OK (ambiguous: {Forest.Count} alternatives).";
                }
                return "parse OK.";
            }
            else
            {
                return $"parse failed : {string.Join("\n", Errors.Select(x => x.ErrorMessage))}";
            }
        }
    }
}