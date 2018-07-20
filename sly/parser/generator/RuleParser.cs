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

           [Production("clause : IDENTIFIER OPTION")]
        public IClause<IN> OptionClause(Token<EbnfToken> id, Token<EbnfToken> discarded)
        {
            IClause<IN> innerClause = BuildTerminalOrNonTerimal(id.Value);
            return new OptionClause<IN>(innerClause);
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


        #region  groups

        


        [Production("clause : LPAREN  groupclauses RPAREN ")] 
        public GroupClause<IN> Group(Token<EbnfToken> discardLeft, GroupClause<IN> clauses, Token<EbnfToken> discardRight)
        {
            return clauses;       
        }

        [Production("clause : LPAREN  groupclauses RPAREN ONEORMORE ")]
        public IClause<IN> GroupOneOrMore(Token<EbnfToken> discardLeft, GroupClause<IN> clauses, Token<EbnfToken> discardRight, Token<EbnfToken> oneZeroOrMore)
        {
            return new OneOrMoreClause<IN>(clauses);
        }

        [Production("clause : LPAREN  groupclauses RPAREN ZEROORMORE ")]
        public IClause<IN> GroupZeroOrMore(Token<EbnfToken> discardLeft, GroupClause<IN> clauses, Token<EbnfToken> discardRight, Token<EbnfToken> discardZeroOrMore)
        {
            return new ZeroOrMoreClause<IN>(clauses);
        }


        [Production("groupclauses : groupclause groupclauses")]

        public object GroupClauses(IClause<IN> clause, GroupClause<IN> clauses)
        {
            GroupClause<IN> group = new GroupClause<IN>( clause );
            if (clauses != null)
            {
                group.AddRange(clauses);
            }
            return group;
        }

        [Production("groupclauses : groupclause")]

        public object GroupClausesOne(IClause<IN> clause)
        {            
            GroupClause<IN> group = new GroupClause<IN>( clause );            
            return group;
        }

        [Production("groupclause : IDENTIFIER ")]
        public GroupClause<IN> GroupClause(Token<EbnfToken> id)
        {
            IClause<IN> clause = BuildTerminalOrNonTerimal(id.Value, true);
            return new GroupClause<IN>(clause); 
        }

        [Production("groupclause : IDENTIFIER DISCARD ")]
        public GroupClause<IN> GroupClauseDiscarded(Token<EbnfToken> id, Token<EbnfToken> discarded)
        {
            IClause<IN> clause = BuildTerminalOrNonTerimal(id.Value, true);
            return new GroupClause<IN>(clause);
        }

        #endregion





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