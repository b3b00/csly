using BravoLights.Common;
using BravoLights.Common.Ast;
using BravoLights.Connections;

namespace BravoLights.Ast
{
    class SimVarExpression : VariableBase, IAstNode
    {
        public readonly NameAndUnits NameAndUnits;

        public SimVarExpression(string simVarName, string units)
        {
            NameAndUnits = new NameAndUnits { Name = simVarName, Units = units };
        }              

        public override string ToString()
        {
            return Identifier;
        }

        protected override IConnection Connection => SimConnectConnection.Connection;

        public override string Identifier => $"A:{this.NameAndUnits.Name}, {this.NameAndUnits.Units}";
    }
}
