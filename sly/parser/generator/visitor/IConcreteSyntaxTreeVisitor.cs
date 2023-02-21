using System.Collections.Generic;
using sly.lexer;
using sly.parser.syntax.tree;

namespace sly.parser.generator.visitor
{
    public interface IConcreteSyntaxTreeVisitor<IN,OUT> where IN : struct
    {
        OUT VisitOptionNode(bool exists, OUT child);
        OUT VisitNode(SyntaxNode<IN> node, IList<OUT> children);
        OUT VisitManyNode(ManySyntaxNode<IN> node, IList<OUT> children);

        OUT VisitEpsilon();
        OUT VisitLeaf(Token<IN> token);
    }
}