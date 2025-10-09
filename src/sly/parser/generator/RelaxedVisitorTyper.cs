using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using sly.buildresult;
using sly.i18n;
using sly.lexer;
using sly.parser.parser;
using sly.parser.syntax.grammar;

namespace sly.parser.generator;

public class RelaxedVisitorTyper<IN, OUT> where IN : struct, Enum
{
    
    private string _i18n {get; set;}
    
    private IDictionary<string, Type> _nonTerminalTypes {get;set;}
    
    private ParserConfiguration<IN,OUT> _parserConfiguration {get;set;}

    public RelaxedVisitorTyper(string i18n)
    {
        _i18n = i18n;
        _nonTerminalTypes =  new Dictionary<string, Type>();
    }
    
    public BuildResult<Parser<IN, OUT>> CheckRelaxedVisitor(BuildResult<Parser<IN, OUT>> result,
        ParserConfiguration<IN, OUT> configuration)
    {
        _parserConfiguration = configuration;
        result = CheckNonTerminalTypes(result, configuration);
        if (result.IsError)
        {
            return result;
        }
        foreach (var nonTerminal in configuration.NonTerminals)
        {
            result = CheckNonTerminal(result, nonTerminal.Value);
        }

        return result;
    }

    private BuildResult<Parser<IN, OUT>> CheckNonTerminalTypes(BuildResult<Parser<IN, OUT>> result,
        ParserConfiguration<IN, OUT> configuration)
    {
        foreach (var nonTerminal in configuration.NonTerminals)
        {
            if (nonTerminal.Value.IsSubRule)
            {
                continue;
            }
            var returnTypes = nonTerminal.Value.Rules.Select(x => x.GetVisitorMethod().ReturnParameter).Distinct().ToList();
            var t = nonTerminal.Value.Rules.Select(x => x.GetVisitorMethod().ReturnType);
            var group = t.GroupBy(x => x.FullName);
            if (group.Count() > 1)
            {
                string names = string.Join(", ", group.Select(x => x.SelectMany(x => x.Name)));
                var message = i18n.I18N.Instance.GetText(_i18n,
                    I18NMessage.ManyReturnTypeForNonTerminal, nonTerminal.Value.Name, names);
                result.AddError(new InitializationError(ErrorLevel.FATAL,
                    message,
                    ErrorCodes.RELAXED_PARSER_MANY_RETURN_TYPE_FOR_NONTERMINAL));
            }
            else
            {
                _nonTerminalTypes[nonTerminal.Key] = returnTypes[0].ParameterType;
            }
        }

        return result;
    }

    private BuildResult<Parser<IN, OUT>> CheckNonTerminal(BuildResult<Parser<IN, OUT>> result,
        NonTerminal<IN, OUT> nonTerminal)
    {
        if (nonTerminal.IsSubRule)
        {
            return result;
            // TODO ?
        }
        

        foreach (var rule in nonTerminal.Rules)
        {
            CheckVisitorSignature(result, rule);
        }

        return result;
    }

    private BuildResult<Parser<IN, OUT>> CheckVisitorSignature(BuildResult<Parser<IN, OUT>> result,
        Rule<IN, OUT> rule)
    {
        if (rule.IsExpressionRule)
        {
            return result; // TODO
        }
        var visitor = rule.GetVisitorMethod();
        var parameters = rule.GetVisitorMethod().GetParameters();
        // TODO : exclude discarded terminals !
        var realClauses = rule.Clauses.Where(x => !(x is TerminalClause<IN,OUT> term && term.Discarded)).ToList();
        if (parameters.Length != realClauses.Count)
        {
            result.AddError(new InitializationError(ErrorLevel.FATAL, i18n.I18N.Instance.GetText(_i18n,
                I18NMessage.IncorrectVisitorParameterNumber, visitor.Name,
                rule.RuleString, rule.Clauses.Count.ToString(), (rule.Clauses.Count + 1).ToString(),
                visitor.GetParameters().Length.ToString()), ErrorCodes.PARSER_INCORRECT_VISITOR_PARAMETER_NUMBER));
            return result;
        }

        for (int i = 0; i < realClauses.Count; i++)
        {
            var clause = realClauses[i];
            
            
            var arg = parameters[i];

            result = CheckVisitorArgument(result, rule, visitor, clause, arg);
        }

        return result;
    }

