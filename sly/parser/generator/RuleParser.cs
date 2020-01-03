using System;
using sly.lexer;
using sly.parser.syntax.grammar;

namespace sly.parser.generator
{
    public class RuleParser<IN> where IN : struct
    {
        #region rules grammar

        [Production("rule : IDENTIFIER COLON clauses")]
        public object Root(Token<EbnfTokenGeneric> name, Token<EbnfTokenGeneric> discarded, ClauseSequence<IN> clauses)
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
        public IClause<IN> ZeroMoreClause(Token<EbnfTokenGeneric> id, Token<EbnfTokenGeneric> discarded)
        {
            var innerClause = BuildTerminalOrNonTerimal(id.Value, true);
            return new ZeroOrMoreClause<IN>(innerClause);
        }

        [Production("clause : IDENTIFIER ONEORMORE")]
        public IClause<IN> OneMoreClause(Token<EbnfTokenGeneric> id, Token<EbnfTokenGeneric> discarded)
        {
            var innerClause = BuildTerminalOrNonTerimal(id.Value);
            return new OneOrMoreClause<IN>(innerClause);
        }

        [Production("clause : IDENTIFIER OPTION")]
        public IClause<IN> OptionClause(Token<EbnfTokenGeneric> id, Token<EbnfTokenGeneric> discarded)
        {
            var innerClause = BuildTerminalOrNonTerimal(id.Value);
            return new OptionClause<IN>(innerClause);
        }

        [Production("clause : IDENTIFIER DISCARD ")]
        public IClause<IN> SimpleDiscardedClause(Token<EbnfTokenGeneric> id, Token<EbnfTokenGeneric> discard)
        {
            var clause = BuildTerminalOrNonTerimal(id.Value, true);
            return clause;
        }

        [Production("clause : choiceclause DISCARD")]
        public IClause<IN> AlternateDiscardedClause(ChoiceClause<IN> choices, Token<EbnfTokenGeneric> discarded)
        {
            choices.IsDiscarded = true;
            return choices;
        }
        
        [Production("clause : choiceclause")]
        public IClause<IN> AlternateClause(ChoiceClause<IN> choices)
        {
            choices.IsDiscarded = false;
            return choices;
        }

        [Production("choiceclause : LCROG  choices RCROG  ")]
        public IClause<IN> AlternateChoices(Token<EbnfTokenGeneric> discardleft, IClause<IN> choices, Token<EbnfTokenGeneric> discardright)
        {
            // TODO
            return choices;
        }
        
        [Production("choices : IDENTIFIER  ")]
        public IClause<IN> ChoicesOne(Token<EbnfTokenGeneric> head)
        {
            // TODO
            var choice = BuildTerminalOrNonTerimal(head.Value);
            return new ChoiceClause<IN>(choice);
        }
        
        [Production("choices : IDENTIFIER OR choices ")]
        public IClause<IN> ChoicesMany(Token<EbnfTokenGeneric> head, Token<EbnfTokenGeneric> discardOr, ChoiceClause<IN> tail)
        {
            var headClause = BuildTerminalOrNonTerimal(head.Value); 
            return new ChoiceClause<IN>(headClause,tail.Choices);
        }
        

        [Production("clause : IDENTIFIER ")]
        public IClause<IN> SimpleClause(Token<EbnfTokenGeneric> id)
        {
            var clause = BuildTerminalOrNonTerimal(id.Value);
            return clause;
        }


        #region  groups

        [Production("clause : LPAREN  groupclauses RPAREN ")]
        public GroupClause<IN> Group(Token<EbnfTokenGeneric> discardLeft, GroupClause<IN> clauses,
            Token<EbnfTokenGeneric> discardRight)
        {
            return clauses;
        }
        
        [Production("clause : choiceclause ONEORMORE ")]
        public IClause<IN> ChoiceOneOrMore(ChoiceClause<IN> choices,Token<EbnfTokenGeneric> discardOneOrMore)
        {
            return new OneOrMoreClause<IN>(choices);
        }

        [Production("clause : choiceclause ZEROORMORE ")]
        public IClause<IN> ChoiceZeroOrMore(ChoiceClause<IN> choices,Token<EbnfTokenGeneric> discardZeroOrMore)
        {
            return new ZeroOrMoreClause<IN>(choices);
        }
        

        [Production("clause : choiceclause OPTION ")]
        public IClause<IN> ChoiceOptional(ChoiceClause<IN> choices,Token<EbnfTokenGeneric> discardOption)
        {
            return new OptionClause<IN>(choices);
        }

        [Production("clause : LPAREN  groupclauses RPAREN ONEORMORE ")]
        public IClause<IN> GroupOneOrMore(Token<EbnfTokenGeneric> discardLeft, GroupClause<IN> clauses,
            Token<EbnfTokenGeneric> discardRight, Token<EbnfTokenGeneric> oneZeroOrMore)
        {
            return new OneOrMoreClause<IN>(clauses);
        }

        [Production("clause : LPAREN  groupclauses RPAREN ZEROORMORE ")]
        public IClause<IN> GroupZeroOrMore(Token<EbnfTokenGeneric> discardLeft, GroupClause<IN> clauses,
            Token<EbnfTokenGeneric> discardRight, Token<EbnfTokenGeneric> discardZeroOrMore)
        {
            return new ZeroOrMoreClause<IN>(clauses);
        }

        [Production("clause : LPAREN  groupclauses RPAREN OPTION ")]
        public IClause<IN> GroupOptional(Token<EbnfTokenGeneric> discardLeft, GroupClause<IN> group,
            Token<EbnfTokenGeneric> discardRight, Token<EbnfTokenGeneric> option)
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
        public GroupClause<IN> GroupClause(Token<EbnfTokenGeneric> id)
        {
            var clause = BuildTerminalOrNonTerimal(id.Value);
            return new GroupClause<IN>(clause);
        }

        [Production("groupclause : IDENTIFIER DISCARD ")]
        public GroupClause<IN> GroupClauseDiscarded(Token<EbnfTokenGeneric> id, Token<EbnfTokenGeneric> discarded)
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