using System;
using sly.lexer;
using sly.lexer.fsm;

namespace ParserTests.lexer
{
    public static class ExtendedGenericLexer
    {
        public static bool CheckDate(ReadOnlyMemory<char> value)
        {
            var ok = false;
            if (value.Length == 6)
            {
                ok = char.IsDigit(value.At(0));
                ok = ok && char.IsDigit(value.At(1));
                ok = ok && value.At(2) == '.';
                ok = ok && char.IsDigit(value.At(3));
                ok = ok && char.IsDigit(value.At(4));
                ok = ok && value.At(5) == '.';
            }

            return ok;
        }

        public static void AddExtension(Extensions token, LexemeAttribute lexem, GenericLexer<Extensions> lexer)
        {
            if (token == Extensions.DATE)
            {
                NodeCallback<GenericToken> callback = match =>
                {
                    match.Properties[GenericLexer<Extensions>.DerivedToken] = Extensions.DATE;
                    match.Result.Channel = 0;
                    return match;
                };

                var fsmBuilder = lexer.FSMBuilder;

                fsmBuilder.GoTo(GenericLexer<Extensions>.in_double)
                    .Transition('.', CheckDate)
                    .Mark("start_date")
                    .RepetitionTransition(4, "[0-9]")
                    .End(GenericToken.Extension)
                    .CallBack(callback);
            }
        }
        
        public static void AddShortExtension(ShortExtensions token, LexemeAttribute lexem, GenericLexer<ShortExtensions> lexer)
        {
            if (token == ShortExtensions.DATE)
            {
                NodeCallback<GenericToken> callback = match =>
                {
                    match.Properties[GenericLexer<ShortExtensions>.DerivedToken] = ShortExtensions.DATE;
                    return match;
                };

                var fsmBuilder = lexer.FSMBuilder;

                fsmBuilder.GoTo(GenericLexer<ShortExtensions>.in_double)
                    .Transition('.', CheckDate)
                    .Mark("start_date")
                    .RepetitionTransition(4, "[0-9]")
                    .End(GenericToken.Extension)
                    .CallBack(callback);
            }

            if (token == ShortExtensions.TEST)
            {
                NodeCallback<GenericToken> callbackTEST = (FSMMatch<GenericToken> match) =>
                {
                    // this store the token id the the FSMMatch object to be later returned by GenericLexer.Tokenize 
                    match.Properties[GenericLexer<ShortExtensions>.DerivedToken] = ShortExtensions.TEST;
                    return match;
                };
                var builder = lexer.FSMBuilder;
                builder.GoTo("start").Transition('#').Mark("in-ext").Transition('#').End(GenericToken.Extension).CallBack(callbackTEST);
                //var builder = lexer.FSMBuilder;
                builder.GoTo("in-ext").TransitionTo('*',"in-ext");
                //var builder = lexer.FSMBuilder;
                builder.GoTo("in-ext").Transition('€').End(GenericToken.Extension).CallBack(callbackTEST);
            }
        }
    }
}