    private BuildResult<Parser<IN, OUT>> CheckVisitorArgument(BuildResult<Parser<IN, OUT>> result, Rule<IN, OUT> rule,
        MethodInfo visitor,
        IClause<IN, OUT> clause, ParameterInfo arg)
    {
        switch (clause)
        {
            case TerminalClause<IN, OUT> terminal:
            {
                var expected = typeof(Token<IN>);
                if (!expected.IsAssignableFrom(arg.ParameterType) && arg.ParameterType != expected)
                {
                    result.AddInitializationError(ErrorLevel.FATAL,
                        i18n.I18N.Instance.GetText(_i18n, I18NMessage.IncorrectVisitorParameterType, visitor.Name,
                            rule.RuleString, arg.Name, expected.FullName, arg.ParameterType.FullName),
                        ErrorCodes.PARSER_INCORRECT_VISITOR_PARAMETER_TYPE);
                }

                break;
            }
            case ManyClause<IN, OUT> many:
            {
                Type expected = null;
                if (many.Clause is NonTerminalClause<IN, OUT> ntgrp && ntgrp.IsGroup)
                {
                    var (res, type) = CheckGroup(result, ntgrp, rule.Dump());
                    if (res.IsError)
                    {
                        return res;
                    }

                    expected = typeof(List<>).MakeGenericType(
                        typeof(Group<,>).MakeGenericType(typeof(IN), type));

                }

                if (many.Clause is TerminalClause<IN, OUT>)
                {
                    expected = typeof(List<Token<IN>>);
                }

                if (many.Clause is NonTerminalClause<IN, OUT> nt && !nt.IsGroup)
                {
                    if (_nonTerminalTypes.TryGetValue(nt.NonTerminalName, out var type))
                    {
                        expected = typeof(List<>).MakeGenericType(type);
                    }
                }
                
                if (!expected.IsAssignableFrom(arg.ParameterType) && arg.ParameterType != expected)
                {
                    result.AddInitializationError(ErrorLevel.FATAL,
                        i18n.I18N.Instance.GetText(_i18n, I18NMessage.IncorrectVisitorParameterType, visitor.Name,
                            rule.RuleString, arg.Name, expected.FullName, arg.ParameterType.FullName),
                        ErrorCodes.PARSER_INCORRECT_VISITOR_PARAMETER_TYPE);
                }
            
                return result;
            
            }
            case OptionClause<IN, OUT> option:
            {
                Type expected = null;

                if (option.Clause is NonTerminalClause<IN,OUT> ntgrp && ntgrp.IsGroup)
                {
                    var (res, type) = CheckGroup(result, ntgrp, rule.Dump());
                    if (res.IsError)
                    {
                        return res;
                    }
                    expected = typeof(ValueOption<>).MakeGenericType(typeof(Group<,>).MakeGenericType(typeof(IN), type));
                    
                }

                if (option.Clause is TerminalClause<IN, OUT>)
                {
                    expected = typeof(Token<IN>);
                }

                if (option.Clause is NonTerminalClause<IN, OUT> nt && !nt.IsGroup)
                {
                    if (_nonTerminalTypes.TryGetValue(nt.NonTerminalName, out var type))
                    {
                        expected = typeof(ValueOption<>).MakeGenericType(type);
                    }
                }
                
                if (!expected.IsAssignableFrom(arg.ParameterType) && arg.ParameterType != expected)
                {
                    result.AddInitializationError(ErrorLevel.FATAL,
                        i18n.I18N.Instance.GetText(_i18n, I18NMessage.IncorrectVisitorParameterType, visitor.Name,
                            rule.RuleString, arg.Name, expected.FullName, arg.ParameterType.FullName),
                        ErrorCodes.PARSER_INCORRECT_VISITOR_PARAMETER_TYPE);
                }
            
                return result;
            
            }
            case NonTerminalClause<IN, OUT> nonTerminal:
            {
                if (_nonTerminalTypes.TryGetValue(nonTerminal.NonTerminalName, out var expected))
                {

                    if (!expected.IsAssignableFrom(arg.ParameterType) && arg.ParameterType != expected)
                    {
                        result.AddInitializationError(ErrorLevel.FATAL,
                            i18n.I18N.Instance.GetText(_i18n, I18NMessage.IncorrectVisitorParameterType, visitor.Name,
                                rule.RuleString, arg.Name, expected.FullName, arg.ParameterType.FullName),
                            ErrorCodes.PARSER_INCORRECT_VISITOR_PARAMETER_TYPE);
                    }
                }

                break;
            }
        }

        return result;
    }

    private (BuildResult<Parser<IN, OUT>> result, Type groupType) CheckGroup(BuildResult<Parser<IN, OUT>> result, NonTerminalClause<IN, OUT> group,
         string rule)
    {
        var rules = _parserConfiguration.GetRulesForNonTerminal(group.NonTerminalName);
        var groupTypes = rules[0].Clauses
            .Where(x => x is NonTerminalClause<IN, OUT>)
            .Cast<NonTerminalClause<IN, OUT>>()
            .Select(x =>
            {
                if (_nonTerminalTypes.TryGetValue(x.NonTerminalName, out var type))
                {
                    return type;
                }

                return null;
            }).ToList();
        var grouped = groupTypes.GroupBy(x => x.FullName).ToList();
        if (grouped.Count > 1)
        {
            string names = string.Join(", ", grouped.SelectMany(x => x.Select(x => x.Name)));
            var message = i18n.I18N.Instance.GetText(_i18n,
                I18NMessage.ManyTypeInGroup, rule, group.Dump(), names);
            result.AddError(new InitializationError(ErrorLevel.FATAL,
                message,
                ErrorCodes.RELAXED_PARSER_NON_TERMINAL_MUST_HAVE_SAME_TYPE_IN_GROUP));
            return (result, null);
        }
        /*
        
            */
        return (result, groupTypes.SingleOrDefault());
    }
}