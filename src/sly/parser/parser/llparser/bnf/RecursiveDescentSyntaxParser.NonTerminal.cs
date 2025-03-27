using System.Collections.Generic;
using System.Linq;
using sly.lexer;
using sly.parser.syntax.grammar;

namespace sly.parser.llparser.bnf;

public partial class RecursiveDescentSyntaxParser<IN, OUT> where IN : struct
{
    #region parsing

    public SyntaxParseResult<IN, OUT> ParseNonTerminal(IList<Token<IN>> tokens, NonTerminalClause<IN, OUT> nonTermClause,
        int currentPosition, SyntaxParsingContext<IN, OUT> parsingContext)
    {
        var result = ParseNonTerminal(tokens, nonTermClause.NonTerminalName, currentPosition, parsingContext);
        return result;
    }

    public SyntaxParseResult<IN, OUT> ParseNonTerminal(IList<Token<IN>> tokens, string nonTerminalName,
        int currentPosition, SyntaxParsingContext<IN, OUT> parsingContext)
    {
        if (parsingContext.TryGetParseResult(new NonTerminalClause<IN, OUT>(nonTerminalName), currentPosition,
                out var memoizedResult))
        {
            return memoizedResult;
        }

        var startPosition = currentPosition;
        var nt = Configuration.NonTerminals[nonTerminalName];
        var errors = new List<UnexpectedTokenSyntaxError<IN>>();

        var i = 0;
        var rules = nt.Rules;

        var innerRuleErrors = new List<UnexpectedTokenSyntaxError<IN>>();
        var greaterIndex = 0;
        var rulesResults = new List<SyntaxParseResult<IN, OUT>>();
        while (i < rules.Count)
        {
            var innerrule = rules[i];
            if (startPosition < tokens.Count 
                && (!tokens[startPosition].IsEOS || (tokens[startPosition].IsEOS && innerrule.MayBeEmpty)) 
                && innerrule.Match(tokens, startPosition, Configuration))
            {
                var innerRuleRes = Parse(tokens, innerrule, startPosition, nonTerminalName, parsingContext);
                rulesResults.Add(innerRuleRes);

                var other = greaterIndex == 0 && innerRuleRes.EndingPosition == 0;
                var innerRuleResErrors = innerRuleRes.GetErrors();
                if (innerRuleRes.EndingPosition > greaterIndex && innerRuleResErrors != null &&
                    innerRuleResErrors.Count == 0 || other)
                {
                    greaterIndex = innerRuleRes.EndingPosition;
                    if (innerRuleResErrors != null)
                        innerRuleErrors.AddRange(innerRuleResErrors);
                }

                if (innerRuleResErrors != null)
                    innerRuleErrors.AddRange(innerRuleResErrors);
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

            var noMatching = NoMatchingRuleError(tokens, currentPosition, allAcceptableTokens);
            parsingContext.Memoize(new NonTerminalClause<IN, OUT>(nonTerminalName), currentPosition, noMatching);
            return noMatching;
        }

        errors.AddRange(innerRuleErrors);
        SyntaxParseResult<IN, OUT> max = null;
        int okEndingPosition = -1;
        int koEndingPosition = -1;
        bool hasOk = false;
        SyntaxParseResult<IN, OUT> maxOk = null;
        SyntaxParseResult<IN, OUT> maxKo = null;
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

            if (rulesResult.IsError && rulesResult.EndingPosition > koEndingPosition)
            {
                koEndingPosition = rulesResult.EndingPosition;
                maxKo = rulesResult;
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


        var result = new SyntaxParseResult<IN, OUT>();
        result.AddErrors(errors);
        result.Root = max.Root;
        result.EndingPosition = max.EndingPosition;
        result.IsError = max.IsError;
        result.IsEnded = max.IsEnded;
        result.HasByPassNodes = max.HasByPassNodes;


        List<UnexpectedTokenSyntaxError<IN>> terr = new List<UnexpectedTokenSyntaxError<IN>>();
        foreach (var ruleResult in rulesResults)
        {
            var ruleErrors = ruleResult.GetErrors();
            if (ruleErrors != null)
            {
                terr.AddRange(ruleErrors);
            }
        }

        parsingContext.Memoize(new NonTerminalClause<IN, OUT>(nonTerminalName), currentPosition, result);
        return result;
    }

    #endregion
}