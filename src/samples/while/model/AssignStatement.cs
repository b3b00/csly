using System;
using System.Collections.Generic;
using System.Text;
using csly.whileLang.compiler;
using sly.lexer;
using Sigil;

namespace csly.whileLang.model
{
    public class AssignStatement : Statement
    {
        public AssignStatement(string variableName, Expression value)
        {
            VariableName = variableName;
            Value = value;
        }

        public string VariableName { get; set; }

        public Expression Value { get; set; }
        public bool IsVariableCreation { get; internal set; }
        public Scope CompilerScope { get; set; }

        public LexerPosition Position { get; set; }

        public string Dump(string tab)
        {
            var dmp = new StringBuilder();
            dmp.AppendLine($"{tab}(ASSIGN");
            dmp.AppendLine($"{tab}\t{VariableName}");
            dmp.AppendLine(Value.Dump(tab + "\t"));
            dmp.AppendLine($"{tab})");
            return dmp.ToString();
        }

        public string Transpile(CompilerContext context)
        {
            var code = new StringBuilder();
            if (IsVariableCreation)
                code.AppendLine(
                    $"{TypeConverter.WhileToCSharp(CompilerScope.GetVariableType(VariableName))} {VariableName};");
            code.AppendLine($"{VariableName} = {Value.Transpile(context)};");
            return code.ToString();
        }

        public Emit<Func<int>> EmitByteCode(CompilerContext context, Emit<Func<int>> emiter)
        {
            var ternaries = new List<TernaryExpression>();
            Value.AppendTernaries(ternaries);
            foreach (var ternary in ternaries)
            {
                ternary.EmitByteCodeForVariable(context, emiter);
            }
            
            Local local = null;
            if (IsVariableCreation)
                local = emiter.DeclareLocal(TypeConverter.WhileToType(CompilerScope.GetVariableType(VariableName)),
                    VariableName);
            else
                local = emiter.Locals[VariableName];
            Value.EmitByteCode(context, emiter);
            emiter.StoreLocal(local);
            return emiter;
        }

        public void AppendTernaries(List<TernaryExpression> ternaries)
        {
            Value.AppendTernaries(ternaries);
        }
    }
}