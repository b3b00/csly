using System;
using System.Linq;
using sly.lexer;
using sly.lexer.fsm;

namespace ParserTests.lexer
{
    public static class Issue210Extensions
    {
        
        
        public static void AddExtensions(Issue210Token token, LexemeAttribute lexem, GenericLexer<Issue210Token> lexer)
        {
            if (token == Issue210Token.SPECIAL || token == Issue210Token.QMARK)
            {
                FSMMatch<GenericToken> Callback(FSMMatch<GenericToken> match)
                {
                    var result = match.Result.Value;
                    if (result.Length >= 2)
                    {
                        if (match.Result.Value[0] == '?' && match.Result.Value.Last() == '?')
                        {
                            var section = result.Substring(1, result.Length - 2);

                            match.Result.SpanValue = section.AsMemory();
                            match.Properties[GenericLexer<Issue210Token>.DerivedToken] = Issue210Token.SPECIAL;
                            return match;
                        }
                        Console.WriteLine($"bad lexing {match.Result.Value}");
                        match.Result.SpanValue = null;
                        match.Properties[GenericLexer<Issue210Token>.DerivedToken] = default(Issue210Token);
                        return match;
                    }

                    return match;
                }




                lexer.FSMBuilder.GoTo(GenericLexer<Issue210Token>.start)
                    .SafeTransition('?')
                    .Mark("qmark")
                    .ExceptTransition(new[] {'?'}) // moving to first char of a special
                    .Mark("in_qmark") // now we are really in a potential SPECIAL
                    .ExceptTransitionTo(new[] {'?'}, "in_qmark")
                    .Transition('?') // ending ? of a SPECIAL
                    .End(GenericToken.Extension) // we re done with a SPECIAL
                    .Mark("end_qmark")
                    .CallBack(Callback)
                    .GoTo("qmark")
                    .TransitionTo('?', "end_qmark");
            }
        }
    }
}