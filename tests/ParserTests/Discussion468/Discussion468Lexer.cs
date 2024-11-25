using System;
using sly.lexer;
using sly.lexer.fsm;

namespace ParserTests.Discussion468
{
    public enum Discussion468Lexer
    {
        [CustomId("_.:a-zA-Z","_.:0-9a-zA-Z")]
        IDENTIFIER

    }

    public enum Discussion468WithExtensionLexer
    {
        [Extension]
        IDENTIFIER
    }

    public class Discussion468Extension
    {
        public static void AddExtension(Discussion468WithExtensionLexer token, LexemeAttribute lexem, GenericLexer<Discussion468WithExtensionLexer> lexer) {
            if (token == Discussion468WithExtensionLexer.IDENTIFIER) {

   	
                // precondition to check if starting date matches the dd.mm format
                // Func<string, bool> checkDate = (string value) => {
                //     bool ok = false;
                //     if (value.Length==5) {
                //         ok = char.IsDigit(value[0]);
                //         ok = ok && char.IsDigit(value[1]);
                //         ok = ok && value[2] == '.';
                //         ok = ok && char.IsDigit(value[3]);
                //         ok = ok && char.IsDigit(value[4]);
                //     }            
                //     return ok;
                // }

                // callback on end_date node 
                NodeCallback<GenericToken> callback = (FSMMatch<GenericToken> match) => 
                {
                    // this store the token id the the FSMMatch object to be later returned by GenericLexer.Tokenize 
                    match.Properties[GenericLexer<Discussion468WithExtensionLexer>.DerivedToken] = Discussion468WithExtensionLexer.IDENTIFIER;
                    return match;
                };
   	
                var fsmBuilder = lexer.FSMBuilder;

   	
                fsmBuilder.GoTo(GenericLexer<Discussion468WithExtensionLexer>.start) // start a start node
                    .Transition('.') // add a transition on '.' with precondition
                    .Mark("id")
                    .GoTo(GenericLexer<Discussion468WithExtensionLexer>.start)
                    .TransitionTo(':',"id") 
                    .GoTo(GenericLexer<Discussion468WithExtensionLexer>.start)
                    .TransitionTo('_',"id") 
                    .GoTo(GenericLexer<Discussion468WithExtensionLexer>.start)
                    .TransitionTo('.',"id") 
                    .GoTo(GenericLexer<Discussion468WithExtensionLexer>.start)
                    .RangeTransitionTo('a','z',"id")
                    .GoTo(GenericLexer<Discussion468WithExtensionLexer>.start)
                    .RangeTransitionTo('A','Z',"id")
                    // then begin to loop on "id" node adding the digits transitions
                    .TransitionTo(':',"id")
                    .TransitionTo('.',"id")
                    .TransitionTo('_',"id")
                    .RangeTransitionTo('a','z',"id")
                    .RangeTransitionTo('A','Z',"id")
                    .RangeTransitionTo('0','9',"id")
                    .End(GenericToken.Extension) // mark as ending node 
                    .CallBack(callback); // set the ending callback that will tag the token as IDNENTIFIER
            }
        }
    }
}