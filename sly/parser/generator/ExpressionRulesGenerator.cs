using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

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

    class ExpressionRulesGenerator
    {
        // TODO 
        public static ParserConfiguration<IN, OUT> BuildExpressionRules<IN, OUT>(ParserConfiguration<IN, OUT> configuration, Type parserClass) where IN : struct
        {
            return configuration;

            List<MethodInfo> methods = parserClass.GetMethods().ToList<MethodInfo>();
            methods = methods.Where(m =>
            {
                List<Attribute> attributes = m.GetCustomAttributes().ToList<Attribute>();
                Attribute attr = attributes.Find(a => a.GetType() == typeof(OperationAttribute));
                return attr != null;
            }).ToList<MethodInfo>();

            Dictionary<int, List<OperationMetaData<IN>>> operationByPrecedence = new Dictionary<int, List<OperationMetaData<IN>>>();

            methods.ForEach(m =>
            {
                OperationAttribute[] attributes =
                    (OperationAttribute[])m.GetCustomAttributes(typeof(OperationAttribute), true);

                foreach (OperationAttribute attr in attributes)
                {
                    OperationMetaData<IN> operation = new OperationMetaData<IN>(attr.Precedence, attr.Assoc,m,ConvertIntToEnum<IN>(attr.Token));
                    List<OperationMetaData<IN>> operations = new List<OperationMetaData<IN>>();
                    if (operationByPrecedence.ContainsKey(operation.Precedence))
                    {
                        operations = operationByPrecedence[operation.Precedence];
                    }
                    operations.Add(operation);
                    operationByPrecedence[operation.Precedence] = operations;
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

            if (operandNonTerminal != null && operationByPrecedence.Count > 0)
            {
                GenerateExpressionParser<IN,OUT>(configuration, operandNonTerminal, operationByPrecedence);
            }

        }

        private static void GenerateExpressionParser<IN, OUT>(ParserConfiguration<IN, OUT> configuration, string operandNonTerminal, Dictionary<int, List<OperationMetaData<IN>>> operationByPrecedence) where IN : struct
        {
            List<int> precedences = operationByPrecedence.Keys.ToList<int>();
            precedences.Sort();
            int i = 0;
            foreach  (int precedence in precedences)
            {
                bool last = i == precedence.Count - 1;
                i++;
            }
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

