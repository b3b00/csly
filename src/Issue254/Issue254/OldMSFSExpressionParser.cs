using BravoLights.Common;
using BravoLights.Common.Ast;
using Issue254.Issue254;
using sly.lexer;
using sly.parser.generator;

namespace BravoLights.Ast
{
#pragma warning disable CA1822 // Mark members as static
    public class OldMSFSExpressionParser : OldExpressionParser
    {
        [Production("primary: LVAR")]
        public IAstNode Lvar(Token<OldExpressionToken> token)
        {
            var text = token.Value[2..];

            return new LvarExpression
            {
                LVarName = text
            };
        }

        [Production("primary: SIMVAR")]
        public IAstNode SimVarExpression(Token<OldExpressionToken> simvarToken)
        {
            var text = simvarToken.Value[2..];
            var bits = text.Split(",");
            var varName = bits[0];
            var type = bits[1].Trim();
            return new SimVarExpression(varName, type);
        }

        public static IAstNode Parse(string expression)
        {
            return Parse<MSFSExpressionParser>(expression);
        }
    }
#pragma warning restore CA1822 // Mark members as static
}