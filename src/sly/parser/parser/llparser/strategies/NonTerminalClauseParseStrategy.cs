using System.Collections.Generic;
using sly.lexer;
using sly.parser.syntax.grammar;

namespace sly.parser.llparser.strategies;

public class NonTerminalClauseParseStrategy<IN, OUT> : AbstractClauseParseStrategy<IN, OUT> where IN : struct
{
    public override SyntaxParseResult<IN> Parse(IClause<IN> clause, IList<Token<IN>> tokens, int position,
        SyntaxParsingContext<IN> parsingContext)
    {
        var nonTerminal = clause as NonTerminalClause<IN>;
        var nonTerminalName = nonTerminal.NonTerminalName;
        if (parsingContext.TryGetParseResult(new NonTerminalClause<IN>(nonTerminalName), position,
                out var memoizedResult))
        {
            return memoizedResult;
        }

        var startPosition = position;
        var nt = Configuration.NonTerminals[nonTerminalName];
        var errors = new List<UnexpectedTokenSyntaxError<IN>>();

        var i = 0;
        var rules = nt.Rules;

        var innerRuleErrors = new List<UnexpectedTokenSyntaxError<IN>>();
        var greaterIndex = 0;
        var rulesResults = new List<SyntaxParseResult<IN>>();
        while (i < rules.Count)
        {
            var innerrule = rules[i];
            if (startPosition < tokens.Count && !tokens[startPosition].IsEOS &&
                innerrule.Match(tokens, startPosition, Configuration))
            {
                // TODO : call the great strateguerre here
                var innerRuleRes = Strategist.Parse(tokens, innerrule, startPosition, nonTerminalName, parsingContext);
                rulesResults.Add(innerRuleRes);

                var other = greaterIndex == 0 && innerRuleRes.EndingPosition == 0;
                if (innerRuleRes.EndingPosition > greaterIndex && innerRuleRes.Errors != null &&
                    innerRuleRes.Errors.Count == 0 || other)
                {
                    greaterIndex = innerRuleRes.EndingPosition;
                    //innerRuleErrors.Clear();
                    innerRuleErrors.AddRange(innerRuleRes.Errors);
                }

                innerRuleErrors.AddRange(innerRuleRes.Errors);
            }
            i++;
        }

        if (rulesResults.Count == 0)
        {
            var allAcceptableTokens = new List<LeadingToken<IN>>();
            nt.Rules.ForEach(r =>
            {
                if (r != null && r.PossibleLeadingTokens != null)
                    allAcceptableTokens.AddRange(r.PossibleLeadingTokens);
            });
            // allAcceptableTokens = allAcceptableTokens.ToList();

            var noMatching = NoMatchingRuleError(tokens, position, allAcceptableTokens);
            parsingContext.Memoize(new NonTerminalClause<IN>(nonTerminalName), position, noMatching);
            return noMatching;
        }

        errors.AddRange(innerRuleErrors);
        SyntaxParseResult<IN> max = null;
        int okEndingPosition = -1;
        int koEndingPosition = -1;
        bool hasOk = false;
        SyntaxParseResult<IN> maxOk = null;
        SyntaxParseResult<IN> maxKo = null;
        foreach (var rulesResult in rulesResults)
        {
            if (rulesResult.IsOk)
            {
                hasOk = true;
                if (rulesResult.EndingPosition > okEndingPosition)
                {
                    okEndingPosition = rulesResult.EndingPosition;
                    maxOk = rulesResult;
                }
            }

            if (rulesResult.IsError)
            {
                if (rulesResult.EndingPosition > koEndingPosition)
                {
                    koEndingPosition = rulesResult.EndingPosition;
                    maxKo = rulesResult;
                }
            }
        }

        if (hasOk)
        {
            max = maxOk;
        }
        else
        {
            max = maxKo;
        }


        var result = new SyntaxParseResult<IN>();
        result.Errors = errors;
        result.Root = max.Root;
        result.EndingPosition = max.EndingPosition;
        result.IsError = max.IsError;
        result.IsEnded = max.IsEnded;
        result.HasByPassNodes = max.HasByPassNodes;

        if (rulesResults.Count > 0)
        {
            List<UnexpectedTokenSyntaxError<IN>> terr = new List<UnexpectedTokenSyntaxError<IN>>();
            foreach (var ruleResult in rulesResults)
            {
                terr.AddRange(ruleResult.Errors);
                foreach (var err in ruleResult.Errors)
                {
                    result.AddExpectings(err.ExpectedTokens);
                }
            }
        }

        parsingContext.Memoize(new NonTerminalClause<IN>(nonTerminalName),position,result);
        return result;
    }
}