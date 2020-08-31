using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using sly.buildresult;
using sly.parser.syntax.grammar;

namespace sly.parser.generator
{
    public class LeftRecursionChecker<IN,OUT> where IN : struct
    {
        public LeftRecursionChecker()
        {
            
        }

        private static List<string> BuildPath(List<string> current, string step)
        {
            var newPath = new List<string>();
            newPath.AddRange(current);
            newPath.Add(step);
            return newPath;
        }

        public static void CheckLeftRecursion(ParserConfiguration<IN, OUT> configuration)
        {
            foreach (var nonTerminal in configuration.NonTerminals)
            {
                CheckLeftRecursion(configuration,nonTerminal.Value, new List<string>(){nonTerminal.Key});
            }
        }
        
        public static void CheckLeftRecursion(ParserConfiguration<IN,OUT> configuration,
            NonTerminal<IN> nonTerminal, List<string> currentPath)
        {
            var (found,path) = FindRecursion(currentPath);
            if (found)
            {
                throw new Exception("left recursion : "+path);
            }
            
            var leftClauses = nonTerminal.Rules.Select(x => x.Clauses.First() as NonTerminalClause<IN>).Where(x => x != null);
            foreach (var leftClause in leftClauses)
            {
                var newNonTerminal = configuration.NonTerminals[leftClause.NonTerminalName];
                if (newNonTerminal != null)
                {
                    var nPath = BuildPath(currentPath, leftClause.NonTerminalName);
                    CheckLeftRecursion(configuration,newNonTerminal,nPath);
                }
                
            }
            
            
        }

        private static (bool, string) FindRecursion(List<string> path)
        {
            for (int i = 0; i < path.Count - 1;i++)
            {
                string step = path[i];
                int next = path.LastIndexOf(step);
                if (next > i)
                {
                    string failure = string.Join(" > ",path.GetRange(i, next - i + 1));
                    return (true, failure);
                }
            }
            
            return (false, null);
        }
        
        
    }
}