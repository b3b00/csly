using System.Collections.Generic;
using sly.lexer;
using sly.parser.syntax.tree;

namespace sly.parser.generator.visitor
{
    public interface IConcreteSyntaxTreeVisitor<IN,OUT, TREE_OUT> where IN : struct
    {
        TREE_OUT VisitOptionNode(bool exists, TREE_OUT child);
        TREE_OUT VisitNode(SyntaxNode<IN, OUT> node, IList<TREE_OUT> children);
        TREE_OUT VisitManyNode(ManySyntaxNode<IN, OUT> node, IList<TREE_OUT> children);
        TREE_OUT VisitLeaf(Token<IN> token);
    }
}