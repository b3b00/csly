using sly.i18n;
using sly.parser;
using sly.parser.generator;
using sly.parser.syntax.grammar;

namespace aot.parser;

public class ParserBuilder<IN, OUT> : IProductionBuilder<IN,OUT> where IN : struct
{
 
    
    Dictionary<string, NonTerminal<IN,OUT>> NonTerminals = new Dictionary<string, NonTerminal<IN,OUT>>();

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

    public IProductionBuilder<IN, OUT> Right(IN operation, Func<object[], OUT> visitor)
    {
        throw new NotImplementedException();
    }

    public IProductionBuilder<IN, OUT> Right(string explicitOperation, Func<object[], OUT> visitor)
    {
        throw new NotImplementedException();
    }

    public IProductionBuilder<IN, OUT> Left(IN operation, Func<object[], OUT> visitor)
    {
        throw new NotImplementedException();
    }

    public IProductionBuilder<IN, OUT> Left(string explicitOperation, Func<object[], OUT> visitor)
    {
        throw new NotImplementedException();
    }

    public IProductionBuilder<IN, OUT> Prefix(IN operation, Func<object[], OUT> visitor)
    {
        throw new NotImplementedException();
    }

    public IProductionBuilder<IN, OUT> Prefix(string explicitOperation, Func<object[], OUT> visitor)
    {
        throw new NotImplementedException();
    }

    public IProductionBuilder<IN, OUT> Postfix(IN operation, Func<object[], OUT> visitor)
    {
        throw new NotImplementedException();
    }

    public IProductionBuilder<IN, OUT> Postfix(string explicitOperation, Func<object[], OUT> visitor)
    {
        throw new NotImplementedException();
    }

    public ISyntaxParser<IN, OUT> Build()
    {
        throw new NotImplementedException();
    }
}