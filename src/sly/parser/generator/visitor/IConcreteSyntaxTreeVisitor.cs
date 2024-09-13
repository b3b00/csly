using System.Collections.Generic;
using sly.lexer;
using sly.parser.syntax.tree;

namespace sly.parser.generator.visitor
{
    public interface IConcreteSyntaxTreeVisitor<IN,OUT, OUTPUT> where IN : struct
    {
        OUTPUT VisitOptionNode(bool exists, OUTPUT child);
        OUTPUT VisitNode(SyntaxNode<IN, OUT> node, IList<OUTPUT> children);
        OUTPUT VisitManyNode(ManySyntaxNode<IN, OUT> node, IList<OUTPUT> children);

        OUTPUT VisitEpsilon();
        OUTPUT VisitLeaf(Token<IN> token);
    }
}