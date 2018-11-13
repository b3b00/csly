using System;
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
            var rule = new Rule<IN>();
            rule.NonTerminalName = name.Value;
            rule.Clauses = clauses.Clauses;
            return rule;
        }


        [Production("clauses : clause clauses")]
        public object Clauses(IClause<IN> clause, ClauseSequence<IN> clauses)
        {
            var list = new ClauseSequence<IN>(clause);
            if (clauses != null) list.AddRange(clauses);
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
            var innerClause = BuildTerminalOrNonTerimal(id.Value, true);
            return new ZeroOrMoreClause<IN>(innerClause);
        }

        [Production("clause : IDENTIFIER ONEORMORE")]
        public IClause<IN> OneMoreClause(Token<EbnfToken> id, Token<EbnfToken> discarded)
        {
            var innerClause = BuildTerminalOrNonTerimal(id.Value);
            return new OneOrMoreClause<IN>(innerClause);
        }

        [Production("clause : IDENTIFIER OPTION")]
        public IClause<IN> OptionClause(Token<EbnfToken> id, Token<EbnfToken> discarded)
        {
            var innerClause = BuildTerminalOrNonTerimal(id.Value);
            return new OptionClause<IN>(innerClause);
        }

        [Production("clause : IDENTIFIER DISCARD ")]
        public IClause<IN> SimpleDiscardedClause(Token<EbnfToken> id, Token<EbnfToken> discard)
        {
            var clause = BuildTerminalOrNonTerimal(id.Value, true);
            return clause;
        }

        [Production("clause : IDENTIFIER ")]
        public IClause<IN> SimpleClause(Token<EbnfToken> id)
        {
            var clause = BuildTerminalOrNonTerimal(id.Value);
            return clause;
        }


        #region  groups

        [Production("clause : LPAREN  groupclauses RPAREN ")]
        public GroupClause<IN> Group(Token<EbnfToken> discardLeft, GroupClause<IN> clauses,
            Token<EbnfToken> discardRight)
        {
            return clauses;
        }

        [Production("clause : LPAREN  groupclauses RPAREN ONEORMORE ")]
        public IClause<IN> GroupOneOrMore(Token<EbnfToken> discardLeft, GroupClause<IN> clauses,
            Token<EbnfToken> discardRight, Token<EbnfToken> oneZeroOrMore)
        {
            return new OneOrMoreClause<IN>(clauses);
        }

        [Production("clause : LPAREN  groupclauses RPAREN ZEROORMORE ")]
        public IClause<IN> GroupZeroOrMore(Token<EbnfToken> discardLeft, GroupClause<IN> clauses,
            Token<EbnfToken> discardRight, Token<EbnfToken> discardZeroOrMore)
        {
            return new ZeroOrMoreClause<IN>(clauses);
        }

        [Production("clause : LPAREN  groupclauses RPAREN OPTION ")]
        public IClause<IN> GroupOptional(Token<EbnfToken> discardLeft, GroupClause<IN> group,
            Token<EbnfToken> discardRight, Token<EbnfToken> option)
        {
            return new OptionClause<IN>(group);
        }


        [Production("groupclauses : groupclause groupclauses")]
        public object GroupClauses(GroupClause<IN> group, GroupClause<IN> groups)
        {
            if (groups != null) group.AddRange(groups);
            return group;
        }

        [Production("groupclauses : groupclause")]
        public object GroupClausesOne(GroupClause<IN> group)
        {
            return group;
        }

        [Production("groupclause : IDENTIFIER ")]
        public GroupClause<IN> GroupClause(Token<EbnfToken> id)
        {
            var clause = BuildTerminalOrNonTerimal(id.Value);
            return new GroupClause<IN>(clause);
        }

        [Production("groupclause : IDENTIFIER DISCARD ")]
        public GroupClause<IN> GroupClauseDiscarded(Token<EbnfToken> id, Token<EbnfToken> discarded)
        {
            var clause = BuildTerminalOrNonTerimal(id.Value, true);
            return new GroupClause<IN>(clause);
        }

        #endregion


        private IClause<IN> BuildTerminalOrNonTerimal(string name, bool discard = false)
        {
            var token = default(IN);
            IClause<IN> clause;
            var isTerminal = false;
            var b = Enum.TryParse(name, out token);
            if (b)
            {
                isTerminal = true;
                ;
            }

            if (isTerminal)
                clause = new TerminalClause<IN>(token, discard);
            else
                clause = new NonTerminalClause<IN>(name);
            return clause;
        }

        #endregion
    }
}