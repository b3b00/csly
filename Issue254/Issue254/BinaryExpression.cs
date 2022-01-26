
using System;
using System.Collections.Generic;

namespace BravoLights.Common.Ast
{
    abstract class BinaryExpression<TOutput> : IAstNode
    {
        internal readonly IAstNode Lhs;
        internal readonly IAstNode Rhs;

        private object lastLhsValue;
        private object lastRhsValue;
        private object lastReportedValue;

        protected BinaryExpression(IAstNode lhs, IAstNode rhs)
        {
            Lhs = lhs;
            Rhs = rhs;
        }

        public string ErrorText => null;
        public event EventHandler<ValueChangedEventArgs> ValueChanged;

        public IEnumerable<IVariable> Variables
        {
            get
            {
                foreach (var variable in Lhs.Variables) {
                    yield return variable;
                }
                foreach (var variable in Rhs.Variables)
                {
                    yield return variable;
                }
            }
        }

        protected abstract string OperatorText { get; }
        protected abstract object ComputeValue(object lhsValue, object rhsValue);
       
    
       

        public override string ToString()
        {
            return $"({Lhs} {OperatorText} {Rhs})";
        }
    }
}
