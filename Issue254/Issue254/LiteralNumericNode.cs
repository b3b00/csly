using System.Globalization;

namespace BravoLights.Common.Ast
{
    /// <summary>
    /// A node which represents a literal double number.
    /// </summary>
    class LiteralNumericNode : ConstantNode<double>
    {
        public LiteralNumericNode(double value) : base(value)
        {
        }

        public override string ToString()
        {
            return Value.ToString(CultureInfo.InvariantCulture);
        }
    }
}
