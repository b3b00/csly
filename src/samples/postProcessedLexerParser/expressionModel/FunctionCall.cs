using System;
using System.Collections.Generic;
using System.Linq;

namespace postProcessedLexerParser.expressionModel
{
    public class FunctionCall : Expression
    {

        public static double Sin(IEnumerable<double?> parameters)
        {
            if (parameters.Count() == 1 && parameters.First().HasValue)
            {
                return Math.Sin(parameters.First().Value);
            }

            return 0.0;
        }
        
        public static double Cos(IEnumerable<double?> parameters)
        {
            if (parameters.Count() == 1 && parameters.First().HasValue)
            {
                return Math.Cos(parameters.First().Value);
            }
            return 0.0;
        }
        
        public static double Tan(IEnumerable<double?> parameters)
        {
            if (parameters.Count() == 1 && parameters.First().HasValue)
            {
                return Math.Tan(parameters.First().Value);
            }
            return 0.0;
        }
        
        public static double Sqrt(IEnumerable<double?> parameters)
        {
            if (parameters.Count() == 1 && parameters.First().HasValue)
            {
                return Math.Sqrt(parameters.First().Value);
            }
            return 0.0;
        }
        
        public static double Ln(IEnumerable<double?> parameters)
        {
            if (parameters.Count() == 1 && parameters.First().HasValue)
            {
                return Math.Log(parameters.First().Value);
            }
            return 0.0;
        }
        
        public static double Log2(IEnumerable<double?> parameters)
        {
            if (parameters.Count() == 1 && parameters.First().HasValue)
            {
                return Math.Log(parameters.First().Value);
            }
            return 0.0;
        }

        public List<Expression> Parameters { get; set; }

        public string Name { get; set; }
        
        public FunctionCall(string name, List<Expression> parameters)
        {
            Name = name;
            Parameters = parameters;
        }

        public double? Evaluate(ExpressionContext context)
        {
            var parameters = Parameters.Select(x => x.Evaluate(context));
            switch (Name)
            {
                case "sin" :
                    return Sin(parameters);
                case "cos" :
                    return Cos(parameters);
                case "tan" :
                    return Tan(parameters);
                case "sqrt" :
                    return Sqrt(parameters);
                case "ln" :
                    return Ln(parameters);
                case "log" :
                    return Log2(parameters);
            }

            return 0.0;
        }
        
        
    }
}