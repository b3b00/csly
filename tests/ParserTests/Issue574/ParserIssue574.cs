using sly.lexer;
using sly.parser.generator;
using System.Collections.Generic;
using sly.parser.parser;

namespace ParserTests.Issue574
{
    [ParserRoot("Root")]
    public class ParserIssue574
    {
        [Production("root : Item *")]
        public object root_NTTypeAndIdentifierCSVElement_(List<object> p0)
        {
            return default(object);
        }

        [Production("Item : NTManySpecificiers NTType Identifier")]
        public object NTTypeAndIdentifierCSVElement_NTManySpecificiers_NTType_Identifier(object p0, object p1, Token<TokenIssue574> p2)
        {
            return default(object);
        }

        [Production("NTManySpecificiers : NTSpecifier *")]
        public object NTManySpecificiers_NTSpecifier_(List<object> p0)
        {
            return default(object);
        }

        [Production("NTSpecifier : [ Specifier1 | Specifier2 ]")]
        public object NTSpecifier_Specifier1_Specifier2_(Token<TokenIssue574> p0)
        {
            return default(object);
        }

        [Production("NTType : NTBaseType")]
        public object NTType_NTBaseType(object p0)
        {
            return default(object);
        }

        [Production("NTBaseType : Type")]
        public object NTBaseType_Type(Token<TokenIssue574> p0)
        {
            return default(object);
        }
    }
}