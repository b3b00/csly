using cpg.parser.parsgenerator.generator;
using lexer;
using parser.parsergenerator.generator;
using parser.parsergenerator.parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cpg.parser.parsgenerator.parser.llparser
{

    class ParserState<T>
    {

        int CurrentTokenPosition { get; set; }

        IList<Token<T>> Tokens { get; set; }

        Rule<T> Rule { get; set; }

        public ParserState(int position, Rule<T> rule, IList<Token<T>> tokens)
        {
            this.CurrentTokenPosition = position;
            this.Rule = rule;
            this.Tokens = tokens;
        }


    }

    public class LLSyntaxParser<T> : ISyntaxParser<T>
    {
        public ParserConfiguration<T> Configuration { get; set; }

        public string StartingNonTerminal { get; set; }

        public LLSyntaxParser(ParserConfiguration<T> configuration, string startingNonTerminal)
        {
            Configuration = configuration;
            StartingNonTerminal = startingNonTerminal;
        }

        public SyntaxParseResult<T> Parse(IList<Token<T>> tokens)
        {
            Dictionary<string, NonTerminal<T>> NonTerminals = Configuration.NonTerminals;

            Stack<ParserState<T>> stack = new Stack<ParserState<T>>();

            NonTerminal<T> nt = NonTerminals[StartingNonTerminal];

            List<Rule<T>> rules = nt.Rules.Where<Rule<T>>(r => r.PossibleLeadingTokens.Contains(tokens[0].TokenID)).ToList<Rule<T>>();

            List<ParserState<T>> states = rules.Select(r => new ParserState<T>(0, r, tokens)).ToList<ParserState<T>>();
            states.Reverse();
            states.ForEach(s => stack.Push(s));

            return null;


        }



    }
}
