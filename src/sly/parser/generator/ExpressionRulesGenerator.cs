﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using sly.buildresult;
using sly.i18n;
using sly.parser.syntax.grammar;

namespace sly.parser.generator
{
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

            var operationsByPrecedence = new Dictionary<int, List<OperationMetaData<IN,OUT>>>();
            methods.ForEach(m =>
            {
                var attributes =
                    (OperationAttribute[])m.GetCustomAttributes(typeof(OperationAttribute), true);
                var names = 
                    (NodeNameAttribute[])m.GetCustomAttributes(typeof(NodeNameAttribute), true);
                
                string nodeName = null;
                // a visitor method can only have 1 node name (NodeNameAttribute is not multiple)
                if (names != null && names.Length > 0)
                {
                    nodeName = names[0].Name;
                }

                foreach (var attr in attributes)
                {
                    IN oper = default;
                    string explicitToken = null;
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
                            explicitToken = attr.StringToken;
                        }
                    }


                    bool isEnumValue = EnumConverter.IsEnumValue<IN>(attr.StringToken) ||
                                       attr.IntToken >= 0;
                    OperationMetaData<IN, OUT> operation = null;
                    if (!isEnumValue && !string.IsNullOrEmpty(explicitToken) && explicitToken.StartsWith("'") && explicitToken.EndsWith("'")) 
                    {
                        operation = new OperationMetaData<IN, OUT>(attr.Precedence, attr.Assoc, m, attr.Affix, explicitToken ,nodeName);
                    }
                    else if (isEnumValue)
                    {
                        operation = new OperationMetaData<IN, OUT>(attr.Precedence, attr.Assoc, m, attr.Affix, oper, nodeName);
                    }
                    else
                    {
                        throw new ParserConfigurationException($"bad enum name {attr.StringToken} on Operation definition.");   
                    }

                    var operations = new List<OperationMetaData<IN, OUT>>();
                    if (operationsByPrecedence.TryGetValue(operation.Precedence, out var value))
                        operations = value;
                    operations.Add(operation);
                    operationsByPrecedence[operation.Precedence] = operations;
                }
            });

            
            if (GenerateExpressionParserRules(configuration, parserClass, result, operationsByPrecedence, out var buildResult)) return buildResult;

            return result;
        }

        // TODO AOT : add operands
        internal bool GenerateExpressionParserRules(ParserConfiguration<IN, OUT> configuration, Type parserClass, BuildResult<ParserConfiguration<IN, OUT>> result,
            Dictionary<int, List<OperationMetaData<IN, OUT>>> operationsByPrecedence, out BuildResult<ParserConfiguration<IN, OUT>> buildResult)
        {
            if (operationsByPrecedence.Count > 0)
            {
                var operandNonTerminal = GetOperandNonTerminal(parserClass,configuration, result);


                if (operandNonTerminal != null && operationsByPrecedence.Count > 0)
                    GenerateExpressionParser(configuration, operandNonTerminal, operationsByPrecedence,
                        parserClass.Name);
            }

            configuration.UsesOperations = operationsByPrecedence.Any<KeyValuePair<int, List<OperationMetaData<IN, OUT>>>>(); 
            result.Result = configuration;
            var rec = LeftRecursionChecker<IN, OUT>.CheckLeftRecursion(configuration);
            if (rec.foundRecursion)
            {
                var recs = string.Join("\n", rec.recursions.Select<List<string>, string>(x => string.Join(" > ",x)));
                result.AddError(new ParserInitializationError(ErrorLevel.FATAL,
                    I18N.Instance.GetText(I18n,I18NMessage.LeftRecursion,recs),
                    ErrorCodes.PARSER_LEFT_RECURSIVE));
                {
                    buildResult = result;
                    return false;
                }
            }
            buildResult = result;
            return true;
        }

        private string GetOperandNonTerminal(Type parserClass, ParserConfiguration<IN,OUT> configuration,
            BuildResult<ParserConfiguration<IN, OUT>> result) 
        {
            if (configuration.OperandRules != null && configuration.OperandRules.Any())
            {
                string nonTerminalName = string.Join("-",configuration.OperandRules.Select(x => x.NonTerminalName).Distinct());
                return nonTerminalName;
            }
            
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
                result.AddError(new ParserInitializationError(ErrorLevel.FATAL, I18N.Instance.GetText(I18n,I18NMessage.MissingOperand),ErrorCodes.PARSER_MISSING_OPERAND));
                return null;
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
                var operandNonTerminal = new NonTerminal<IN,OUT>(operandNonTerminalName);
                
                
                foreach (var operand in operandNonTerminals)
                {
                    if (!string.IsNullOrEmpty(operand))
                    {
                        var rule = new Rule<IN,OUT>
                        {
                            IsByPassRule = true,
                            IsExpressionRule = true,
                            Clauses = new List<IClause<IN,OUT>> {new NonTerminalClause<IN,OUT>(operand)}
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
            string operandNonTerminal, Dictionary<int, List<OperationMetaData<IN, OUT>>> operationsByPrecedence,
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

                var nonTerminal = BuildPrecedenceNonTerminal(name, nextName, operations, nextPrecedence < 0);

                configuration.NonTerminals[nonTerminal.Name] = nonTerminal;
            }

            // entry point non terminal
            var entrypoint = new NonTerminal<IN,OUT>($"{parserClassName}_expressions", new List<Rule<IN,OUT>>());
            var prec = precedences[0];
            var lowestname = GetNonTerminalNameForPrecedence(prec, operationsByPrecedence, operandNonTerminal);
            var rule = new Rule<IN,OUT>();
            rule.Clauses.Add(new NonTerminalClause<IN,OUT>(lowestname));
            rule.IsByPassRule = true;
            rule.IsExpressionRule = true;
            rule.ExpressionAffix = Affix.NotOperator;
            configuration.NonTerminals[entrypoint.Name] = entrypoint;
            entrypoint.Rules.Add(rule);
        }

        private NonTerminal<IN,OUT> BuildPrecedenceNonTerminal(string name, string nextName, List<OperationMetaData<IN, OUT>> operations, bool isLastLevel = false)
        {
            var nonTerminal = new NonTerminal<IN,OUT>(name, new List<Rule<IN,OUT>>());

            var InFixOps = operations.Where<OperationMetaData<IN, OUT>>(x => x.Affix == Affix.InFix).ToList<OperationMetaData<IN, OUT>>();
            if (InFixOps.Count > 0)
            {
                var InFixClauses = InFixOps.Select<OperationMetaData<IN, OUT>, TerminalClause<IN,OUT>>(x =>
                {
                    if (x.IsExplicitOperatorToken)
                    {
                        return new TerminalClause<IN,OUT>(x.ExplicitOperatorToken.Substring(1,x.ExplicitOperatorToken.Length-2));
                    }
                    else
                    {
                        return new TerminalClause<IN,OUT>(new LeadingToken<IN>(x.OperatorToken));
                    }
                }).ToList<IClause<IN,OUT>>();

                var rule = new Rule<IN,OUT>
                {
                    ExpressionAffix = Affix.InFix,
                    IsExpressionRule = true,
                    NonTerminalName = name
                };

                rule.Clauses.Add(new NonTerminalClause<IN,OUT>(nextName));
                rule.Clauses.Add(InFixClauses.Count == 1 ? InFixClauses[0] : new ChoiceClause<IN,OUT>(InFixClauses));
                rule.Clauses.Add(new NonTerminalClause<IN,OUT>(name));
                     
                InFixOps.ForEach(x =>
                {
                    rule.SetVisitor(x);
                    rule.IsExpressionRule = true;
                    rule.IsInfixExpressionRule = true;

                });
                nonTerminal.Rules.Add(rule);
                if (isLastLevel)
                {
                    // if next = operand => add rule (name : operand)
                    var rule2 = new Rule<IN,OUT>
                    {
                        ExpressionAffix = Affix.NotOperator,
                        IsExpressionRule = true,
                        NonTerminalName = name,
                        IsByPassRule = true
                    };

                    rule2.Clauses.Add(new NonTerminalClause<IN,OUT>(nextName));
                    nonTerminal.Rules.Add(rule2);
                }
            }


            var PreFixOps = operations.Where<OperationMetaData<IN, OUT>>(x => x.Affix == Affix.PreFix).ToList<OperationMetaData<IN, OUT>>();
            if (PreFixOps.Count > 0)
            {
                var PreFixClauses = PreFixOps.Select<OperationMetaData<IN, OUT>, TerminalClause<IN,OUT>>(x =>
                {
                    if (x.IsExplicitOperatorToken)
                    {
                        return new TerminalClause<IN,OUT>(
                            x.ExplicitOperatorToken.Substring(1, x.ExplicitOperatorToken.Length - 2));
                    }
                    else
                    {
                        return new TerminalClause<IN,OUT>(new LeadingToken<IN>(x.OperatorToken));
                    }
                }).ToList<IClause<IN,OUT>>();
                
                    

                var rule = new Rule<IN,OUT>
                {
                    ExpressionAffix = Affix.PreFix,
                    IsExpressionRule = true
                };

                rule.Clauses.Add(PreFixClauses.Count == 1 ? PreFixClauses[0] : new ChoiceClause<IN,OUT>(PreFixClauses));
                rule.Clauses.Add(new NonTerminalClause<IN,OUT>(nextName));

                PreFixOps.ForEach(x => rule.SetVisitor(x));
                nonTerminal.Rules.Add(rule);
            }

            var PostFixOps = operations.Where<OperationMetaData<IN, OUT>>(x => x.Affix == Affix.PostFix).ToList<OperationMetaData<IN, OUT>>();
            if (PostFixOps.Count > 0)
            {
                var PostFixClauses = PostFixOps.Select<OperationMetaData<IN, OUT>, TerminalClause<IN,OUT>>(x =>
                {
                    if (x.IsExplicitOperatorToken)
                    {
                        return new TerminalClause<IN,OUT>(
                            x.ExplicitOperatorToken.Substring(1, x.ExplicitOperatorToken.Length - 2));
                    }
                    else
                    {
                        return new TerminalClause<IN,OUT>(new LeadingToken<IN>(x.OperatorToken));
                    }
                }).ToList<IClause<IN,OUT>>();

                var rule = new Rule<IN,OUT>
                {
                    ExpressionAffix = Affix.PostFix,
                    IsExpressionRule = true
                };

                rule.Clauses.Add(new NonTerminalClause<IN,OUT>(nextName));
                rule.Clauses.Add(PostFixClauses.Count == 1 ? PostFixClauses[0] : new ChoiceClause<IN,OUT>(PostFixClauses));

                PostFixOps.ForEach(x => rule.SetVisitor(x));
                nonTerminal.Rules.Add(rule);
            }

            if (InFixOps.Count == 0)
            {
                var rule0 = new Rule<IN,OUT>();
                rule0.Clauses.Add(new NonTerminalClause<IN,OUT>(nextName));
                rule0.IsExpressionRule = true;
                rule0.ExpressionAffix = Affix.NotOperator;
                rule0.IsByPassRule = true;
                nonTerminal.Rules.Add(rule0);
            }

            return nonTerminal;
        }

        private string GetNonTerminalNameForPrecedence(int precedence,
            Dictionary<int, List<OperationMetaData<IN, OUT>>> operationsByPrecedence, string operandName) 
        {
            if (precedence > 0)
            {
                var tokens = operationsByPrecedence[precedence].Select<OperationMetaData<IN, OUT>, string>(o => o.Operatorkey).Distinct().ToList<string>();
                return GetNonTerminalNameForPrecedence(precedence, tokens);
            }

            return operandName;
        }

        private string GetNonTerminalNameForPrecedence(int precedence, List<string> operators) 
        {
            var operatorsPart = operators
                //.Select<IN, string>(oper => oper.ToString())
                .ToList<string>()
                .Aggregate<string>((s1, s2) => $"{s1}_{s2}");
            
            return $"{operatorsPart}";
        }
    }
}