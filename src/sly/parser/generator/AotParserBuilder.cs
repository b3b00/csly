using System;
using System.Collections.Generic;
using System.Linq;
using sly.buildresult;
using sly.i18n;
using sly.lexer;
using sly.lexer.fsm;
using sly.parser;
using sly.parser.generator;
using sly.parser.generator.visitor;
using sly.parser.syntax.grammar;

namespace aot.parser;

public class AotParserBuilder<IN, OUT> : IAotParserBuilder<IN,OUT> where IN : struct
{
    readonly Dictionary<string, NonTerminal<IN,OUT>> _nonTerminals = new Dictionary<string, NonTerminal<IN,OUT>>();


    private bool _useMemoization = false;

    private bool _useBroadenTokenWindow = false;

    private bool _useAutoCloseIndentations = false;

    private string _i18N;

    private string _rootRule;

    private object _parserInstance;
    
    private IAotLexerBuilder<IN> _lexerBuilder;

    private Action<IN, LexemeAttribute, GenericLexer<IN>> _extensionBuilder = null;
    
    private LexerPostProcess<IN> _lexerPostProcess = null;

    private ParserConfiguration<IN, OUT> _configuration = null;
    public static IAotParserBuilder<IN,OUT> NewBuilder<IN,OUT>(object parserInstance, string rootRule, string i18N = "en") where IN : struct
    {
        return new AotParserBuilder<IN,OUT>(i18N, parserInstance, rootRule);
    }
    
    private AotParserBuilder(string i18N, object parserInstance, string rootRule)
    {
        _i18N = i18N;
        _parserInstance = parserInstance;
        _rootRule = rootRule;
    }

    public ISyntaxParser<IN, OUT> BuildSyntaxParser(BuildResult<ParserConfiguration<IN, OUT>> result)
    {
        // build configuration
        _configuration = new ParserConfiguration<IN, OUT>();
        _configuration.UseMemoization = _useMemoization;
        _configuration.BroadenTokenWindow = _useBroadenTokenWindow;
        _configuration.AutoCloseIndentations = _useAutoCloseIndentations;
        _configuration.NonTerminals = _nonTerminals;
        _configuration.StartingRule = _rootRule;
        
        // add operations if needed
      

        var b = new ParserBuilder<IN,OUT>("en");
        var syntaxParser = b.BuildSyntaxParser(_configuration, ParserType.LL_RECURSIVE_DESCENT, _rootRule);
        // initialize starting tokens
        syntaxParser.Init(_configuration,_rootRule);
        return syntaxParser;
    }

    public BuildResult<Parser<IN, OUT>> BuildParser()
    {
        // TODO : build syntax parser
        var syntaxParser = BuildSyntaxParser(new BuildResult<ParserConfiguration<IN, OUT>>());
        
        // TODO : build lexer using explicit tokens
        var tokens = _configuration.GetAllExplicitTokenClauses();
        if (tokens != null && tokens.Any())
        {
            _lexerBuilder.WithExplicitTokens(tokens.Select(x => x.ExplicitToken).ToList());
        }
        var lexer = _lexerBuilder.Build();
        
        SyntaxTreeVisitor<IN, OUT> visitor = new SyntaxTreeVisitor<IN, OUT>(_configuration, _parserInstance);
        
        Parser<IN, OUT> parser = new Parser<IN, OUT>(_i18N, syntaxParser, visitor);
        parser.Configuration = _configuration;
        if (lexer.IsOk)
        {
            parser.Lexer = lexer.Result;
            return new BuildResult<Parser<IN,OUT>>(parser);
        }
        else
        {
            var result = new BuildResult<Parser<IN, OUT>>();
            result.AddErrors(lexer.Errors);
            return result;
        }
    }
    
    public IAotParserBuilder<IN, OUT> WithLexerbuilder(IAotLexerBuilder<IN> lexerBuilder)
    {
        _lexerBuilder = lexerBuilder;
        return this;
    }
   
    #region optins

    public IAotParserBuilder<IN, OUT> UseMemoization(bool use = true)
    {
        _useMemoization = use;
        return this;
    }

    public IAotParserBuilder<IN, OUT> UseBroadenTokenWindow(bool use = true)
    {
        _useBroadenTokenWindow = use;
        return this;
    }

    public IAotParserBuilder<IN, OUT> UseAutoCloseIndentations(bool use = true)
    {
        _useAutoCloseIndentations = use;
        return this;
    }
    
    #endregion
    
    #region rules
    
    public IAotParserBuilder<IN,OUT> Production(string ruleString, Func<object[], OUT> visitor)
    {
        ParserBuilder<IN, OUT> builder = new ParserBuilder<IN, OUT>();
        var (nonTerminaName, clauses) = builder.ExtractNTAndRule(ruleString);
        var rule = builder.BuildNonTerminal(new Tuple<string, string>(nonTerminaName,clauses));
        rule.NonTerminalName =  nonTerminaName;
        rule.RuleString = $"{nonTerminaName} : {ruleString}";
        
        if (rule != null)
        {
            rule.RuleString = ruleString;
            rule.SetLambdaVisitor(visitor);
            AddRule(rule);    
        }

        return this;
    }
    
    #endregion
    
    
  

   
    
    #region tools
    
     private void AddRule(Rule<IN, OUT> rule)
    {
        if (!_nonTerminals.TryGetValue(rule.NonTerminalName, out var nonTerminal))
        {
            nonTerminal = new NonTerminal<IN, OUT>(rule.NonTerminalName);
        }
        nonTerminal.Rules.Add(rule);
        _nonTerminals[rule.NonTerminalName] = nonTerminal;
       
    }

   
    
    #endregion
}