using System.Diagnostics.CodeAnalysis;

namespace csly.whileLang.compiler
{
    public class CompilerContext
    {
        public CompilerContext()
        {
            RootScope = new Scope();
            CurrentScope = RootScope;
        }

        public bool IsQuiet { get; set; }

        public Scope RootScope { get; protected set; }

        public Scope CurrentScope { get; protected set; }

        [ExcludeFromCodeCoverage]
        public string Dump()
        {
            return RootScope.Dump("");
        }

        public override string ToString()
        {
            return Dump();
        }


        #region variable management

        public bool SetVariableType(string name, WhileType variableType)
        {
            return CurrentScope.SetVariableType(name, variableType);
        }


        public WhileType GetVariableType(string name)
        {
            return CurrentScope.GetVariableType(name);
        }

        public bool VariableExists(string name)
        {
            return CurrentScope.ExistsVariable(name);
        }

        #endregion

        #region scope management

        public Scope OpenNewScope()
        {
            var scope = CurrentScope.CreateNewScope();
            CurrentScope = scope;
            return scope;
        }


        public Scope CloseScope()
        {
            CurrentScope = CurrentScope?.ParentScope;
            return CurrentScope;
        }

        #endregion
    }
}