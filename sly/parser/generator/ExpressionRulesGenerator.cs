using System.Diagnostics.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using sly.buildresult;
using sly.i18n;
using sly.parser.syntax.grammar;

namespace sly.parser.generator
{
    public class OperationMetaData<T> where T : struct
    {
        public OperationMetaData(int precedence, Associativity assoc, MethodInfo method, Affix affix, T oper)
        {
            Precedence = precedence;
            Associativity = assoc;
            VisitorMethod = method;
            OperatorToken = oper;
            Affix = affix;
        }
        
        public OperationMetaData(int precedence, Associativity assoc, MethodInfo method, Affix affix, string oper)
        {
            Precedence = precedence;
            Associativity = assoc;
            VisitorMethod = method;
            ImplicitOperatorToken = oper;
            Affix = affix;
        }

        public int Precedence { get; set; }

        public Associativity Associativity { get; set; }

        public MethodInfo VisitorMethod { get; set; }

        public T OperatorToken { get; set; }

        public Affix Affix { get; set; }

        public bool IsBinary => Affix == Affix.InFix;

        public bool IsUnary => Affix != Affix.InFix;

        public bool IsImplicitOperatorToken => !string.IsNullOrEmpty(ImplicitOperatorToken);

        public string ImplicitOperatorToken { get; set; }

        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"{OperatorToken} / {Affix} : {Precedence} / {Associativity}";
        }
    }

    public class ExpressionRulesGenerator<IN,OUT> where IN : struct
    {
        public string I18n { get; set; }
        
        public ExpressionRulesGenerator(string i18n = null)
        {
            if (string.IsNullOrEmpty(i18n))
            {
                i18n = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
            }
            I18n = i18n;
        }
        
        public BuildResult<ParserConfiguration<IN, OUT>> BuildExpressionRules(
            ParserConfiguration<IN, OUT> configuration, Type parserClass,
            BuildResult<ParserConfiguration<IN, OUT>> result) 
        {
            
            var methods = parserClass.GetMethods().ToList<MethodInfo>();
            methods = methods.Where<MethodInfo>(m =>
            {
                var attributes = m.GetCustomAttributes(typeof(OperationAttribute),true).ToList<object>();
                
                return attributes.Any<object>();
            }).ToList<MethodInfo>();

            var operationsByPrecedence = new Dictionary<int, List<OperationMetaData<IN>>>();
            methods.ForEach(m =>
            {
                var attributes =
                    (OperationAttribute[])m.GetCustomAttributes(typeof(OperationAttribute), true);

                foreach (var attr in attributes)
                {
                    IN oper = default;
                    string implicitToken = null;
                    if (attr.IsIntToken)
                    {
                        oper = EnumConverter.ConvertIntToEnum<IN>(attr.IntToken);
                    }
                    else if (attr.IsStringToken)
                    {
                        if (EnumConverter.IsEnumValue<IN>(attr.StringToken))
                        {
                            oper = EnumConverter.ConvertStringToEnum<IN>(attr.StringToken);
                        }
                        else
                        {
                            implicitToken = attr.StringToken;
                        }
                    }

                    OperationMetaData<IN> operation = null;
                    if (string.IsNullOrEmpty(implicitToken))
                    {
                        operation = new OperationMetaData<IN>(attr.Precedence, attr.Assoc, m, attr.Affix, oper);
                    }
                    else
                    {
                        operation = new OperationMetaData<IN>(attr.Precedence, attr.Assoc, m, attr.Affix, implicitToken);
                    }

                    var operations = new List<OperationMetaData<IN>>();
                    if (operationsByPrecedence.ContainsKey(operation.Precedence))
                        operations = operationsByPrecedence[operation.Precedence];
                    operations.Add(operation);
                    operationsByPrecedence[operation.Precedence] = operations;
                }
            });

            
            if (operationsByPrecedence.Count > 0)
            {
                var operandNonTerminal = GetOperandNonTerminal(parserClass,configuration, result);


                if (operandNonTerminal != null && operationsByPrecedence.Count > 0)
                    GenerateExpressionParser(configuration, operandNonTerminal, operationsByPrecedence,
                        parserClass.Name);
            }

            configuration.UsesOperations = operationsByPrecedence.Any<KeyValuePair<int, List<OperationMetaData<IN>>>>(); 
            result.Result = configuration;
            return result;
        }

        private string GetOperandNonTerminal(Type parserClass, ParserConfiguration<IN,OUT> configuration,
            BuildResult<ParserConfiguration<IN, OUT>> result) 
        {
            List<MethodInfo> methods;
            methods = parserClass.GetMethods().ToList<MethodInfo>();
            
            var operandMethods = methods.Where<MethodInfo>(m =>
            {
                var attributes = m.GetCustomAttributes().ToList<Attribute>();
                var attr = attributes.Find(a => a.GetType() == typeof(OperandAttribute));
                return attr != null;
            }).ToList<MethodInfo>();
            
            
            if (!operandMethods.Any<MethodInfo>())
            {
                result.AddError(new ParserInitializationError(ErrorLevel.FATAL, I18N.Instance.GetText(I18n,Message.MissingOperand),ErrorCodes.PARSER_MISSING_OPERAND));
                throw new Exception("missing [operand] attribute");
            }

            string operandNonTerminalName = null;

            if (operandMethods.Count == 1)
            {
                var operandMethod = operandMethods.Single<MethodInfo>();
                operandNonTerminalName = GetNonTerminalNameFromProductionMethod(operandMethod);
            }
            else
            {
                operandNonTerminalName = $"{parserClass.Name}_operand";
                var operandNonTerminals = operandMethods.Select<MethodInfo, string>(GetNonTerminalNameFromProductionMethod);
                var operandNonTerminal = new NonTerminal<IN>(operandNonTerminalName);
                
                
                foreach (var operand in operandNonTerminals)
                {
                    if (!string.IsNullOrEmpty(operand))
                    {
                        var rule = new Rule<IN>
                        {
                            IsByPassRule = true,
                            IsExpressionRule = true,
                            Clauses = new List<IClause<IN>> {new NonTerminalClause<IN>(operand)}
                        };
                        operandNonTerminal.Rules.Add(rule);
                    }
                }

                configuration.NonTerminals[operandNonTerminalName] = operandNonTerminal;
            }

            return operandNonTerminalName;
        }

        private string GetNonTerminalNameFromProductionMethod(MethodInfo operandMethod)
        {
            string operandNonTerminal = null;
            if (operandMethod.GetCustomAttributes().ToList<Attribute>()
                .Find(attr => attr.GetType() == typeof(ProductionAttribute)) is ProductionAttribute production)
            {
                var ruleItems = production.RuleString.Split(':');
                if (ruleItems.Length > 0) operandNonTerminal = ruleItems[0].Trim();
            }

            return operandNonTerminal;
        }


        private void GenerateExpressionParser(ParserConfiguration<IN, OUT> configuration,
            string operandNonTerminal, Dictionary<int, List<OperationMetaData<IN>>> operationsByPrecedence,
            string parserClassName) 
        {
            var precedences = operationsByPrecedence.Keys.ToList<int>();
            precedences.Sort();

            for (var i = 0; i < precedences.Count; i++)
            {
                var precedence = precedences[i];
                var nextPrecedence = i < precedences.Count - 1 ? precedences[i + 1] : -1;
                var operations = operationsByPrecedence[precedence];
                var name = GetNonTerminalNameForPrecedence(precedence, operationsByPrecedence, operandNonTerminal);
                var nextName = GetNonTerminalNameForPrecedence(nextPrecedence, operationsByPrecedence, operandNonTerminal);

                var nonTerminal = BuildPrecedenceNonTerminal(name, nextName, operations);

                configuration.NonTerminals[nonTerminal.Name] = nonTerminal;
            }

            // entry point non terminal
            var entrypoint = new NonTerminal<IN>($"{parserClassName}_expressions", new List<Rule<IN>>());
            var prec = precedences[0];
            var lowestname = GetNonTerminalNameForPrecedence(prec, operationsByPrecedence, operandNonTerminal);
            var rule = new Rule<IN>();
            rule.Clauses.Add(new NonTerminalClause<IN>(lowestname));
            rule.IsByPassRule = true;
            rule.IsExpressionRule = true;
            rule.ExpressionAffix = Affix.NotOperator;
            configuration.NonTerminals[entrypoint.Name] = entrypoint;
            entrypoint.Rules.Add(rule);
        }

        private NonTerminal<IN> BuildPrecedenceNonTerminal(string name, string nextName, List<OperationMetaData<IN>> operations)
        {
            var nonTerminal = new NonTerminal<IN>(name, new List<Rule<IN>>());

            var InFixOps = operations.Where<OperationMetaData<IN>>(x => x.Affix == Affix.InFix).ToList<OperationMetaData<IN>>();
            if (InFixOps.Count > 0)
            {
                var InFixClauses = InFixOps.Select<OperationMetaData<IN>, TerminalClause<IN>>(x => new TerminalClause<IN>(x.OperatorToken)).ToList<IClause<IN>>();

                var rule = new Rule<IN>
                {
                    ExpressionAffix = Affix.InFix,
                    IsExpressionRule = true,
                    NonTerminalName = name
                };

                rule.Clauses.Add(new NonTerminalClause<IN>(nextName));
                rule.Clauses.Add(InFixClauses.Count == 1 ? InFixClauses[0] : new ChoiceClause<IN>(InFixClauses));
                rule.Clauses.Add(new NonTerminalClause<IN>(name));

                InFixOps.ForEach(x =>
                {
                    rule.SetVisitor(x);
                    rule.IsExpressionRule = true;
                    rule.IsInfixExpressionRule = true;

                });
                nonTerminal.Rules.Add(rule);
            }


            var PreFixOps = operations.Where<OperationMetaData<IN>>(x => x.Affix == Affix.PreFix).ToList<OperationMetaData<IN>>();
            if (PreFixOps.Count > 0)
            {
                var PreFixClauses = PreFixOps.Select<OperationMetaData<IN>, TerminalClause<IN>>(x => new TerminalClause<IN>(x.OperatorToken)).ToList<IClause<IN>>();

                var rule = new Rule<IN>
                {
                    ExpressionAffix = Affix.PreFix,
                    IsExpressionRule = true
                };

                rule.Clauses.Add(PreFixClauses.Count == 1 ? PreFixClauses[0] : new ChoiceClause<IN>(PreFixClauses));
                rule.Clauses.Add(new NonTerminalClause<IN>(nextName));

                PreFixOps.ForEach(x => rule.SetVisitor(x));
                nonTerminal.Rules.Add(rule);
            }

            var PostFixOps = operations.Where<OperationMetaData<IN>>(x => x.Affix == Affix.PostFix).ToList<OperationMetaData<IN>>();
            if (PostFixOps.Count > 0)
            {
                var PostFixClauses = PostFixOps.Select<OperationMetaData<IN>, TerminalClause<IN>>(x => new TerminalClause<IN>(x.OperatorToken)).ToList<IClause<IN>>();

                var rule = new Rule<IN>
                {
                    ExpressionAffix = Affix.PostFix,
                    IsExpressionRule = true
                };

                rule.Clauses.Add(new NonTerminalClause<IN>(nextName));
                rule.Clauses.Add(PostFixClauses.Count == 1 ? PostFixClauses[0] : new ChoiceClause<IN>(PostFixClauses));

                PostFixOps.ForEach(x => rule.SetVisitor(x));
                nonTerminal.Rules.Add(rule);
            }

            if (InFixOps.Count == 0)
            {
                var rule0 = new Rule<IN>();
                rule0.Clauses.Add(new NonTerminalClause<IN>(nextName));
                rule0.IsExpressionRule = true;
                rule0.ExpressionAffix = Affix.NotOperator;
                rule0.IsByPassRule = true;
                nonTerminal.Rules.Add(rule0);
            }

            return nonTerminal;
        }

        private string GetNonTerminalNameForPrecedence(int precedence,
            Dictionary<int, List<OperationMetaData<IN>>> operationsByPrecedence, string operandName) 
        {
            if (precedence > 0)
            {
                var tokens = operationsByPrecedence[precedence].Select<OperationMetaData<IN>, IN>(o => o.OperatorToken).ToList<IN>();
                return GetNonTerminalNameForPrecedence(precedence, tokens);
            }

            return operandName;
        }

        private string GetNonTerminalNameForPrecedence(int precedence, List<IN> operators) 
        {
            var operatorsPart = operators
                .Select<IN, string>(oper => oper.ToString())
                .ToList<string>()
                .Aggregate<string>((s1, s2) => $"{s1}_{s2}");
            
            return $"expr_{precedence}_{operatorsPart}";
        }
    }
}