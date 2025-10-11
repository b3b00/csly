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
    
    private Type _expressionType {get;set;}

    public RelaxedVisitorTyper(string i18n)
    {
        _i18n = i18n;
        _nonTerminalTypes =  new Dictionary<string, Type>();
    }
    
    public BuildResult<Parser<IN, OUT>> CheckRelaxedVisitor(BuildResult<Parser<IN, OUT>> result,
        ParserConfiguration<IN, OUT> configuration)
    {
        _parserConfiguration = configuration;
        
        var (r,expressionType) = GetExpressionsType(result,configuration);
        result = r;
        _expressionType = expressionType;
        
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
    
    public (BuildResult<Parser<IN, OUT>> result , Type expressionType) GetExpressionsType(BuildResult<Parser<IN, OUT>> result,
        ParserConfiguration<IN, OUT> configuration)
    {
        if (configuration.UsesOperations)
        {
            var expressionTypes = configuration
                .NonTerminals
                .Values
                .SelectMany(x => x.Rules)
                .Where(x => x.IsExpressionRule)
                .Select(x => x.GetOperations())
                .SelectMany(x => x.Select(y => y.VisitorMethod))
                .Select((x => x.ReturnParameter.ParameterType));
            var grouped = expressionTypes.GroupBy(x => x.FullName).ToList();
            if (grouped.Count > 1)
            {
                string names = string.Join(", ", grouped.SelectMany(x => x.Select(x => x.Name)));
                var message = i18n.I18N.Instance.GetText(_i18n,
                    I18NMessage.ManyTypeForExpressions, names);
                result.AddError(new InitializationError(ErrorLevel.FATAL,
                    message,
                    ErrorCodes.RELAXED_PARSER_EXPRESSIONS_MUST_HAVE_SAME_TYPE));
                return (result, null);
            }
            return (result, expressionTypes.First());
        }
        return (result, null);
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

            if (nonTerminal.Value.Rules.Any(x => x.IsExpressionRule))
            {
                continue; // already checked when searching for expression type
            }
            var returnTypes = nonTerminal.Value.Rules.Select(x => x.GetVisitorMethod().ReturnParameter).Distinct().ToList();
            var t = nonTerminal.Value.Rules.Select(x => x.GetVisitorMethod().ReturnType);
            var group = t.GroupBy(x => x.Name);
            if (group.Count() > 1)
            {
                string names = string.Join(", ", group.Select(x => x.Key));
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
            result = CheckExpressionVisitors(result, rule);
            return result;
        }
        var visitor = rule.GetVisitorMethod();
        if (visitor == null) 
        {
            ;
        }
        var parameters = visitor.GetParameters();
        //  exclude discarded terminals !
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

    private BuildResult<Parser<IN, OUT>> CheckExpressionVisitors(BuildResult<Parser<IN, OUT>> result, Rule<IN, OUT> rule)
    {
        var operations = rule.GetOperations();
        foreach (var operation in operations)
        {
            var operationVisitorvisitor = operation.VisitorMethod;
            if (operation.IsUnary)
            {
                var arg = operation.Affix == Affix.PreFix
                    ? operationVisitorvisitor.GetParameters()[1]
                    : operationVisitorvisitor.GetParameters()[0];
                var operandType = arg.ParameterType;
                if (operandType != _expressionType) 
                {
                    result.AddInitializationError(ErrorLevel.FATAL,
                        i18n.I18N.Instance.GetText(_i18n, I18NMessage.IncorrectVisitorParameterType, operationVisitorvisitor.Name,
                            rule.RuleString, arg.Name, _expressionType.Name, arg.ParameterType.Name),
                        ErrorCodes.PARSER_INCORRECT_VISITOR_PARAMETER_TYPE);   
                }
                
                var oper = operation.Affix == Affix.PreFix
                    ? operationVisitorvisitor.GetParameters()[0]
                    : operationVisitorvisitor.GetParameters()[1];
                var operatorType = oper.ParameterType;
                if (operatorType != typeof(Token<IN>)) 
                {
                    result.AddInitializationError(ErrorLevel.FATAL,
                        i18n.I18N.Instance.GetText(_i18n, I18NMessage.IncorrectVisitorParameterType, operationVisitorvisitor.Name,
                            rule.RuleString, oper.Name, typeof(Token<IN>).Name, oper.ParameterType.Name),
                        ErrorCodes.PARSER_INCORRECT_VISITOR_PARAMETER_TYPE);   
                }
            }
            if (operation.IsBinary)
            {
                var arg = operationVisitorvisitor.GetParameters()[0];
                var operandType = arg.ParameterType;
                if (operandType != _expressionType) 
                {
                    result.AddInitializationError(ErrorLevel.FATAL,
                        i18n.I18N.Instance.GetText(_i18n, I18NMessage.IncorrectVisitorParameterType, operationVisitorvisitor.Name,
                            rule.RuleString, arg.Name, _expressionType.Name, arg.ParameterType.Name),
                        ErrorCodes.PARSER_INCORRECT_VISITOR_PARAMETER_TYPE);   
                }
                arg = operationVisitorvisitor.GetParameters()[2];
                operandType = arg.ParameterType;
                if (operandType != _expressionType) 
                {
                    result.AddInitializationError(ErrorLevel.FATAL,
                        i18n.I18N.Instance.GetText(_i18n, I18NMessage.IncorrectVisitorParameterType, operationVisitorvisitor.Name,
                            rule.RuleString, arg.Name, _expressionType.Name, arg.ParameterType.Name),
                        ErrorCodes.PARSER_INCORRECT_VISITOR_PARAMETER_TYPE);   
                }
                var oper = operationVisitorvisitor.GetParameters()[1];
                var operatorType = oper.ParameterType;
                if (operatorType != typeof(Token<IN>)) 
                {
                    result.AddInitializationError(ErrorLevel.FATAL,
                        i18n.I18N.Instance.GetText(_i18n, I18NMessage.IncorrectVisitorParameterType, operationVisitorvisitor.Name,
                            rule.RuleString, oper.Name, typeof(Token<IN>).Name, oper.ParameterType.Name),
                        ErrorCodes.PARSER_INCORRECT_VISITOR_PARAMETER_TYPE);   
                }
            }
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
                            rule.RuleString, arg.Name, expected.Name, arg.ParameterType.Name),
                        ErrorCodes.PARSER_INCORRECT_VISITOR_PARAMETER_TYPE);
                }

                break;
            }
            case ManyClause<IN, OUT> many:
            {
                Type expected = null;
                if (many.Clause is NonTerminalClause<IN, OUT> ntgrp && ntgrp.IsGroup)
                {
                    var (res, type) = CheckGroup(result, ntgrp, rule);
                    if (res.IsError)
                    {
                        return res;
                    }

                    if (type == null)
                    {
                        // gruop only contains terminals => return all is ok
                        return res;
                    }

                    expected = typeof(List<>).MakeGenericType(
                        typeof(Group<,>).MakeGenericType(typeof(IN), type));

                }

                if (many.Clause is TerminalClause<IN, OUT> || many.Clause is ChoiceClause<IN,OUT> terminalChoice && terminalChoice.IsTerminalChoice)
                {
                    expected = typeof(List<Token<IN>>);
                }

                if (many.Clause is NonTerminalClause<IN, OUT> nt && !nt.IsGroup )
                {
                    if (nt != null && _nonTerminalTypes.TryGetValue(nt.NonTerminalName, out var type))
                    {
                        expected = typeof(List<>).MakeGenericType(type);
                    }
                }

                if (many.Clause is ChoiceClause<IN, OUT> nonTerminaleChoice && nonTerminaleChoice.IsNonTerminalChoice)
                {
                    var choiceCheck = CheckNonterminalChoices(result, rule, nonTerminaleChoice);
                    if (choiceCheck.result.IsError)
                    {
                        return choiceCheck.result;
                    }
                    result =  choiceCheck.result;
                    expected = choiceCheck.choiceType;
                }

                if (!expected.IsAssignableFrom(arg.ParameterType) && arg.ParameterType != expected)
                {
                    result.AddInitializationError(ErrorLevel.FATAL,
                        i18n.I18N.Instance.GetText(_i18n, I18NMessage.IncorrectVisitorParameterType, visitor.Name,
                            rule.RuleString, arg.Name, expected.Name, arg.ParameterType.Name),
                        ErrorCodes.PARSER_INCORRECT_VISITOR_PARAMETER_TYPE);
                }
            
                return result;
            
            }
            case OptionClause<IN, OUT> option:
            {
                Type expected = null;

                if (option.Clause is NonTerminalClause<IN,OUT> ntgrp && ntgrp.IsGroup)
                {
                    var (res, type) = CheckGroup(result, ntgrp, rule);
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

                if (option.Clause is ChoiceClause<IN, OUT> termChoice && termChoice.IsTerminalChoice)
                {
                    expected = typeof(Token<IN>);
                }

                if (option.Clause is ChoiceClause<IN, OUT> nonTermChoice && nonTermChoice.IsNonTerminalChoice)
                {
                    var choiceCheck = CheckNonterminalChoices(result, rule, nonTermChoice);
                    if (choiceCheck.result.IsError)
                    {
                        return choiceCheck.result;
                    }
                    result =  choiceCheck.result;
                    expected = choiceCheck.choiceType;
                }
                
                if (!expected.IsAssignableFrom(arg.ParameterType) && arg.ParameterType != expected)
                {
                    result.AddInitializationError(ErrorLevel.FATAL,
                        i18n.I18N.Instance.GetText(_i18n, I18NMessage.IncorrectVisitorParameterType, visitor.Name,
                            rule.RuleString, arg.Name, expected.Name, arg.ParameterType.Name),
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
                                rule.RuleString, arg.Name, expected.Name, arg.ParameterType.Name),
                            ErrorCodes.PARSER_INCORRECT_VISITOR_PARAMETER_TYPE);
                    }
                }

                break;
            }
        }

        return result;
    }

    private (BuildResult<Parser<IN, OUT>> result, Type choiceType) CheckNonterminalChoices(BuildResult<Parser<IN, OUT>> result, Rule<IN, OUT> rule,
        ChoiceClause<IN, OUT> nonTerminaleChoice)
    {
        // TODO check if all types
        var types = nonTerminaleChoice.Choices.Select<IClause<IN, OUT>, object>(x =>
        {
            if (x is NonTerminalClause<IN, OUT> nt)
            {
                if (_nonTerminalTypes.TryGetValue(nt.NonTerminalName, out var type))
                {
                    return type;
                }
            }

            return null;
        }).Cast<Type>().Where(x => x != null).ToList();
        var grouped = types.GroupBy(x => x?.Name).ToList();
        if (grouped.Count > 1)
        {
            string names = string.Join(", ", grouped.SelectMany(x => x.Select(x => x.Name)));
            var message = i18n.I18N.Instance.GetText(_i18n,
                I18NMessage.ManyTypeInGroup, rule.Dump(), nonTerminaleChoice.Dump(), names);
            result.AddError(new InitializationError(ErrorLevel.FATAL,
                message,
                ErrorCodes.RELAXED_PARSER_NON_TERMINAL_IN_CHOICE_MUST_HAVE_SAME_TYPE_IN_GROUP));
            return (result, null);
        }

        return (result, types[0]);
    }

    private (BuildResult<Parser<IN, OUT>> result, Type groupType) CheckGroup(BuildResult<Parser<IN, OUT>> result, NonTerminalClause<IN, OUT> group,
         Rule<IN,OUT> rule)
    {
        var rules = _parserConfiguration.GetRulesForNonTerminal(group.NonTerminalName);
        var groupTypes = rules[0].Clauses
            .Select<IClause<IN, OUT>, Type>(x =>
            {
                if (x is NonTerminalClause<IN, OUT> nt)
                {
                    if (_nonTerminalTypes.TryGetValue(nt.NonTerminalName, out var type))
                    {
                        return type;
                    }



                    if (rule != null && rule.IsExpressionRule)
                    {
                        return _expressionType;
                    }
                }
            

                if (x is ChoiceClause<IN, OUT> choice && choice.IsNonTerminalChoice)
                {
                    var choiceCheck = CheckNonterminalChoices(result, rule, choice);
                    if (choiceCheck.result.IsError)
                    {
                        return null;
                    }
                    result =  choiceCheck.result;
                    return choiceCheck.choiceType;
                }
                if (x is ChoiceClause<IN,OUT> termChoice && termChoice.IsTerminalChoice)
                {
                    // ignore terminal clauses
                    return null;
                }

                if (x is TerminalClause<IN,OUT>)
                {
                    // ignore terminal clauses
                    return null;
                }

                return default(Type);
            }).ToList();
        if (result.IsError)
        {
            return (result, null);
        }
        groupTypes = groupTypes.Where(x => x != null).ToList();
        var grouped = groupTypes.GroupBy(x => x?.Name ?? "").ToList();
        if (grouped.Count > 1)
        {
            string names = string.Join(", ", grouped.SelectMany(x => x.Select(x => x.Name)));
            var message = i18n.I18N.Instance.GetText(_i18n,
                I18NMessage.ManyTypeInGroup, rule.Dump(), group.Dump(), names);
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