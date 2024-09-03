using cslyGenerator;
using sly.lexer;
using sly.lexer.fsm;
using sly.parser.generator;

namespace aot;


    public enum ExtendedLexer 
    {
        [Extension]
        EXT,
        
        [Sugar("-")]
        DASH,
        
        [AlphaNumId]
        ID
        
    }

    [ParserGenerator(typeof(ExtendedLexer),typeof(ExtendedLexerParser),typeof(string))]
    public partial class ExtendedLexerGenerator : AbstractParserGenerator<ExtendedLexer>
    {
        public override Action<ExtendedLexer, LexemeAttribute, GenericLexer<ExtendedLexer>> UseTokenExtensions()
        {
            var e = (ExtendedLexer token, LexemeAttribute lexem, GenericLexer<ExtendedLexer> lexer) =>
            {
                if (token == ExtendedLexer.EXT)
                {
                    NodeCallback<GenericToken> callback = (FSMMatch<GenericToken> match) =>
                    {
                        match.Properties[GenericLexer<ExtendedLexer>.DerivedToken] = ExtendedLexer.EXT;
                        return match;
                    };

                    var fsmBuilder = lexer.FSMBuilder;


                    fsmBuilder.GoTo(GenericLexer<ExtendedLexer>.start) 
                        .Transition('$')
                        .Transition('_')
                        .Transition('$')
                        .End(GenericToken.Extension) // mark as ending node 
                        .CallBack(callback); // set the ending callback
                }
            };
            return e;
        }
    }

    public class ExtendedLexerParser
    {
        [Production("root : EXT* DASH[d] ID* ")]
        public string Root(List<Token<ExtendedLexer>> exts, List<Token<ExtendedLexer>> ids)
        {
            return
                $"EXT({string.Join(", ", exts.Select(x => x.Value))}) - IDS({string.Join(", ", ids.Select(x => x.Value))})";
        }
    }
