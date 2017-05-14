using sly.lexer;
using System.Linq;
using sly.parser.generator;

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using sly.parser.syntax;

namespace sly.parser.generator
{

   
    /// <summary>
    /// this class provides API to build parser
    /// </summary>
    public class EBNFParserBuilder<T>
    {

        

        [LexerConfigurationAttribute]
        public  ILexer<EbnfToken> BuildJsonLexer(ILexer<EbnfToken> lexer)
        {                  
            lexer.AddDefinition(new TokenDefinition<EbnfToken>(EbnfToken.COLON, ":"));
            lexer.AddDefinition(new TokenDefinition<EbnfToken>(EbnfToken.PLUS, "\\+"));            
            lexer.AddDefinition(new TokenDefinition<EbnfToken>(EbnfToken.STAR, "\\*"));
            lexer.AddDefinition(new TokenDefinition<EbnfToken>(EbnfToken.IDENTIFIER, "[a-z]"));            
            lexer.AddDefinition(new TokenDefinition<EbnfToken>(EbnfToken.COLON, ":"));            
            lexer.AddDefinition(new TokenDefinition<EbnfToken>(EbnfToken.WS, "[ \\t]+", true));
            lexer.AddDefinition(new TokenDefinition<EbnfToken>(EbnfToken.EOL, "[\\n\\r]+", true, true));
            return lexer;
        }

        [Reduction("rule : IDENTIFIER COLON clauses")]
        public  object Root(Token<EbnfToken> name, Token<EbnfToken> discarded, List<IClause<T>> clauses)
        {
            return null;
        }


    }
}