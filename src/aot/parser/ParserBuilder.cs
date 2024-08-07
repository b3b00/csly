using sly.i18n;
using sly.parser;
using sly.parser.generator;
using sly.parser.syntax.grammar;

namespace aot.parser;

public class ParserBuilder<IN, OUT> : IProductionBuilder<IN,OUT> where IN : struct
{
 
    
    Dictionary<string, NonTerminal<IN,OUT>> NonTerminals = new Dictionary<string, NonTerminal<IN,OUT>>();
    
    Dictionary<int, List<OperationMetaData<IN, OUT>>> OperationsByPrecedence = new Dictionary<int, List<OperationMetaData<IN, OUT>>>();

    private Parser<EbnfTokenGeneric, GrammarNode<IN, OUT>> GrammarParser = null;
    
    public static IProductionBuilder<IN,OUT> NewBuilder<IN,OUT>(string i18n = "en") where IN : struct
    {
        return new ParserBuilder<IN,OUT>(i18n);
    }
    
    private ParserBuilder(string i18n)
    {
        var ruleparser = new RuleParser<IN,OUT>();
        var builder = new sly.parser.generator.ParserBuilder<EbnfTokenGeneric, GrammarNode<IN,OUT>>(i18n);

        GrammarParser = builder.BuildParser(ruleparser, ParserType.LL_RECURSIVE_DESCENT, "rule").Result;

    }

    private void AddRule(Rule<IN, OUT> rule)
    {
        if (!NonTerminals.TryGetValue(rule.NonTerminalName, out var nonTerminal))
        {
            nonTerminal = new NonTerminal<IN, OUT>(rule.NonTerminalName);
        }
        nonTerminal.Rules.Add(rule);
        NonTerminals[rule.NonTerminalName] = nonTerminal;
    }

    private void AddOperation(int precedence,Associativity associativity, Func<object[],OUT> visitor, Affix affix, IN operation, string nodeName) 
    {
        OperationMetaData<IN, OUT> operationMeta =
            new OperationMetaData<IN, OUT>(precedence, Associativity.None, visitor, Affix.PreFix, operation, nodeName);
        
        List<OperationMetaData<IN,OUT >> operationsForPrecedence;
        if (!OperationsByPrecedence.TryGetValue(precedence, out operationsForPrecedence))
        {
            operationsForPrecedence = new List<OperationMetaData<IN, OUT>>();
        }
        operationsForPrecedence.Add(operationMeta);
        OperationsByPrecedence[precedence] = operationsForPrecedence;
    }
    
    private void AddOperation(int precedence,Associativity associativity, Func<object[],OUT> visitor, Affix affix, string operation, string nodeName) 
    {
        OperationMetaData<IN, OUT> operationMeta =
            new OperationMetaData<IN, OUT>(precedence, Associativity.None, visitor, Affix.PreFix, operation, nodeName);
        
        List<OperationMetaData<IN,OUT >> operationsForPrecedence;
        if (!OperationsByPrecedence.TryGetValue(precedence, out operationsForPrecedence))
        {
            operationsForPrecedence = new List<OperationMetaData<IN, OUT>>();
        }
        operationsForPrecedence.Add(operationMeta);
        OperationsByPrecedence[precedence] = operationsForPrecedence;
    }
    
    public IProductionBuilder<IN,OUT> Production(string ruleString, Func<object[], OUT> visitor)
    {
        var r = GrammarParser.Parse(ruleString);
        if (r.IsOk)
        {
            var rule = r.Result as Rule<IN, OUT>;
            AddRule(rule);    
        }

        return this;
    }
    
    public IProductionBuilder<IN, OUT> Operand(string ruleString, Func<object[], OUT> visitor)
    {
        // TODO : operand 
        var r = GrammarParser.Parse(ruleString);
        if (r.IsOk)
        {
            var rule = r.Result as Rule<IN, OUT>;
            AddRule(rule);    
        }

        return this;
    }

    public IProductionBuilder<IN, OUT> Right(int precedence, IN operation, Func<object[], OUT> visitor)
    {
        AddOperation(precedence,Associativity.Right,visitor,Affix.InFix,operation,"");
        return this;
    }

    public IProductionBuilder<IN, OUT> Right(int precedence, string explicitOperation, Func<object[], OUT> visitor)
    {
        AddOperation(precedence,Associativity.Right,visitor,Affix.InFix,explicitOperation,"");
        return this;
    }

    public IProductionBuilder<IN, OUT> Left(int precedence, IN operation, Func<object[], OUT> visitor)
    {
        AddOperation(precedence,Associativity.Left,visitor,Affix.InFix,operation,"");
        return this;
    }

    public IProductionBuilder<IN, OUT> Left(int precedence, string explicitOperation, Func<object[], OUT> visitor)
    {
        AddOperation(precedence,Associativity.Left,visitor,Affix.InFix,explicitOperation,"");
        return this;
    }

    public IProductionBuilder<IN, OUT> Prefix(int precedence, IN operation, Func<object[], OUT> visitor)
    {
       AddOperation(precedence,Associativity.None,visitor,Affix.PreFix,operation,"");
       return this;
    }

    public IProductionBuilder<IN, OUT> Prefix(int precedence, string explicitOperation, Func<object[], OUT> visitor)
    {
        AddOperation(precedence,Associativity.None,visitor,Affix.PreFix,explicitOperation,"");
        return this;
    }

    public IProductionBuilder<IN, OUT> Postfix(int precedence, IN operation, Func<object[], OUT> visitor)
    {
        AddOperation(precedence,Associativity.None,visitor,Affix.PostFix,operation,"");
        return this;
    }

    public IProductionBuilder<IN, OUT> Postfix(int precedence, string explicitOperation, Func<object[], OUT> visitor)
    {
        AddOperation(precedence,Associativity.None,visitor,Affix.PostFix,explicitOperation,"");
        return this;
    }

    public ISyntaxParser<IN, OUT> Build()
    {
        throw new NotImplementedException();
    }
}