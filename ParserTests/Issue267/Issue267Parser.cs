using System;
using System.Collections.Generic;
using System.Text;

using sly.lexer;
using sly.parser.generator;

namespace ParserTests.Issue267
{

    public class Result167
    {
        public string Name { get; set; }
        public double Value { get; set; }
    }
    public class Issue267Parser
    {
        [Production("program : Declare Identifier Equal[d] Double")]
        public Result167 Program(Token<Issue267Token> decl,Token<Issue267Token> id, Token<Issue267Token> value)
        {
            return new Result167() { Name = id.StringWithoutQuotes, Value = value.DoubleValue };
        }
    }
}
