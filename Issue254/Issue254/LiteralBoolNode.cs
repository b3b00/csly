namespace BravoLights.Common.Ast
{
    /// <summary>
    /// A node which represents a literal bool.
    /// </summary>
    class LiteralBoolNode : ConstantNode<bool>
    {
        public LiteralBoolNode(bool value) : base(value)
        {
        }

        public override string ToString()
        {
            return Value ? "ON" : "OFF";
        }
    }
}
