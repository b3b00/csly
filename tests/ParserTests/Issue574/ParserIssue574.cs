using sly.lexer;
using sly.parser.generator;
using System.Collections.Generic;
using sly.parser.parser;

/*
 https://b3b00.github.io/CslyViz/grammar#Q1NMWS1NQVJLI2dlbmVyaWNMZXhlciBUb2tlbjsKCltDdXN0b21JZF0gSWRlbnRpZmllciA6ICJfQS1aYS16IiAiXzAtOUEtWmEteiI7CgoKCltTdWdhcl0gQ29tbWEgOiAiLCI7CgpbS2V5V29yZF0gU3BlY2lmaWVyOiAic3BlY2lmaWVyIjsKCgoKI1tLZXlXb3JkXSBJbW11dCA6ICJpbW11dCI7CgoKW0tleVdvcmRdIFR5cGUgOiAidHlwZSI7CgpwYXJzZXIgUGFyc2VyOwoKcm9vdCA6IE5UVHlwZUFuZElkZW50aWZpZXJDU1ZFbGVtZW50KiA7CgotPiBOVFR5cGVBbmRJZGVudGlmaWVyQ1NWRWxlbWVudDogTlRNYW55U3BlY2lmaWNpZXJzIFR5cGUgSWRlbnRpZmllcjsKCk5UTWFueVNwZWNpZmljaWVyczogTlRTcGVjaWZpZXIqOwoKTlRTcGVjaWZpZXI6ICBTcGVjaWZpZXIgIDsKCgoKJCMkIHR5cGUgeA== 
 */

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

        [Production("Item : NTManySpecificiers Type Identifier")]
        public object Item(object p0, Token<TokenIssue574> p1, Token<TokenIssue574> p2)
        {
            return default(object);
        }

        [Production("NTManySpecificiers : NTSpecifier *")]
        public object NTManySpecificiers(List<object> p0)
        {
            return default(object);
        }

        [Production("NTSpecifier :  Specifier ")]
        public object NTSpecifier(Token<TokenIssue574> p0)
        {
            return default(object);
        }

        
    }
}