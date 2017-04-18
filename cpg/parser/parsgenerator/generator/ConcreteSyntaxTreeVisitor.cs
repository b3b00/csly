using System;
using parser.parsergenerator.syntax;

namespace parser.parsergenerator.generator
{

    public class ConcreteSyntaxTreeVisitor<T> {

        public Type ParserClass {get; set;}

        public ConcreteSyntaxTreeVisitor(Type parserClass) {
            this.ParserClass = ParserClass;
        }

        public object VisitSyntaxTree(ConcreteSyntaxNode<T> root) {
            return null;
        }

    }
}