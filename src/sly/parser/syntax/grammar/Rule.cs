using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using sly.lexer;
using sly.parser.generator;

namespace sly.parser.syntax.grammar
{
    public class Rule<IN,OUT> : GrammarNode<IN,OUT> where IN : struct, Enum
    {
        public Rule()
        {
            Clauses = new List<IClause<IN,OUT>>();
            VisitorMethodsForOperation = new Dictionary<string, OperationMetaData<IN, OUT>>();
            LambdaVisitorsForOperation = new Dictionary<string, OperationMetaData<IN, OUT>>();
            Visitor = null;
            IsSubRule = false;
            NodeName = "";
        }

        public string NodeName { get; set; } = null;
        
        public string[] SubNodeNames { get; set; } = null;
        
        private Dictionary<string,string> NodeNamesMap { get; set; } = new Dictionary<string, string>();
        
        public bool IsByPassRule { get; set; } = false;

        // visitors for operation rules
        private Dictionary<string, OperationMetaData<IN, OUT>> VisitorMethodsForOperation { get; }
        
        private Dictionary<string, OperationMetaData<IN, OUT>> LambdaVisitorsForOperation { get; }
        
        // visitor for classical rules
        private MethodInfo Visitor { get; set; }
        
        private Func<object[],OUT> LambdaVisitor { get; set; }  

        public bool IsExpressionRule { get; set; }
        
        public bool IsInfixExpressionRule { get; set; }

        public Affix ExpressionAffix { get; set; }

        public string RuleString { get; set;  }

        public List<IClause<IN,OUT>> Clauses { get; set; }
        public List<LeadingToken<IN>> PossibleLeadingTokens { get; set; }

        public string NonTerminalName { get; set; }

        public bool ContainsSubRule
        {
            get
            {
                if (Clauses != null && Clauses.Any())
                {
                    bool contains = false;
                    foreach (var clause in Clauses)
                    {
                        switch (clause)
                        {
                            case GroupClause<IN,OUT> _:
                                contains = true;
                                break;
                            case ManyClause<IN,OUT> many:
                                contains  |=  many.Clause is GroupClause<IN,OUT>;
                                break;
                            case OptionClause<IN,OUT> option:
                                contains  |=  option.Clause is GroupClause<IN,OUT>;
                                break;
                        }

                        if (contains)
                        {
                            return true;
                        }
                        }
                    }

                return false;
            }
        }

        public bool IsSubRule { get; set; }

        public bool MayBeEmpty => Clauses == null
                                  || Clauses.Count == 0
                                  || Clauses.Count == 1 && Clauses[0].MayBeEmpty();

        public bool ForcedName { get; set; }


        public OperationMetaData<IN, OUT> GetOperation(string token)
        {
            OperationMetaData<IN, OUT> operation = null;
            if (IsExpressionRule)
            {
                if (LambdaVisitorsForOperation.ContainsKey(token))
                {
                    return LambdaVisitorsForOperation[token];
                }

                if (VisitorMethodsForOperation.ContainsKey(token))
                {
                    return VisitorMethodsForOperation[token];
                }
                else
                {
                    ;

                }
            }

            return operation;
        }
        
        public List<OperationMetaData<IN, OUT>> GetOperations()
        {
            if (IsExpressionRule)
            {
                return VisitorMethodsForOperation.Values.ToList();
            }

            return null;
        }

        public Func<object[], OUT> getLambdaVisitor(string token )
        {
            Func<object[], OUT> visitor = null;
            if (IsExpressionRule && !string.IsNullOrEmpty(token))
            {
                var operation = LambdaVisitorsForOperation.TryGetValue(token.ToString(), out var value)
                    ? value
                    : null;
                visitor = operation?.LambdaVisitor;
            }
            else
            {
                visitor = LambdaVisitor;
            }

            return visitor;
        }

        public void SetLambdaVisitor(Func<object[], OUT> lambdaVisitor)
        {
            LambdaVisitor = lambdaVisitor;
        }
        
        public MethodInfo GetVisitorMethod(string token = null)
        {
            MethodInfo visitor = null;
            if (IsExpressionRule && !string.IsNullOrEmpty(token))
            {
                var operation = VisitorMethodsForOperation.TryGetValue(token, out var value)
                    ? value
                    : null;
                visitor = operation?.VisitorMethod;
            }
            else
            {
                visitor = Visitor;
            }

            return visitor;
        }
        
        public Func<object[], OUT> GetLambdaVisitor(string token = null)
        {
            Func<object[], OUT> visitor = null;
            if (IsExpressionRule && !string.IsNullOrEmpty(token))
            {
                var operation = LambdaVisitorsForOperation.TryGetValue(token, out var value)
                    ? value
                    : null;
                visitor = operation?.LambdaVisitor;
            }
            else
            {
                visitor = LambdaVisitor;
            }

            return visitor;
        }
        
        public void SetVisitor(MethodInfo visitor)
        {
            Visitor = visitor;
        }
        
        public void SetVisitor(Func<object[],OUT> visitorLambda)
        {
            LambdaVisitor = visitorLambda;
        }

        public void SetVisitor(OperationMetaData<IN, OUT> operation)
        {
            NodeNamesMap[operation.Operatorkey] = operation.NodeName;
            if (operation.VisitorMethod != null)
                VisitorMethodsForOperation[operation.Operatorkey] = operation;
            else if (operation.LambdaVisitor != null)
            {
                LambdaVisitorsForOperation[operation.Operatorkey] = operation;
            }
        }

        
        
        public bool Match(IList<Token<IN>> tokens, int position, ParserConfiguration<IN,OUT> configuration)
        {
            bool activateBroadWindow = configuration.BroadenTokenWindow;
            if (activateBroadWindow && Clauses.Count >= 2 && Clauses[0] is TerminalClause<IN,OUT> startingTerminalClause && Clauses[1] is NonTerminalClause<IN,OUT> nTerm)
            {
                if (startingTerminalClause.MayBeEmpty())
                {
                    return true;
                }
                if (!startingTerminalClause.MayBeEmpty() && startingTerminalClause.Check(tokens[position]))
                {
                    var secondPossibleLeadings =
                        configuration.NonTerminals[nTerm.NonTerminalName].GetPossibleLeadingTokens();
                    if (secondPossibleLeadings.Exists(x => x.Match(tokens[position+1])) || nTerm.MayBeEmpty())
                    {
                        return true;
                    }
                }

                

                return false;
            }

            int i = 0;
            bool match = false;
            if (position < 0 || position >= tokens.Count)
            {
                return false;
            }
            var token = tokens[position];
            if (MayBeEmpty)
            {
                return true;
            }
            while (i < PossibleLeadingTokens.Count && !match)
            {
                var leader = PossibleLeadingTokens[i];
                match = leader.Match(token);
                i++;
            }
            return match;
        }

        [ExcludeFromCodeCoverage]
        public string Dump()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(NonTerminalName).Append(" : ");
            foreach (var clause in Clauses)
            {
                builder.Append(clause.Dump()).Append(" ");
            }

            return builder.ToString();
        }
    }
}