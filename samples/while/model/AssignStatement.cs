using csly.whileLang.compiler;
using sly.lexer;
using System;
using System.Collections.Generic;
using System.Text;
using Sigil;

namespace csly.whileLang.model
{
    public class AssignStatement : Statement
    {

        public string VariableName { get; set; }

        public Expression Value { get; set; }

        private Scope scope;
        public Scope CompilerScope { get { return scope; }  set { scope = value; } }

        public TokenPosition Position { get; set; }
        public bool IsVariableCreation { get; internal set; }

        public AssignStatement(string variableName, Expression value)
        {
            VariableName = variableName;
            Value = value;
        }

        public string Dump(string tab)
        {
            StringBuilder dmp = new StringBuilder();
            dmp.AppendLine($"{tab}(ASSIGN");
            dmp.AppendLine($"{tab}\t{VariableName}");
            dmp.AppendLine(Value.Dump(tab+"\t"));
            dmp.AppendLine($"{tab})");
            return dmp.ToString();
        }

        public string Transpile(CompilerContext context)
        {
            StringBuilder code = new StringBuilder();
            if (IsVariableCreation)
            {
                code.AppendLine($"{TypeConverter.WhileToCSharp(this.CompilerScope.GetVariableType(VariableName))} {VariableName};");
            }
            code.AppendLine($"{VariableName} = {Value.Transpile(context)};");
            return code.ToString();
        }

        public Emit<Func<int>> EmitByteCode(CompilerContext context, Emit<Func<int>> emiter)
        {
            Local local = null;
            if (IsVariableCreation)
            {
                local = emiter.DeclareLocal(TypeConverter.WhileToType(CompilerScope.GetVariableType(VariableName)), VariableName);
            }
            else
            {
                local = emiter.Locals[VariableName];
            }
            Value.EmitByteCode(context,emiter);
            emiter.StoreLocal(local);
            return emiter;

        }
    }
}
