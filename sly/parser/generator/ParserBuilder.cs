using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using sly.buildresult;
using sly.lexer;
using sly.parser.generator.visitor;
using sly.parser.llparser;
using sly.parser.syntax.grammar;

namespace sly.parser.generator
{
    public delegate BuildResult<Parser<IN, OUT>> ParserChecker<IN, OUT>(BuildResult<Parser<IN, OUT>> result,
        NonTerminal<IN> nonterminal) where IN : struct;

    /// <summary>
    ///     this class provides API to build parser
    /// </summary>
    public class ParserBuilder<IN, OUT> where IN : struct
    {
        #region API

        /// <summary>
        ///     Builds a parser (lexer, syntax parser and syntax tree visitor) according to a parser definition instance
        /// </summary>
        /// <typeparam name="IN"></typeparam>
        /// <param name="parserInstance">
        ///     a parser definition instance , containing
        ///     [Reduction] methods for grammar rules
        ///     <param name="parserType">
        ///         a ParserType enum value stating the analyser type (LR, LL ...) for now only LL recurive
        ///         descent parser available
        ///     </param>
        ///     <param name="rootRule">the name of the root non terminal of the grammar</param>
        ///     <returns></returns>
        public virtual BuildResult<Parser<IN, OUT>> BuildParser(object parserInstance, ParserType parserType,
            string rootRule)
        {
            Parser<IN, OUT> parser = null;
            var result = new BuildResult<Parser<IN, OUT>>();
            if (parserType == ParserType.LL_RECURSIVE_DESCENT)
            {
                var configuration = ExtractParserConfiguration(parserInstance.GetType());
                configuration.StartingRule = rootRule;
                var syntaxParser = BuildSyntaxParser(configuration, parserType, rootRule);
                var visitor = new SyntaxTreeVisitor<IN, OUT>(configuration, parserInstance);
                parser = new Parser<IN, OUT>(syntaxParser, visitor);
                var lexerResult = BuildLexer();
                parser.Lexer = lexerResult.Result;
                if (lexerResult.IsError) result.AddErrors(lexerResult.Errors);
                parser.Instance = parserInstance;
                parser.Configuration = configuration;
                result.Result = parser;
            }
            else if (parserType == ParserType.EBNF_LL_RECURSIVE_DESCENT)
            {
                var builder = new EBNFParserBuilder<IN, OUT>();
                result = builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, rootRule);
            }

            parser = result.Result;
            if (!result.IsError)
            {
                var expressionResult = parser.BuildExpressionParser(result, rootRule);
                if (expressionResult.IsError) result.AddErrors(expressionResult.Errors);
                result.Result.Configuration = expressionResult.Result;

                result = CheckParser(result);
            }

            return result;
        }


        protected virtual ISyntaxParser<IN, OUT> BuildSyntaxParser(ParserConfiguration<IN, OUT> conf,
            ParserType parserType, string rootRule)
        {
            ISyntaxParser<IN, OUT> parser = null;
            switch (parserType)
            {
                case ParserType.LL_RECURSIVE_DESCENT:
                {
                    parser = new RecursiveDescentSyntaxParser<IN, OUT>(conf, rootRule);
                    break;
                }
                default:
                {
                    parser = null;
                    break;
                }
            }

            return parser;
        }

        #endregion


        #region CONFIGURATION

        private Tuple<string, string> ExtractNTAndRule(string ruleString)
        {
            Tuple<string, string> result = null;
            if (ruleString != null)
            {
                var nt = "";
                var rule = "";
                var i = ruleString.IndexOf(":");
                if (i > 0)
                {
                    nt = ruleString.Substring(0, i).Trim();
                    rule = ruleString.Substring(i + 1);
                    result = new Tuple<string, string>(nt, rule);
                }
            }

            return result;
        }


        protected virtual BuildResult<ILexer<IN>> BuildLexer()
        {
            var lexer = LexerBuilder.BuildLexer(new BuildResult<ILexer<IN>>());
            return lexer;
        }


        protected virtual ParserConfiguration<IN, OUT> ExtractParserConfiguration(Type parserClass)
        {
            var conf = new ParserConfiguration<IN, OUT>();
            var functions = new Dictionary<string, MethodInfo>();
            var nonTerminals = new Dictionary<string, NonTerminal<IN>>();
            var methods = parserClass.GetMethods().ToList();
            methods = methods.Where(m =>
            {
                var attributes = m.GetCustomAttributes().ToList();
                var attr = attributes.Find(a => a.GetType() == typeof(ProductionAttribute));
                return attr != null;
            }).ToList();

            parserClass.GetMethods();
            methods.ForEach(m =>
            {
                var attributes = (ProductionAttribute[]) m.GetCustomAttributes(typeof(ProductionAttribute), true);

                foreach (var attr in attributes)
                {
                    var ntAndRule = ExtractNTAndRule(attr.RuleString);


                    var r = BuildNonTerminal(ntAndRule);
                    r.SetVisitor(m);
                    r.NonTerminalName = ntAndRule.Item1;
                    var key = ntAndRule.Item1 + "__" + r.Key;
                    functions[key] = m;
                    NonTerminal<IN> nonT = null;
                    if (!nonTerminals.ContainsKey(ntAndRule.Item1))
                        nonT = new NonTerminal<IN>(ntAndRule.Item1, new List<Rule<IN>>());
                    else
                        nonT = nonTerminals[ntAndRule.Item1];
                    nonT.Rules.Add(r);
                    nonTerminals[ntAndRule.Item1] = nonT;
                }
            });

            conf.NonTerminals = nonTerminals;

            return conf;
        }

