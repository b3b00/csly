using System;
using System.Collections.Generic;
using sly.lexer;
using sly.parser.syntax;

namespace sly.parser.generator
{
    public class RuleParser<IN> where IN : struct
    {
        
        
        
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
            IClause<IN> innerClause = BuildTerminalOrNonTerimal(id.Value,true);
            return new ZeroOrMoreClause<IN>(innerClause);
        }

        [Production("clause : IDENTIFIER ONEORMORE")]
        public IClause<IN> OneMoreClause(Token<EbnfToken> id, Token<EbnfToken> discarded)
        {
            IClause<IN> innerClause = BuildTerminalOrNonTerimal(id.Value);
            return new OneOrMoreClause<IN>(innerClause);
        }

        [Production("clause : IDENTIFIER DISCARD ")]
        public IClause<IN> SimpleDiscardedClause(Token<EbnfToken> id, Token<EbnfToken> discard)
        {
            IClause<IN> clause = BuildTerminalOrNonTerimal(id.Value, true);
            return clause;
        }

        [Production("clause : IDENTIFIER ")]
        public IClause<IN> SimpleClause(Token<EbnfToken> id)
        {
            IClause<IN> clause = BuildTerminalOrNonTerimal(id.Value);
            return clause;
        }

       



        private IClause<IN> BuildTerminalOrNonTerimal(string name, bool discard = false)
        {

            IN token = default(IN);
            IClause<IN> clause;
            bool isTerminal = false;
            bool b = Enum.TryParse<IN>(name, out token);
            if (b)
            {
                isTerminal = true;
                ;
            }
            if (isTerminal)
            {
                clause = new TerminalClause<IN>(token,discard);
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