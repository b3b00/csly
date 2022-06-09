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
                    match.Properties[GenericLexer<Extensions>.DerivedToken] = Extensions.DATE;
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
    }
}