        private Rule<IN> BuildNonTerminal(Tuple<string, string> ntAndRule)
        {
            var rule = new Rule<IN>();

            var clauses = new List<IClause<IN>>();
            var ruleString = ntAndRule.Item2;
            var clausesString = ruleString.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in clausesString)
            {
                IClause<IN> clause = null;
                var isTerminal = false;
                var token = default(IN);
                try
                {
                    var tIn = typeof(IN);
                    var b = Enum.TryParse(item, out token);
                    if (b)
                    {
                        isTerminal = true;
                    }

                    //token = (IN)Enum.Parse(tIn , item);
                    //isTerminal = true;
                }
                catch (ArgumentException)
                {
                    isTerminal = false;
                }

                if (isTerminal)
                {
                    clause = new TerminalClause<IN>(token);
                }
                else if (item == "[d]")
                {
                    if (clauses.Last() is TerminalClause<IN> discardedTerminal) discardedTerminal.Discarded = true;
                }
                else
                {
                    clause = new NonTerminalClause<IN>(item);
                }

                if (clause != null) clauses.Add(clause);
            }

            rule.Clauses = clauses;
            //rule.Key = ntAndRule.Item1 + "_" + ntAndRule.Item2.Replace(" ", "_");

            return rule;
        }

        #endregion

        #region parser checking

        private BuildResult<Parser<IN, OUT>> CheckParser(BuildResult<Parser<IN, OUT>> result)
        {
            var checkers = new List<ParserChecker<IN, OUT>>();
            checkers.Add(CheckUnreachable);
            checkers.Add(CheckNotFound);

            if (result.Result != null && !result.IsError)
                foreach (var checker in checkers)
                    if (checker != null)
                        result.Result.Configuration.NonTerminals.Values.ToList()
                            .ForEach(nt => result = checker(result, nt));
            return result;
        }

        private static BuildResult<Parser<IN, OUT>> CheckUnreachable(BuildResult<Parser<IN, OUT>> result,
            NonTerminal<IN> nonTerminal)
        {
            var conf = result.Result.Configuration;
            var found = false;
            if (nonTerminal.Name != conf.StartingRule)
            {
                foreach (var nt in result.Result.Configuration.NonTerminals.Values.ToList())
                    if (nt.Name != nonTerminal.Name)
                    {
                        found = NonTerminalReferences(nt, nonTerminal.Name);
                        if (found) break;
                    }

                if (!found)
                    result.AddError(new ParserInitializationError(ErrorLevel.WARN,
                        $"non terminal [{nonTerminal.Name}] is never used."));
            }

            return result;
        }


        private static bool NonTerminalReferences(NonTerminal<IN> nonTerminal, string referenceName)
        {
            var found = false;
            var iRule = 0;
            while (iRule < nonTerminal.Rules.Count && !found)
            {
                var rule = nonTerminal.Rules[iRule];
                var iClause = 0;
                while (iClause < rule.Clauses.Count && !found)
                {
                    var clause = rule.Clauses[iClause];
                    if (clause is NonTerminalClause<IN> ntClause)
                    {
                        if (ntClause != null) found = found || ntClause.NonTerminalName == referenceName;
                    }
                    else if (clause is OptionClause<IN> option)
                    {
                        if (option != null && option.Clause is NonTerminalClause<IN> inner)
                            found = found || inner.NonTerminalName == referenceName;
                    }
                    else if (clause is ZeroOrMoreClause<IN> zeroOrMore)
                    {
                        if (zeroOrMore != null && zeroOrMore.Clause is NonTerminalClause<IN> inner)
                            found = found || inner.NonTerminalName == referenceName;
                    }
                    else if (clause is OneOrMoreClause<IN> oneOrMore)
                    {
                        if (oneOrMore != null && oneOrMore.Clause is NonTerminalClause<IN> inner)
                            found = found || inner.NonTerminalName == referenceName;
                    }

                    iClause++;
                }

                iRule++;
            }

            return found;
        }


        private static BuildResult<Parser<IN, OUT>> CheckNotFound(BuildResult<Parser<IN, OUT>> result,
            NonTerminal<IN> nonTerminal)
        {
            var conf = result.Result.Configuration;
            foreach (var rule in nonTerminal.Rules)
            foreach (var clause in rule.Clauses)
                if (clause is NonTerminalClause<IN> ntClause)
                    if (!conf.NonTerminals.ContainsKey(ntClause.NonTerminalName))
                        result.AddError(new ParserInitializationError(ErrorLevel.ERROR,
                            $"{ntClause.NonTerminalName} references from {rule.RuleString} does not exist."));
            return result;
        }

        #endregion
    }
}