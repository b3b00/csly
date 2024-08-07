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

public class AotEBNFParserBuilder<IN, OUT> : IAotEBNFParserBuilder<IN,OUT> where IN : struct
{
    readonly Dictionary<string, NonTerminal<IN,OUT>> _nonTerminals = new Dictionary<string, NonTerminal<IN,OUT>>();

    readonly Dictionary<int, List<OperationMetaData<IN, OUT>>> _operationsByPrecedence = new Dictionary<int, List<OperationMetaData<IN, OUT>>>();

    private bool _useMemoization = false;

    private bool _useBroadenTokenWindow = false;

    private bool _useAutoCloseIndentations = false;

    private readonly List<Rule<IN, OUT>> _operands = new List<Rule<IN, OUT>>();

    private readonly Parser<EbnfTokenGeneric, GrammarNode<IN, OUT>> _grammarParser = null;

    private string _i18N;

    private string _rootRule;

    private object _parserInstance;
    
    private IAotLexerBuilder<IN> _lexerBuilder;

    private Action<IN, LexemeAttribute, GenericLexer<IN>> _extensionBuilder = null;
    
    private LexerPostProcess<IN> _lexerPostProcess = null;

    private ParserConfiguration<IN, OUT> _configuration = null;
    public static IAotEBNFParserBuilder<IN,OUT> NewBuilder<IN,OUT>(object parserInstance, string rootRule, string i18N = "en") where IN : struct
    {
        return new AotEBNFParserBuilder<IN,OUT>(i18N, parserInstance, rootRule);
    }
    
    private AotEBNFParserBuilder(string i18N, object parserInstance, string rootRule)
    {
        var ruleparser = new RuleParser<IN,OUT>();
        var grammarParserBuilder = new ParserBuilder<EbnfTokenGeneric, GrammarNode<IN,OUT>>(i18N);
        _i18N = i18N;
        _parserInstance = parserInstance;
        _rootRule = rootRule;
        var gpb = 
            AotParserBuilder<EbnfTokenGeneric, GrammarNode<IN, OUT>>.NewBuilder<EbnfTokenGeneric, GrammarNode<IN, OUT>>(new RuleParser<IN, OUT>(),"rule");

        var b = new AotRuleParser<IN, OUT>();
        var grammar = b.BuildParser(i18N);
        _grammarParser = grammar;
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
        _configuration.UsesOperations = _operationsByPrecedence != null && _operationsByPrecedence.Any();
        _configuration.OperandRules = _operands;
        
        // add operations if needed
        if (_configuration.UsesOperations)
        {
            // TODO AOT : operands !
            ExpressionRulesGenerator<IN, OUT> expressionRulesGenerator = new ExpressionRulesGenerator<IN, OUT>(_i18N);
            var ok = expressionRulesGenerator.GenerateExpressionParserRules(_configuration, _parserInstance.GetType(),
                result,
                _operationsByPrecedence, out var expressionResult);
            if (!ok)
            {
                return null;
            }
        }

        var b = new EBNFParserBuilder<IN,OUT>("en");
        var syntaxParser = b.BuildSyntaxParser(_configuration, ParserType.EBNF_LL_RECURSIVE_DESCENT, _rootRule);
        // initialize starting tokens
        syntaxParser.Init(_configuration,_rootRule);
        return syntaxParser;
    }

    public Parser<IN, OUT> BuildParser()
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
        
        SyntaxTreeVisitor<IN, OUT> visitor = new EBNFSyntaxTreeVisitor<IN, OUT>(_configuration, _parserInstance);
        
