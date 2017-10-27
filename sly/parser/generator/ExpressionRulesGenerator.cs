using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using sly.parser.syntax;

namespace sly.parser.generator
{

    public class OperationMetaData<T> where T : struct
    {

        public int Precedence { get; set; }

        public Associativity Associativity { get; set; }

        public MethodInfo VisitorMethod { get; set; }

        public T OperatorToken { get; set; }

        public int Arity { get; set; }

        public bool IsBinary => Arity == 2;

        public bool IsUnary => Arity == 1;


        public OperationMetaData(int precedence, Associativity assoc, MethodInfo method, int arity, T oper)
        {
            Precedence = precedence;
            Associativity = assoc;
            VisitorMethod = method;
            OperatorToken = oper;
            Arity = arity;
        }

        public override string ToString()
        {
            return $"{OperatorToken} / {Arity} : {Precedence} / {Associativity}";
        }
    }

    public class ExpressionRulesGenerator
    {
        public static ParserConfiguration<IN, OUT> BuildExpressionRules<IN, OUT>(ParserConfiguration<IN, OUT> configuration, Type parserClass) where IN : struct
        {
            List<MethodInfo> methods = parserClass.GetMethods().ToList<MethodInfo>();
            methods = methods.Where(m =>
            {
                List<Attribute> attributes = m.GetCustomAttributes().ToList<Attribute>();
                Attribute attr = attributes.Find(a => a.GetType() == typeof(OperationAttribute));
                return attr != null;
            }).ToList<MethodInfo>();


            var operationsByPrecedence = new Dictionary<int, List<OperationMetaData<IN>>>();


            methods.ForEach(m =>
            {
                OperationAttribute[] attributes =
                    (OperationAttribute[])m.GetCustomAttributes(typeof(OperationAttribute), true);

                foreach (OperationAttribute attr in attributes)
                {
                    OperationMetaData<IN> operation = new OperationMetaData<IN>(attr.Precedence, attr.Assoc, m, attr.Arity, ConvertIntToEnum<IN>(attr.Token));
                    var operations = new List<OperationMetaData<IN>>();
                    if (operationsByPrecedence.ContainsKey(operation.Precedence))
                    {
                        operations = operationsByPrecedence[operation.Precedence];
                    }
                    operations.Add(operation);
                    operationsByPrecedence[operation.Precedence] = operations;
                }
            });

            if (operationsByPrecedence.Count > 0)
            {

                methods = parserClass.GetMethods().ToList<MethodInfo>();
                MethodInfo operandMethod = methods.Find(m =>
                {
                    List<Attribute> attributes = m.GetCustomAttributes().ToList<Attribute>();
                    Attribute attr = attributes.Find(a => a.GetType() == typeof(OperandAttribute));
                    return attr != null;
                });

                string operandNonTerminal = null;

                if (operandMethod == null)
                {
                    throw new Exception("missing [operand] attribute");
                }
                else
                {
                    ProductionAttribute production = operandMethod.GetCustomAttributes().ToList<Attribute>().Find((Attribute attr) => attr.GetType() == typeof(ProductionAttribute)) as ProductionAttribute;
                    if (production != null)
                    {
                        string[] ruleItems = production.RuleString.Split(new char[] { ':' });
                        if (ruleItems.Length == 2)
                        {
                            operandNonTerminal = ruleItems[0].Trim();
                        }
                    }
                }


                if (operandNonTerminal != null && operationsByPrecedence.Count > 0)
                {
                    GenerateExpressionParser<IN, OUT>(configuration, operandNonTerminal, operationsByPrecedence, parserClass.Name);
                }

            }
            return configuration;

        }




