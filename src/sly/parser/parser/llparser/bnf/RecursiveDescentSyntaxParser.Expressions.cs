using sly.parser.generator;
using sly.parser.syntax.grammar;
using sly.parser.syntax.tree;

namespace sly.parser.llparser.bnf;

public partial class RecursiveDescentSyntaxParser<IN, OUT> where IN : struct
{
    
            protected SyntaxNode<IN> ManageExpressionRules(Rule<IN> rule, SyntaxNode<IN> node)
            {
                var operatorIndex = -1;
                switch (rule.IsExpressionRule)
                {
                    case true when rule.IsByPassRule:
                        node.IsByPassNode = true;
                        node.HasByPassNodes = true;
                        break;
                    case true when !rule.IsByPassRule:
                    {
                        node.ExpressionAffix = rule.ExpressionAffix;
                        switch (node.Children.Count)
                        {
                            case 3:
                                operatorIndex = 1;
                                break;
                            case 2 when node.ExpressionAffix == Affix.PreFix:
                                operatorIndex = 0;
                                break;
                            case 2:
                            {
                                if (node.ExpressionAffix == Affix.PostFix) operatorIndex = 1;
                                break;
                            }
                        }

                        if (operatorIndex >= 0 && node.Children[operatorIndex] is SyntaxLeaf<IN> operatorNode)
                        {
                            var visitor = rule.GetVisitor(operatorNode.Token.TokenID);
                            if (visitor != null)
                            {
                                node.Visitor = visitor;
                                node.Operation = rule.GetOperation(operatorNode.Token.TokenID);
                            }
                        }

                        break;
                    }
                    case false:
                        node.Visitor = rule.GetVisitor();
                        break;
                }
    
                return node;
            }
    
}