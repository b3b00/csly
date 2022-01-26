using BravoLights.Common;
using BravoLights.Common.Ast;
using BravoLights.Connections;

namespace BravoLights.Ast
{
    class LvarExpression : VariableBase, IAstNode
    {
        public string LVarName { get; set; }

        protected override IConnection Connection => LVarManager.Connection;

        public override string Identifier => $"L:{LVarName}";

        public override string ToString()
        {
            return Identifier;
        }
    }
    
      
}
