using System;
using System.Collections.Generic;
using System.Text;

namespace csly.whileLang.compiler
{
    public class Scope
    {

        public List<Scope> scopes;

        private Dictionary<string, WhileType> variables;

        public Scope ParentScope;

        public int Id;

        public string Path { get
            {
                string path = "";
                if (ParentScope == null)
                {
                    path = Id.ToString();
                }
                else
                {
                    path = $"{ParentScope.Path}.{Id}";

                }
                return path;
            }
        }
        
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

        public Scope CreateNewScope()
        {
            Scope scope = new Scope(this, scopes.Count);
            scopes.Add(scope);
            return scope;
        }


        /// <summary>
        /// Set a variable type
        /// </summary>
        /// <param name="name">varaible name</param>
        /// <param name="variableType">variable type</param>
        /// <returns>true if variable is a new variable in scope</returns>
        public bool SetVariableType(string name, WhileType variableType)
        {
            // TODO search upper scope first
            bool creation = !variables.ContainsKey(name);            
            variables[name] = variableType;
            return creation;
        }
        

        public WhileType GetVariableType(string name)
        {
            WhileType varType = WhileType.NONE;
            if (variables.ContainsKey(name))
            {
                varType = variables[name];
            }
            else if (ParentScope != null)
            {
                varType = ParentScope.GetVariableType(name);
            }
            return varType;
        }

        public bool ExistsVariable(string name)
        {            
            bool exists = variables.ContainsKey(name);
            if (!exists && ParentScope != null)
            {
                exists = ParentScope.ExistsVariable(name);
            }
            return exists;
        }

        public  string Dump(string tab = "")
        {
            StringBuilder dmp = new StringBuilder();
            dmp.AppendLine($"{tab}scope({Path}) {{");
            dmp.AppendLine($"{tab}\t[");
            foreach (KeyValuePair<string, WhileType> pair in variables)
            {
                dmp.AppendLine($"{tab}\t{pair.Key}={pair.Value}");
            }
            dmp.AppendLine($"{tab}\t],");
            foreach (Scope scope in scopes)
            {
                dmp.AppendLine($"{scope.Dump(tab + "\t")},");
            }
            dmp.AppendLine($"{tab}}}");
            return dmp.ToString();
        }
    }
}