        private static void GenerateExpressionParser<IN, OUT>(ParserConfiguration<IN, OUT> configuration, string operandNonTerminal, Dictionary<int, List<OperationMetaData<IN>>> operationsByPrecedence, string parserClassName) where IN : struct
        {
            List<int> precedences = operationsByPrecedence.Keys.ToList<int>();
            precedences.Sort();
            int max = precedences.Max();

            for (int i = 0; i < precedences.Count; i++)
            {
                int precedence = precedences[i];
                int nextPrecedence = i < precedences.Count - 1 ? precedences[i + 1] : -1;
                var operations = operationsByPrecedence[precedence];
                string name = GetNonTerminalNameForPrecedence(precedence, operationsByPrecedence, operandNonTerminal);
                string nextName = GetNonTerminalNameForPrecedence(nextPrecedence, operationsByPrecedence, operandNonTerminal);

                NonTerminal<IN> nonTerminal = BuilNonTerminal<IN>(i == precedences.Count - 1, name, nextName, operations, operationsByPrecedence);

                configuration.NonTerminals[nonTerminal.Name] = nonTerminal;
            }

            // entry point non terminal
            NonTerminal<IN> entrypoint = new NonTerminal<IN>($"{parserClassName}_expressions", new List<Rule<IN>>());
            int prec = precedences[0];            
            string lowestname = GetNonTerminalNameForPrecedence(prec, operationsByPrecedence, operandNonTerminal);
            Rule<IN> rule = new Rule<IN>();
            rule.Clauses.Add(new NonTerminalClause<IN>(lowestname));
            rule.IsByPassRule = true;
            rule.IsExpressionRule = true;
            configuration.NonTerminals[entrypoint.Name] = entrypoint;
            entrypoint.Rules.Add(rule);
        }
            

        private static NonTerminal<IN> BuilNonTerminal<IN>(bool last, string name, string nextName, List<OperationMetaData<IN>> operations, Dictionary<int, List<OperationMetaData<IN>>> operationsByPrecedence) where IN : struct
        {
            NonTerminal<IN> nonTerminal = new NonTerminal<IN>(name, new List<Rule<IN>>());
            foreach (OperationMetaData<IN> operation in operations)
            {
                if (operation.IsBinary)
                {
                    Rule<IN> rule = new Rule<IN>();
                    rule.Clauses.Add(new NonTerminalClause<IN>(nextName));
                    rule.Clauses.Add(new TerminalClause<IN>(operation.OperatorToken));
                    rule.Clauses.Add(new NonTerminalClause<IN>(name));
                    rule.IsExpressionRule = true;
                    rule.SetVisitor(operation);
                    nonTerminal.Rules.Add(rule);

                }
                else if (operation.IsUnary)
                {
                    Rule<IN> rule = new Rule<IN>();
                    rule.Clauses.Add(new TerminalClause<IN>(operation.OperatorToken));
                    rule.Clauses.Add(new NonTerminalClause<IN>(nextName));
                    rule.IsExpressionRule = true;
                    rule.SetVisitor(operation);
                    nonTerminal.Rules.Add(rule);
                }
            }
           
            Rule<IN> rule0 = new Rule<IN>();
            rule0.Clauses.Add(new NonTerminalClause<IN>(nextName));
            rule0.IsExpressionRule = true;
            rule0.IsByPassRule = true;
            nonTerminal.Rules.Add(rule0);

            return nonTerminal;
        }

        private static string GetNonTerminalNameForPrecedence<IN>(int precedence, Dictionary<int, List<OperationMetaData<IN>>> operationsByPrecedence, string operandName) where IN : struct
        {
            if (precedence > 0)
            {
                List<IN> tokens = operationsByPrecedence[precedence].Select(o => o.OperatorToken).ToList<IN>();
                return GetNonTerminalNameForPrecedence(precedence, tokens);
            }
            else
            {
                return operandName;
            }
        }

        private static string GetNonTerminalNameForPrecedence<IN>(int precedence, List<IN> operators) where IN : struct
        {
            string operatorsPart = operators
                .Select(oper => oper.ToString())
                .ToList<string>()
                .Aggregate((s1, s2) => $"{s1}_{s2}");
            string name = $"expr_{precedence}_{operatorsPart}";


            return name;
        }





        public static IN ConvertIntToEnum<IN>(int intValue)
        {
            Type genericType = typeof(IN);
            if (genericType.IsEnum)
            {
                foreach (IN value in Enum.GetValues(genericType))
                {
                    Enum test = Enum.Parse(typeof(IN), value.ToString()) as Enum;
                    int val = Convert.ToInt32(test);
                    if (val == intValue)
                    {
                        return value;
                    }
                }
            }
            return default(IN);

        }
    }
}