        Parser<IN, OUT> parser = new Parser<IN, OUT>(_i18N, syntaxParser, visitor);
        parser.Configuration = _configuration;
        parser.Lexer = lexer;
        
        
        return parser;
    }
    
    public IAotEBNFParserBuilder<IN, OUT> WithLexerbuilder(IAotLexerBuilder<IN> lexerBuilder)
    {
        _lexerBuilder = lexerBuilder;
        return this;
    }
   
    #region optins

    public IAotEBNFParserBuilder<IN, OUT> UseMemoization(bool use = true)
    {
        _useMemoization = use;
        return this;
    }

    public IAotEBNFParserBuilder<IN, OUT> UseBroadenTokenWindow(bool use = true)
    {
        _useBroadenTokenWindow = use;
        return this;
    }

    public IAotEBNFParserBuilder<IN, OUT> UseAutoCloseIndentations(bool use = true)
    {
        _useAutoCloseIndentations = use;
        return this;
    }
    
    #endregion
    
    #region rules
    
    public IAotEBNFParserBuilder<IN,OUT> Production(string ruleString, Func<object[], OUT> visitor)
    {
        var r = _grammarParser.Parse(ruleString);
        if (r.IsOk)
        {
            var rule = r.Result as Rule<IN, OUT>;
            rule.RuleString = ruleString;
            rule.SetLambdaVisitor(visitor);
            AddRule(rule, false);    
        }
        else
        {
            throw new InvalidOperationException(
                $"error while parsing EBNF rule : >{ruleString}< : {string.Join(" || ", r.Errors.Select(x => x.ErrorMessage))}");
        }

        return this;
    }
    
    #endregion
    
    
    #region operations
    public IAotEBNFParserBuilder<IN, OUT> Operand(string ruleString, Func<object[], OUT> visitor)
    {
        var r = _grammarParser.Parse(ruleString);
        if (r.IsOk)
        {
            var rule = r.Result as Rule<IN, OUT>;
            rule.SetVisitor(visitor);
            AddRule(rule, true);    
        }

        return this;
    }

    public IAotEBNFParserBuilder<IN, OUT> Right(int precedence, IN operation, Func<object[], OUT> visitor)
    {
        AddOperation(precedence,Associativity.Right,visitor,Affix.InFix,operation,null);
        return this;
    }

    public IAotEBNFParserBuilder<IN, OUT> Right(int precedence, string explicitOperation, Func<object[], OUT> visitor)
    {
        AddOperation(precedence,Associativity.Right,visitor,Affix.InFix,explicitOperation, null);
        return this;
    }

    public IAotEBNFParserBuilder<IN, OUT> Left(int precedence, IN operation, Func<object[], OUT> visitor)
    {
        AddOperation(precedence,Associativity.Left,visitor,Affix.InFix,operation,null);
        return this;
    }

    public IAotEBNFParserBuilder<IN, OUT> Left(int precedence, string explicitOperation, Func<object[], OUT> visitor)
    {
        AddOperation(precedence,Associativity.Left,visitor,Affix.InFix,explicitOperation,null);
        return this;
    }

    public IAotEBNFParserBuilder<IN, OUT> Prefix(int precedence, IN operation, Func<object[], OUT> visitor)
    {
       AddOperation(precedence,Associativity.None,visitor,Affix.PreFix,operation,null);
       return this;
    }

    public IAotEBNFParserBuilder<IN, OUT> Prefix(int precedence, string explicitOperation, Func<object[], OUT> visitor)
    {
        AddOperation(precedence,Associativity.None,visitor,Affix.PreFix,explicitOperation,null);
        return this;
    }

    public IAotEBNFParserBuilder<IN, OUT> Postfix(int precedence, IN operation, Func<object[], OUT> visitor)
    {
        AddOperation(precedence,Associativity.None,visitor,Affix.PostFix,operation,null);
        return this;
    }

    public IAotEBNFParserBuilder<IN, OUT> Postfix(int precedence, string explicitOperation, Func<object[], OUT> visitor)
    {
        AddOperation(precedence,Associativity.None,visitor,Affix.PostFix,explicitOperation,null);
        return this;
    }


    #endregion

   
    
    #region tools
    
     private void AddRule(Rule<IN, OUT> rule, bool operand = false)
    {
        if (!_nonTerminals.TryGetValue(rule.NonTerminalName, out var nonTerminal))
        {
            nonTerminal = new NonTerminal<IN, OUT>(rule.NonTerminalName);
        }
        nonTerminal.Rules.Add(rule);
        _nonTerminals[rule.NonTerminalName] = nonTerminal;
        if (operand)
        {
            _operands.Add(rule);
        }
    }

    private void AddOperation(int precedence,Associativity associativity, Func<object[],OUT> visitor, Affix affix, IN operation, string nodeName = null)
    {
        Func<object[], OUT> loggedVisitor = visitor;
        
        OperationMetaData<IN, OUT> operationMeta =
            new OperationMetaData<IN, OUT>(precedence, Associativity.None, loggedVisitor, affix, operation, nodeName);
        
        List<OperationMetaData<IN,OUT >> operationsForPrecedence;
        if (!_operationsByPrecedence.TryGetValue(precedence, out operationsForPrecedence))
        {
            operationsForPrecedence = new List<OperationMetaData<IN, OUT>>();
        }
        operationsForPrecedence.Add(operationMeta);
        _operationsByPrecedence[precedence] = operationsForPrecedence;
    }
    
    private void AddOperation(int precedence,Associativity associativity, Func<object[],OUT> visitor, Affix affix, string operation, string nodeName = null) 
    {
        Func<object[], OUT> loggedVisitor = visitor;
        
        OperationMetaData<IN, OUT> operationMeta =
            new OperationMetaData<IN, OUT>(precedence, Associativity.None, loggedVisitor, Affix.PreFix, operation, nodeName);
        
        List<OperationMetaData<IN,OUT >> operationsForPrecedence;
        if (!_operationsByPrecedence.TryGetValue(precedence, out operationsForPrecedence))
        {
            operationsForPrecedence = new List<OperationMetaData<IN, OUT>>();
        }
        operationsForPrecedence.Add(operationMeta);
        _operationsByPrecedence[precedence] = operationsForPrecedence;
    }
    
    #endregion
}