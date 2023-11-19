using System.Collections.Generic;
using System.Text;

namespace csly.whileLang.compiler
{
    public class Scope
    {
        public int Id;

        public Scope ParentScope;

        public List<Scope> scopes;

        private readonly Dictionary<string, WhileType> variables;

        public Scope()
        {
            Id = 0;
            ParentScope = null;
            variables = new Dictionary<string, WhileType>();
            scopes = new List<Scope>();
        }

        protected Scope(Scope parent, int id)
        {
            Id = id;
            ParentScope = parent;
            variables = new Dictionary<string, WhileType>();
            scopes = new List<Scope>();
        }

        public string Path
        {
            get
            {
                var path = "";
                if (ParentScope == null)
                    path = Id.ToString();
                else
                    path = $"{ParentScope.Path}.{Id}";
                return path;
            }
        }

        public Scope CreateNewScope()
        {
            var scope = new Scope(this, scopes.Count);
            scopes.Add(scope);
            return scope;
        }


        /// <summary>
        ///     Set a variable type
        /// </summary>
        /// <param name="name">varaible name</param>
        /// <param name="variableType">variable type</param>
        /// <returns>true if variable is a new variable in scope</returns>
        public bool SetVariableType(string name, WhileType variableType)
        {
            var creation = !variables.ContainsKey(name);
            variables[name] = variableType;
            return creation;
        }


        public WhileType GetVariableType(string name)
        {
            var varType = WhileType.NONE;
            if (variables.ContainsKey(name))
                varType = variables[name];
            else if (ParentScope != null) varType = ParentScope.GetVariableType(name);
            return varType;
        }

        public bool ExistsVariable(string name)
        {
            var exists = variables.ContainsKey(name);
            if (!exists && ParentScope != null) exists = ParentScope.ExistsVariable(name);
            return exists;
        }

        public string Dump(string tab = "")
        {
            var dmp = new StringBuilder();
            dmp.AppendLine($"{tab}scope({Path}) {{");
            dmp.AppendLine($"{tab}\t[");
            foreach (var pair in variables) dmp.AppendLine($"{tab}\t{pair.Key}={pair.Value}");
            dmp.AppendLine($"{tab}\t],");
            foreach (var scope in scopes) dmp.AppendLine($"{scope.Dump(tab + "\t")},");
            dmp.AppendLine($"{tab}}}");
            return dmp.ToString();
        }
    }
}