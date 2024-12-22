using sly.parser.generator;
using sly.parser.syntax.grammar;
using sly.parser.syntax.tree;

namespace sly.parser.llparser.bnf;

public partial class RecursiveDescentSyntaxParser<IN, OUT> where IN : struct
{
    
            protected SyntaxNode<IN, OUT> ManageExpressionRules(Rule<IN, OUT> rule, SyntaxNode<IN, OUT> node)
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

                        if (operatorIndex >= 0 && node.Children[operatorIndex] is SyntaxLeaf<IN, OUT> operatorNode)
                        {
                            var token = operatorNode.Token;
                            string key = node.ForcedName && node.Name != null ? node.Name : (token.IsExplicit ? $"'{token.Value}'" : token.TokenID.ToString());
                            var operation = rule.GetOperation(key);
                            if (operation != null)
                            {
                                node.Visitor = operation.VisitorMethod;
                                node.LambdaVisitor = operation.LambdaVisitor;
                                node.Operation = operation;
                            }
                        }
                        break;
                    }
                    case false:
                    {
                        node.LambdaVisitor = rule.getLambdaVisitor(null);
                        node.Visitor = rule.GetVisitorMethod(null);
                        break;
                    }
                }
    
                return node;
            }
    
}