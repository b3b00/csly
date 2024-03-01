using System.Collections.Generic;
using System.Linq;
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

        public static List<string> Lst(params string[] args)
        {
            return args.ToList<string>();
        }

        public static List<string> GetLeftClausesName(IClause<IN> clause)
        {
            switch (clause)
            {
                case NonTerminalClause<IN> nonTerminal:
                    return Lst(nonTerminal.NonTerminalName);
                case ManyClause<IN> many:
                    return GetLeftClausesName(many.Clause);
                case OptionClause<IN> option:
                    return GetLeftClausesName(option.Clause);
                case ChoiceClause<IN> choice when choice.IsNonTerminalChoice:
                    return choice.Choices.SelectMany<IClause<IN>, string>(x => GetLeftClausesName(x)).ToList<string>();
                case GroupClause<IN> group:
                    return GetLeftClausesName(group.Clauses.First<IClause<IN>>());
                default:
                    return new List<string>();
            }
        }

        public static List<string> GetLeftClausesName(Rule<IN> rule, ParserConfiguration<IN, OUT> configuration)
        {
            List<string> lefts = new List<string>();

            int i = 0;
            IClause<IN> current = rule.Clauses[0] as IClause<IN>;
            var currentLefts = GetLeftClausesName(current);
            bool stopped = false;
            while (i < rule.Clauses.Count && ! stopped && currentLefts != null && currentLefts.Any<string>())
            {
                stopped = !current.MayBeEmpty();
                lefts.AddRange(currentLefts);
                stopped = !current.MayBeEmpty();
                i++;
                if (i < rule.Clauses.Count<IClause<IN>>())
                {
                    current = rule.Clauses[i];
                    currentLefts = GetLeftClausesName(current);
                }
                else
                {
                    current = null;
                    currentLefts = null;
                }
            }

            return lefts;

        }


        public static (bool foundRecursion, List<List<string>> recursions) CheckLeftRecursion(ParserConfiguration<IN, OUT> configuration)
        {
            List<List<string>> recursions = new List<List<string>>();
            bool foundRecursion = false;
            foreach (var nonTerminal in configuration.NonTerminals)
            {
                var (found,recursion) = CheckLeftRecursion(configuration,nonTerminal.Value, new List<string> {nonTerminal.Key});
                if (found)
                {
                    foundRecursion = true;
                    recursions.AddRange(recursion);
                }
            }

           

            return (foundRecursion, recursions);
        }
        
        public static (bool recursionFound,List<List<string>> recursion) CheckLeftRecursion(ParserConfiguration<IN,OUT> configuration,
            NonTerminal<IN> nonTerminal, List<string> currentPath)
        {
            var foundRecursion = false;
            List<List<string>> recursions = new List<List<string>>();
            
            var (found,path) = FindRecursion(currentPath);
            if (found)
            {
                return (true,new List<List<string>> {currentPath});
            }
            
            var leftClauses = nonTerminal.Rules.SelectMany<Rule<IN>, string>(x => GetLeftClausesName(x, configuration)).ToList<string>();
            
            foreach (var leftClause in leftClauses)
            {
                if (configuration.NonTerminals.TryGetValue(leftClause, out var newNonTerminal) && newNonTerminal != null)
                {
                    var nPath = BuildPath(currentPath, leftClause);
                    var (foundRRuleRecursion, recursion) = CheckLeftRecursion(configuration, newNonTerminal, nPath);
                    if (!foundRRuleRecursion)
                    {
                        continue;
                    }
                    foundRecursion = true;
                    recursions.AddRange(recursion);

                }
            }

            return (foundRecursion, recursions);


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