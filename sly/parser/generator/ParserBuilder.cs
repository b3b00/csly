using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using sly.buildresult;
using sly.i18n;
using sly.lexer;
using sly.lexer.fsm;
using sly.parser.generator.visitor;
using sly.parser.llparser;
using sly.parser.parser;
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
        
        public string I18n { get; set; }

        public ParserBuilder(string i18n)
        {
            if (string.IsNullOrEmpty(i18n))
            {
                i18n = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
            }
            I18n = i18n;
        }
        
        public ParserBuilder() : this(null)
        {
        }

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
            string rootRule, BuildExtension<IN> extensionBuilder = null, LexerPostProcess<IN> lexerPostProcess = null)
        {
            Parser<IN, OUT> parser = null;
            var result = new BuildResult<Parser<IN, OUT>>();
            switch (parserType)
            {
                case ParserType.LL_RECURSIVE_DESCENT:
                {
                    var configuration = ExtractParserConfiguration(parserInstance.GetType());
                    var (foundRecursion, recursions) = LeftRecursionChecker<IN,OUT>.CheckLeftRecursion(configuration);
                    if (foundRecursion)
                    {
                        var recs = string.Join("\n", recursions.Select(x => string.Join(" > ",x)));
                        result.AddError(new ParserInitializationError(ErrorLevel.FATAL,
                            I18N.Instance.GetText(I18n,Message.LeftRecursion, recs),
                            ErrorCodes.PARSER_LEFT_RECURSIVE));
                        return result;

                    }
                    configuration.StartingRule = rootRule;
                    var syntaxParser = BuildSyntaxParser(configuration, parserType, rootRule);
                    var visitor = new SyntaxTreeVisitor<IN, OUT>(configuration, parserInstance);
                    parser = new Parser<IN, OUT>(I18n,syntaxParser, visitor);
                    var lexerResult = BuildLexer(extensionBuilder, lexerPostProcess);
                    parser.Lexer = lexerResult.Result;
                    if (lexerResult.IsError)
                    {
                        result.Errors.AddRange(lexerResult.Errors);
                        return result;
                    }
                    parser.Instance = parserInstance;
                    parser.Configuration = configuration;
                    result.Result = parser;
                    break;
                }
                case ParserType.EBNF_LL_RECURSIVE_DESCENT:
                {
                    var builder = new EBNFParserBuilder<IN, OUT>(I18n);
                    result = builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, rootRule,
                        extensionBuilder,lexerPostProcess);
                    break;
                }
            }

            parser = result.Result;
            if (!result.IsError)
            {
                var expressionResult = parser.BuildExpressionParser(result, rootRule);
                if (expressionResult.IsError) result.AddErrors(expressionResult.Errors);
                result.Result.Configuration = expressionResult.Result;

                result = CheckParser(result);
                if (result.IsError)
                {
                    result.Result = null;
                }
            }
            else
            {
                result.Result = null;
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
                    parser = new RecursiveDescentSyntaxParser<IN, OUT>(conf, rootRule,I18n);
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


        protected virtual BuildResult<ILexer<IN>> BuildLexer(BuildExtension<IN> extensionBuilder = null,
            LexerPostProcess<IN> lexerPostProcess = null)
        {
            var lexer = LexerBuilder.BuildLexer(new BuildResult<ILexer<IN>>(), extensionBuilder, I18n, lexerPostProcess);
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
            rule.RuleString = $"{ntAndRule.Item1} : {ntAndRule.Item2}";
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
            checkers.Add(CheckAlternates);
            checkers.Add(CheckVisitorsSignature);

            if (result.Result != null && !result.IsError)
                foreach (var checker in checkers)
                    if (checker != null)
                        result.Result.Configuration.NonTerminals.Values.ToList()
                            .ForEach(nt => result = checker(result, nt));
            return result;
        }

        private BuildResult<Parser<IN, OUT>> CheckUnreachable(BuildResult<Parser<IN, OUT>> result,
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
                        I18N.Instance.GetText(I18n,Message.NonTerminalNeverUsed, nonTerminal.Name),
                        ErrorCodes.NOT_AN_ERROR));
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
                    switch (clause)
                    {
                        case NonTerminalClause<IN> ntClause:
                            found = ntClause.NonTerminalName == referenceName;
                            break;
                        case OptionClause<IN> option:
                        {
                            if (option.Clause is NonTerminalClause<IN> inner)
                                found = inner.NonTerminalName == referenceName;
                            break;
                        }
                        case ZeroOrMoreClause<IN> zeroOrMore:
                        {
                            if (zeroOrMore.Clause is NonTerminalClause<IN> inner)
                                found = inner.NonTerminalName == referenceName;
                            break;
                        }
                        case OneOrMoreClause<IN> oneOrMore:
                        {
                            switch (oneOrMore.Clause)
                            {
                                case NonTerminalClause<IN> innerNonTerminal:
                                    found = innerNonTerminal.NonTerminalName == referenceName;
                                    break;
                                case ChoiceClause<IN> innerChoice when innerChoice.IsNonTerminalChoice:
                                    found = innerChoice.Choices.Where(c => (c as NonTerminalClause<IN>).NonTerminalName == referenceName).Any();
                                    break;
                            }

                            break;
                        }
                        case ChoiceClause<IN> choice:
                        {
                            int i = 0;
                            while (i < choice.Choices.Count && !found)
                            {
                                if (choice.Choices[i] is NonTerminalClause<IN> nonTerm)
                                {
                                    found = nonTerm.NonTerminalName == referenceName;
                                }
                                i++;
                            }

                            break;
                        }
                    }

                    iClause++;
                }
                iRule++;
            }

            return found;
        }


        private BuildResult<Parser<IN, OUT>> CheckNotFound(BuildResult<Parser<IN, OUT>> result,
            NonTerminal<IN> nonTerminal)
        {
            var conf = result.Result.Configuration;
            foreach (var rule in nonTerminal.Rules)
            foreach (var clause in rule.Clauses)
                if (clause is NonTerminalClause<IN> ntClause)
                    if (!conf.NonTerminals.ContainsKey(ntClause.NonTerminalName))
                        result.AddError(new ParserInitializationError(ErrorLevel.ERROR,
                            I18N.Instance.GetText(I18n,Message.ReferenceNotFound,ntClause.NonTerminalName,rule.RuleString),
                            ErrorCodes.PARSER_REFERENCE_NOT_FOUND));
            return result;
        }
        
        private BuildResult<Parser<IN, OUT>> CheckAlternates(BuildResult<Parser<IN, OUT>> result,
            NonTerminal<IN> nonTerminal)
        {
            var conf = result.Result.Configuration;

            foreach (var rule in nonTerminal.Rules)
            {
                foreach (var clause in rule.Clauses)
                {
                    if (clause is ChoiceClause<IN> choice)
                    {
                        if (!choice.IsTerminalChoice && !choice.IsNonTerminalChoice)
                        {
                            result.AddError(new ParserInitializationError(ErrorLevel.ERROR,
                                I18N.Instance.GetText(I18n,Message.MixedChoices,rule.RuleString,choice.ToString()),
                                ErrorCodes.PARSER_MIXED_CHOICES));
                        }
                        else if (choice.IsDiscarded && choice.IsNonTerminalChoice)
                        {
                            result.AddError(new ParserInitializationError(ErrorLevel.ERROR,
                                I18N.Instance.GetText(I18n,Message.NonTerminalChoiceCannotBeDiscarded,rule.RuleString,choice.ToString()),
                                ErrorCodes.PARSER_NON_TERMINAL_CHOICE_CANNOT_BE_DISCARDED));
                        }
                    }
                }
            }

            return result;
        }

        private  BuildResult<Parser<IN, OUT>> CheckVisitorsSignature(BuildResult<Parser<IN, OUT>> result,
            NonTerminal<IN> nonTerminal)
        {
            foreach (var rule in nonTerminal.Rules)
            {
                if (!rule.IsSubRule)
                {
                    result = CheckVisitorSignature(result, rule);
                }
            }
            
            return result;
        }

        
        private BuildResult<Parser<IN, OUT>> CheckVisitorSignature(BuildResult<Parser<IN, OUT>> result,
            Rule<IN> rule) 
        {
            if (!rule.IsExpressionRule)
            {
                var visitor = rule.GetVisitor();
                if (visitor == null)
                {
                    ;
                }
                var returnInfo = visitor.ReturnParameter;
                var expectedReturn = typeof(OUT);
                var foundReturn = returnInfo.ParameterType;
                if (!expectedReturn.IsAssignableFrom(foundReturn) && foundReturn != expectedReturn)
                {
                    result.AddError(new InitializationError(ErrorLevel.FATAL,
                        I18N.Instance.GetText(I18n,Message.IncorrectVisitorReturnType,visitor.Name,rule.RuleString,typeof(OUT).FullName,returnInfo.ParameterType.Name),
                        ErrorCodes.PARSER_INCORRECT_VISITOR_RETURN_TYPE));
                }

                var realClauses = rule.Clauses.Where(x => !(x is TerminalClause<IN> || x is ChoiceClause<IN>) || (x is TerminalClause<IN> t && !t.Discarded) || (x is ChoiceClause<IN> c && !c.IsDiscarded) ).ToList();

                if (visitor.GetParameters().Length != realClauses.Count && visitor.GetParameters().Length != realClauses.Count +1)
                {
                    result.AddError(new InitializationError(ErrorLevel.FATAL,
                        I18N.Instance.GetText(I18n,Message.IncorrectVisitorParameterNumber,visitor.Name,rule.RuleString,realClauses.Count.ToString(),(realClauses.Count+1).ToString(),visitor.GetParameters().Length.ToString()),
                        ErrorCodes.PARSER_INCORRECT_VISITOR_PARAMETER_NUMBER));
                    // do not go further : it will cause an out of bound error.
                    return result;
                }
                
                int i = 0;
                foreach (var clause in realClauses)
                {
                    var arg = visitor.GetParameters()[i];

                    switch (clause)
                    {
                        case TerminalClause<IN> terminal:
                        {
                            var expected = typeof(Token<IN>);
                            var found = arg.ParameterType;
                            result = CheckArgType(result, rule, expected, visitor, arg);
                            break;
                        }
                        case NonTerminalClause<IN> nonTerminal:
                        {
                            Type expected = null;
                            var found = arg.ParameterType;
                            if (nonTerminal.IsGroup)
                            {
                                expected = typeof(Group<IN,OUT>);
                            }
                            else
                            {
                                expected = typeof(OUT);
                            }
                            CheckArgType(result, rule, expected, visitor, arg);
                            break;
                        }
                        case ManyClause<IN> many:
                        {
                            Type expected = null;
                            Type found =  arg.ParameterType;
                            var innerClause = many.Clause;
                            switch (innerClause)
                            {
                                case TerminalClause<IN> term:
                                {
                                    expected = typeof(List<Token<IN>>);
                                    break;
                                }
                                case NonTerminalClause<IN> nonTerm:
                                {
                                    if (nonTerm.IsGroup)
                                    {
                                        expected = typeof(List<Group<IN, OUT>>);
                                    }
                                    else
                                    {
                                        expected = typeof(List<OUT>);
                                    }

                                    break;
                                }
                                case GroupClause<IN> group:
                                {
                                    expected = typeof(Group<IN, OUT>);
                                    break;
                                }
                                case ChoiceClause<IN> choice:
                                {
                                    if (choice.IsTerminalChoice)
                                    {
                                        expected = typeof(List<Token<IN>>);
                                    }
                                    else if (choice.IsNonTerminalChoice)
                                    {
                                        expected = typeof(List<OUT>);
                                    }
                                    break;
                                }
                            }
                            result = CheckArgType(result, rule, expected, visitor, arg);
                            break;
                        }
                        case GroupClause<IN> group:
                        {
                            Type expected = typeof(Group<IN,OUT>);
                            Type found =  arg.ParameterType;
                            result = CheckArgType(result, rule, expected, visitor, arg);
                            break;
                        }
                        case OptionClause<IN> option:
                        {
                            Type expected = null;
                            Type found =  arg.ParameterType;
                            var innerClause = option.Clause;
                            switch (innerClause)
                            {
                                case TerminalClause<IN> term:
                                {
                                    expected = typeof(Token<IN>);
                                    break;
                                }
                                case NonTerminalClause<IN> nonTerm:
                                {
                                    if (nonTerm.IsGroup)
                                    {
                                        expected = typeof(ValueOption<Group<IN, OUT>>);
                                    }
                                    else
                                    {
                                        expected = typeof(ValueOption<OUT>);
                                    }

                                    break;
                                }
                                case GroupClause<IN> group:
                                {
                                    expected = typeof(ValueOption<Group<IN, OUT>>);
                                    break;
                                }
                                case ChoiceClause<IN> choice:
                                {
                                    if (choice.IsTerminalChoice)
                                    {
                                        expected = typeof(Token<IN>);
                                    }
                                    else if (choice.IsNonTerminalChoice)
                                    {
                                        expected = typeof(ValueOption<OUT>);
                                    }
                                    break;
                                }
                            }
                            result = CheckArgType(result, rule, expected, visitor, arg);
                            break;
                        }
                    }

                    i++;
                }
            }
            else
            {
                var operations = rule.GetOperations();
                foreach (var operation in operations)
                {
                    var visitor = operation.VisitorMethod;
                    var returnInfo = visitor.ReturnParameter;
                    var expectedReturn = typeof(OUT);
                    var foundReturn = returnInfo?.ParameterType;
                    if (!expectedReturn.IsAssignableFrom(foundReturn) && foundReturn != expectedReturn)
                    {
                        result.AddError(new InitializationError(ErrorLevel.FATAL,
                            I18N.Instance.GetText(I18n,Message.IncorrectVisitorReturnType,visitor.Name,rule.RuleString,typeof(OUT).FullName,returnInfo.ParameterType.Name),
                            ErrorCodes.PARSER_INCORRECT_VISITOR_RETURN_TYPE));
                    }
                    
                    if (operation.IsUnary)
                    {
                        var parameters = visitor.GetParameters();
                        if (parameters.Length != 2 && parameters.Length != 3)
                        {
                            result.AddError(new InitializationError(ErrorLevel.FATAL,
                                $"visitor {visitor.Name} for rule {rule.RuleString} has incorrect argument number : 2 or 3, found {parameters.Length}",
                                ErrorCodes.PARSER_INCORRECT_VISITOR_PARAMETER_NUMBER));
                            // do not go further : it will cause an out of bound error.
                            return result;
                        }
                        if (operation.Affix == Affix.PreFix)
                        {
                            var token = parameters[0];
                            result = CheckArgType(result, rule, typeof(Token<IN>), visitor, token);
                            var value = parameters[1];
                            result = CheckArgType(result, rule, typeof(OUT), visitor, value);
                        }
                        else
                        {
                            var token = parameters[1];
                            result = CheckArgType(result, rule, typeof(Token<IN>), visitor, token);
                            var value = parameters[0];
                            result = CheckArgType(result, rule, typeof(OUT), visitor, value);
                        }
                    }
                    else if (operation.IsBinary)
                    {
                        var parameters = visitor.GetParameters();
                        if (parameters.Length != 3 && parameters.Length != 4)
                        {
                            result.AddError(new InitializationError(ErrorLevel.FATAL,$"visitor {visitor.Name} for rule {rule.RuleString} has incorrect argument number : 3 or 4, found {parameters.Length}", ErrorCodes.PARSER_INCORRECT_VISITOR_PARAMETER_NUMBER));
                            // do not go further : it will cause an out of bound error.
                            return result;
                        }

                        var left = parameters[0];
                        result = CheckArgType(result, rule, typeof(OUT),  visitor, left);
                        var op = parameters[1];
                        result = CheckArgType(result, rule, typeof(Token<IN>),  visitor, op);
                        var right = parameters[2];
                        result = CheckArgType(result, rule, typeof(OUT),  visitor, right);
                        
                    }
                }
            }

            return result;
        }

        private  BuildResult<Parser<IN, OUT>> CheckArgType(BuildResult<Parser<IN, OUT>> result, Rule<IN> rule, Type expected, MethodInfo visitor,
            ParameterInfo arg) 
        {
            if (!expected.IsAssignableFrom(arg.ParameterType) && arg.ParameterType != expected)
            {
                result.AddError(new InitializationError(ErrorLevel.FATAL,
                    I18N.Instance.GetText(I18n,Message.IncorrectVisitorParameterType,visitor.Name,rule.RuleString,arg.Name,expected.FullName,arg.ParameterType.FullName),
                    ErrorCodes.PARSER_INCORRECT_VISITOR_PARAMETER_TYPE));
            }

            return result;
        }

       

        #endregion
    }
}