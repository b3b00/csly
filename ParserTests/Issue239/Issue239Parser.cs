using System.Collections.Generic;
using sly.lexer;
using sly.parser.generator;

namespace ParserTests.Issue239
{
    public class Issue239Parser
    {
        [Production("statements : statement*")]
        public object Statements(List<object> statements)
        {
            return statements;
        }

        [Production("statement: INT[d] ID SEMI[d]")]
        public object IntDeclaration(Token<Issue239Lexer> id)
        {
            return $"{id.Value} is an int;\n";
        }

        [Production("statement : ID ASSIGN[d] INT_LITERAL SEMI[d]")]
        public object Assignement(Token<Issue239Lexer> id, Token<Issue239Lexer> value)
        {
            return $"{id.Value} is equal to {value.IntValue}\n";
        }
        
        
            
    }
}