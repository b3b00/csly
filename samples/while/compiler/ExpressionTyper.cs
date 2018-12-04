using csly.whileLang.model;

namespace csly.whileLang.compiler
{
    internal class ExpressionTyper
    {
        private readonly Signatures signatures;

        public ExpressionTyper()
        {
            signatures = new Signatures();
        }


        public WhileType TypeExpression(Expression expr, CompilerContext context)
        {
            if (expr is BoolConstant b) return TypeExpression(b, context);
            if (expr is IntegerConstant i) return TypeExpression(i, context);
            if (expr is StringConstant s) return TypeExpression(s, context);
            if (expr is BinaryOperation binary) return TypeExpression(binary, context);
            if (expr is Neg neg) return TypeExpression(neg, context);
            if (expr is Not not) return TypeExpression(not, context);
            if (expr is Variable variable)
            {
                var varType = context.GetVariableType(variable.Name);
                if (varType == WhileType.NONE)
                    throw new TypingException($" variable {variable.Name} {variable.Position} is not intialized");
                variable.CompilerScope = context.CurrentScope;
                return varType;
            }

            throw new SignatureException($"unknow expression type ({expr.GetType().Name})");
        }

        public WhileType TypeExpression(BoolConstant boolConst, CompilerContext context)
        {
            boolConst.CompilerScope = context.CurrentScope;
            return WhileType.BOOL;
        }

        public WhileType TypeExpression(StringConstant stringConst, CompilerContext context)
        {
            stringConst.CompilerScope = context.CurrentScope;
            return WhileType.STRING;
        }

        public WhileType TypeExpression(IntegerConstant intConst, CompilerContext context)
        {
            intConst.CompilerScope = context.CurrentScope;
            return WhileType.INT;
        }

        public WhileType TypeExpression(BinaryOperation operation, CompilerContext context)
        {
            operation.CompilerScope = context.CurrentScope;
            var left = TypeExpression(operation.Left, context);
            operation.Left.Whiletype = left;
            var right = TypeExpression(operation.Right, context);
            operation.Right.Whiletype = right;
            var resultType = signatures.CheckBinaryOperationTyping(operation.Operator, left, right);

            return resultType;
        }


        public WhileType TypeExpression(Neg neg, CompilerContext context)
        {
            var positiveVal = TypeExpression(neg.Value, context);
            neg.CompilerScope = context.CurrentScope;
            if (positiveVal != WhileType.INT)
                throw new SignatureException($"invalid operation type({positiveVal}) : {neg.Dump("")}");
            return WhileType.INT;
        }

        public WhileType TypeExpression(Not not, CompilerContext context)
        {
            var positiveVal = TypeExpression(not.Value, context);
            not.CompilerScope = context.CurrentScope;
            if (positiveVal != WhileType.BOOL)
                throw new SignatureException($"invalid operation type({positiveVal}) : {not.Dump("")}");
            return WhileType.BOOL;
        }
    }
}