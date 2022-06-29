using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using sly.lexer;
using sly.parser.generator;
using sly.parser.parser;

namespace SimpleTemplate
{
    public class TemplateParser
    {

        [Production("template: item*")]
        public string Template(List<string> items, Dictionary<string,string> context) 
        {
            return string.Join("",items);
        }

        [Production("item : TEXT")]
        public string Text(Token<TemplateLexer> text, Dictionary<string,string> context)
        {
            return text.Value;
        }
        
        [Production("item :OPEN_VALUE[d] ID CLOSE_VALUE[d]")]
        public string Value(Token<TemplateLexer> value, Dictionary<string,string> context)
        {
            
            if (context.TryGetValue(value.Value, out var val))
            {
                return val;
            }

            return "";
        }

        [Production("item : if item (else item)? endif ")]
        public string Conditional(string cond, string thenBlock, ValueOption<Group<TemplateLexer, string>> elseBlock,
            string endif, Dictionary<string,string> context)
        {
            if (cond == "1")
            {
                return thenBlock;
            }
            else
            {
                var gg = elseBlock.Match(
                    (Group<TemplateLexer, string> group) =>
                    {
                        return group;
                    },
                    () =>
                    {
                        var g = new Group<TemplateLexer, string>();
                        g.Add("item", "<none>");
                        return g;
                    });
                return gg.Value("item");
            }
        }

        [Production("if : OPEN_CODE[d] IF[d] OPEN_PAREN[d] TemplateParser_expressions CLOSE_PAREN[d] CLOSE_CODE[d]")]
        public string If(string condition, Dictionary<string,string> context)
        {
            return condition;
        }

        [Production("else :OPEN_CODE[d] ELSE[d] CLOSE_CODE[d]")]
        public string Else(Dictionary<string,string> context)
        {
            return "";
        }
        
        [Production("endif :OPEN_CODE[d] ENDIF[d] CLOSE_CODE[d]")]
        public string EndIf(Dictionary<string,string> context)
        {
            return "";
        }
        
        
        
        
        
        
        
        #region COMPARISON OPERATIONS

        [Infix("LESSER", Associativity.Right, 50)]
        [Infix("GREATER", Associativity.Right, 50)]
        [Infix("EQUALS", Associativity.Right, 50)]
        [Infix("DIFFERENT", Associativity.Right, 50)]
        public string binaryComparisonExpression(string left, Token<TemplateLexer> operatorToken,
            string right, Dictionary<string,string> context)
        {
            int comparison = left.CompareTo(right);

            switch (operatorToken.TokenID)
            {
                case TemplateLexer.LESSER:
                {
                    return comparison < 0 ? "1" : "0";
                    break;
                }
                case TemplateLexer.GREATER:
                {
                    return comparison > 0 ? "1" : "0";
                }
                case TemplateLexer.EQUALS:
                {
                    return comparison == 0 ? "1" : "0";
                }
                case TemplateLexer.DIFFERENT:
                {
                    return comparison != 0 ? "1" : "0";
                }
            }

            return "0";
        }

        #endregion

        #region STRING OPERATIONS

        [Operation((int) TemplateLexer.CONCAT, Affix.InFix, Associativity.Right, 10)]
        public string binaryStringExpression(string left, Token<TemplateLexer> operatorToken, string right, Dictionary<string,string> context)
        {
            return left + right;
        }

        #endregion
        
          #region OPERANDS

          
        [Production("primary: DOUBLE")]
        public string PrimaryInt(Token<TemplateLexer> intToken, Dictionary<string,string> context)
        {
            return intToken.Value;
        }

        
        [Production("primary: TRUE")]
        [Production("primary: FALSE")]
        public string PrimaryBool(Token<TemplateLexer> boolToken, Dictionary<string,string> context)
        {
            return bool.Parse(boolToken.StringWithoutQuotes) ? "1" : "0";
        }

        
        [Production("primary: STRING")]
        public string PrimaryString(Token<TemplateLexer> stringToken, Dictionary<string,string> context)
        {
            return stringToken.StringWithoutQuotes;
        }

        
        [Production("primary: ID")]
        public string PrimaryId(Token<TemplateLexer> varToken, Dictionary<string,string> context)
        {
            string result = "";
            context.TryGetValue(varToken.Value, out result);
            return result;
        }

        [Operand]
        [Production("operand: primary")]
        public string Operand(string prim, Dictionary<string,string> context)
        {
            return prim;
        }

        #endregion

        #region NUMERIC OPERATIONS

        [Operation((int) TemplateLexer.PLUS, Affix.InFix, Associativity.Right, 10)]
        [Operation((int) TemplateLexer.MINUS, Affix.InFix, Associativity.Right, 10)]
        public string binaryTermNumericExpression(string left, Token<TemplateLexer> operatorToken,
            string right, Dictionary<string,string> context)
        {
            double l = double.Parse(left);    
            double r = double.Parse(right);            

            switch (operatorToken.TokenID)
            {
                case TemplateLexer.PLUS:
                {
                    return (l + r).ToString();
                }
                case TemplateLexer.MINUS:
                {
                    return (l - r).ToString();
                }
            }

            
            return "";
        }

        [Operation((int) TemplateLexer.TIMES, Affix.InFix, Associativity.Right, 50)]
        [Operation((int) TemplateLexer.DIVIDE, Affix.InFix, Associativity.Right, 50)]
        public string binaryFactorNumericExpression(string left, Token<TemplateLexer> operatorToken,
            string right, Dictionary<string,string> context)
        {
            double l = double.Parse(left);    
            double r = double.Parse(right);

            switch (operatorToken.TokenID)
            {
                case TemplateLexer.TIMES:
                {
                    return (l * r).ToString();
                }
                case TemplateLexer.DIVIDE:
                {
                    return (l / r).ToString();
                }
            }

            return "";
        }

        [Prefix((int) TemplateLexer.MINUS, Associativity.Right, 100)]
        public string unaryNumericExpression(Token<TemplateLexer> operation, string value, Dictionary<string,string> context)
        {
            if (double.TryParse(value, out double x))
            {
                return (-x).ToString();
            }

            return "0";
        }

        #endregion


        #region BOOLEAN OPERATIONS

        [Operation((int) TemplateLexer.OR, Affix.InFix, Associativity.Right, 10)]
        public string binaryOrExpression(string left, Token<TemplateLexer> operatorToken, string right, Dictionary<string,string> context)
        {
            return left == "1" || right == "1" ? "1" : "0";
        }

        [Operation((int) TemplateLexer.AND, Affix.InFix, Associativity.Right, 50)]
        public string binaryAndExpression(string left, Token<TemplateLexer> operatorToken, string right, Dictionary<string,string> context)
        {
            return left == "1" && right == "1" ? "1" : "0";
        }

        [Operation((int) TemplateLexer.NOT, Affix.PreFix, Associativity.Right, 100)]
        public string binaryOrExpression(Token<TemplateLexer> operatorToken, string value, Dictionary<string,string> context)
        {
            return value == "1" ? "0" : "1";
        }

        #endregion
    }
}