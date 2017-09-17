using System;
using System.Collections.Generic;
using sly.lexer;
using sly.parser.syntax;

namespace sly.parser.generator
{
    public class RuleParser<IN>
    {
        
        [LexerConfiguration]
        public ILexer<EbnfToken> BuildEbnfLexer(ILexer<EbnfToken> lexer)
        {
            lexer.AddDefinition(new TokenDefinition<EbnfToken>(EbnfToken.COLON, ":"));
            lexer.AddDefinition(new TokenDefinition<EbnfToken>(EbnfToken.ONEORMORE, "\\+"));
            lexer.AddDefinition(new TokenDefinition<EbnfToken>(EbnfToken.ZEROORMORE, "\\*"));
            lexer.AddDefinition(new TokenDefinition<EbnfToken>(EbnfToken.IDENTIFIER,
                "[A-Za-z0-9_��������][A-Za-z0-9_��������]*"));
            lexer.AddDefinition(new TokenDefinition<EbnfToken>(EbnfToken.COLON, ":"));
            lexer.AddDefinition(new TokenDefinition<EbnfToken>(EbnfToken.WS, "[ \\t]+", true));
            lexer.AddDefinition(new TokenDefinition<EbnfToken>(EbnfToken.EOL, "[\\n\\r]+", true, true));
            return lexer;
        }

        
         #region rules grammar

        [Production("rule : IDENTIFIER COLON clauses")]
        public object Root(Token<EbnfToken> name, Token<EbnfToken> discarded, ClauseSequence<IN> clauses)
        {
            Rule<IN> rule = new Rule<IN>();
            rule.NonTerminalName = name.Value;
            rule.Clauses = clauses.Clauses;
            return rule;
        }


        [Production("clauses : clause clauses")]

        public object Clauses(IClause<IN> clause, ClauseSequence<IN> clauses)
        {
            ClauseSequence<IN> list = new ClauseSequence<IN>( clause );
            if (clauses != null)
            {
                list.AddRange(clauses);
            }
            return list;
        }

        [Production("clauses : clause ")]
        public ClauseSequence<IN> SingleClause(IClause<IN> clause)
        {
            return new ClauseSequence<IN>(clause); 
        }



       

        [Production("clause : IDENTIFIER ZEROORMORE")]
        public IClause<IN> ZeroMoreClause(Token<EbnfToken> id, Token<EbnfToken> discarded)
        {
            IClause<IN> innerClause = BuildTerminalOrNonTerimal(id.Value);
            return new ZeroOrMoreClause<IN>(innerClause);
        }

        [Production("clause : IDENTIFIER ONEORMORE")]
        public IClause<IN> OneMoreClause(Token<EbnfToken> id, Token<EbnfToken> discarded)
        {
            IClause<IN> innerClause = BuildTerminalOrNonTerimal(id.Value);
            return new OneOrMoreClause<IN>(innerClause);
        }

        [Production("clause : IDENTIFIER ")]
        public IClause<IN> SimpleClause(Token<EbnfToken> id)
        {
            IClause<IN> clause = BuildTerminalOrNonTerimal(id.Value);
            return clause;
        }

        private IClause<IN> BuildTerminalOrNonTerimal(string name)
        {

            IN token = default(IN);
            IClause<IN> clause;
            bool isTerminal = false;
            try
            {
                token = (IN)Enum.Parse(typeof(IN), name, false);
                isTerminal = true;
            }
            catch
            {
                isTerminal = false;
            }
            if (isTerminal)
            {
                clause = new TerminalClause<IN>(token);
            }
            else
            {
                clause = new NonTerminalClause<IN>(name);
            }
            return clause;
        }

        #endregion
    }
}