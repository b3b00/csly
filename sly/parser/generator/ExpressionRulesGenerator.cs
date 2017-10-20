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

        public OperationMetaData(int precedence, Associativity assoc, MethodInfo method, T oper)
        {
            Precedence = precedence;
            Associativity = assoc;
            VisitorMethod = method;
            OperatorToken = oper;
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
                    OperationMetaData<IN> operation = new OperationMetaData<IN>(attr.Precedence, attr.Assoc,m,ConvertIntToEnum<IN>(attr.Token));
                    var operations = new List<OperationMetaData<IN>>();
                    if (operationsByPrecedence.ContainsKey(operation.Precedence))
                    {
                        operations = operationsByPrecedence[operation.Precedence];
                    }
                    operations.Add(operation);
                }
            });


            methods = parserClass.GetMethods().ToList<MethodInfo>();
            MethodInfo operandMethod = methods.Find(m =>
            {
                List<Attribute> attributes = m.GetCustomAttributes().ToList<Attribute>();
                Attribute attr = attributes.Find(a => a.GetType() == typeof(OperandAttribute));
                return attr != null;
            });

            string operandNonTerminal = null;

            if (operandMethod != null)
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
                GenerateExpressionParser<IN,OUT>(configuration, operandNonTerminal, operationsByPrecedence);
            }

            return configuration;

        }

        private static void GenerateExpressionParser<IN, OUT>(ParserConfiguration<IN, OUT> configuration, string operandNonTerminal, Dictionary<int, List<OperationMetaData<IN>>> operationsByPrecedence) where IN : struct
        {
            List<int> precedences = operationsByPrecedence.Keys.ToList<int>();
            precedences.Sort();
            int max = precedences.Max();

            for (int i = 0; i < precedences.Count-1; i++)
            {
                int precedence = precedences[i];
                int nextPrecedence = precedences[i + 1];
                var operations = operationsByPrecedence[precedence];
                string name = GetNonTerminalNameForPrecedence(precedence, operationsByPrecedence);
                string nextName = GetNonTerminalNameForPrecedence(nextPrecedence, operationsByPrecedence);
            }
        }

        private static string GetNonTerminalNameForPrecedence<IN>(int precedence, Dictionary<int, List<OperationMetaData<IN>>> operationsByPrecedence) where IN : struct
        {
            List<IN> tokens = operationsByPrecedence[precedence].Select(o => o.OperatorToken).ToList<IN>();
            return GetNonTerminalNameForPrecedence(precedence, tokens);
        }

        private static string GetNonTerminalNameForPrecedence<IN>(int precedence, List<IN> operators) where IN : struct
        {
            string operatorsPart = operators
                .Select(oper => oper.ToString())
                .ToList<string>()
                .Aggregate((s1, s2) => $"{s1}_{s2}");
            string name = $"expr_{precedence}_{operators}";
            

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

