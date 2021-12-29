using System;
using sly.lexer;
using sly.parser.syntax.grammar;

namespace sly.parser.generator
{
    public class RuleParser<IN,OUT> where IN : struct
    {
        #region rules grammar

        [Production("rule : IDENTIFIER COLON clauses")]
        public GrammarNode<IN,OUT> Root(Token<EbnfTokenGeneric> name, Token<EbnfTokenGeneric> discarded, ClauseSequence<IN,OUT> clauses)
        {
            var rule = new Rule<IN,OUT>();
            rule.NonTerminalName = name.Value;
            rule.Clauses = clauses.Clauses;
            return rule;
        }


        [Production("clauses : clause clauses")]
        public GrammarNode<IN,OUT> Clauses(IClause<IN,OUT> clause, ClauseSequence<IN,OUT> clauses)
        {
            var list = new ClauseSequence<IN,OUT>(clause);
            if (clauses != null) list.AddRange(clauses);
            return list;
        }

        [Production("clauses : clause ")]
        public ClauseSequence<IN,OUT> SingleClause(IClause<IN,OUT> clause)
        {
            return new ClauseSequence<IN,OUT>(clause);
        }


        [Production("clause : IDENTIFIER ZEROORMORE")]
        public IClause<IN,OUT> ZeroMoreClause(Token<EbnfTokenGeneric> id, Token<EbnfTokenGeneric> discarded)
        {
            var innerClause = BuildTerminalOrNonTerimal(id.Value, true);
            return new ZeroOrMoreClause<IN,OUT>(innerClause);
        }

        [Production("clause : IDENTIFIER ONEORMORE")]
        public IClause<IN,OUT> OneMoreClause(Token<EbnfTokenGeneric> id, Token<EbnfTokenGeneric> discarded)
        {
            var innerClause = BuildTerminalOrNonTerimal(id.Value);
            return new OneOrMoreClause<IN,OUT>(innerClause);
        }

        [Production("clause : IDENTIFIER OPTION")]
        public IClause<IN,OUT> OptionClause(Token<EbnfTokenGeneric> id, Token<EbnfTokenGeneric> discarded)
        {
            var innerClause = BuildTerminalOrNonTerimal(id.Value);
            return new OptionClause<IN,OUT>(innerClause);
        }

        [Production("clause : IDENTIFIER DISCARD ")]
        public IClause<IN,OUT> SimpleDiscardedClause(Token<EbnfTokenGeneric> id, Token<EbnfTokenGeneric> discard)
        {
            var clause = BuildTerminalOrNonTerimal(id.Value, true);
            return clause;
        }

        [Production("clause : choiceclause DISCARD")]
        public IClause<IN,OUT> AlternateDiscardedClause(ChoiceClause<IN,OUT> choices, Token<EbnfTokenGeneric> discarded)
        {
            choices.IsDiscarded = true;
            return choices;
        }
        
        [Production("clause : choiceclause")]
        public IClause<IN,OUT> AlternateClause(ChoiceClause<IN,OUT> choices)
        {
            choices.IsDiscarded = false;
            return choices;
        }

        [Production("choiceclause : LCROG  choices RCROG  ")]
        public IClause<IN,OUT> AlternateChoices(Token<EbnfTokenGeneric> discardleft, IClause<IN,OUT> choices, Token<EbnfTokenGeneric> discardright)
        {            
            return choices;
        }
        
        [Production("choices : IDENTIFIER  ")]
        public IClause<IN,OUT> ChoicesOne(Token<EbnfTokenGeneric> head)
        {
            var choice = BuildTerminalOrNonTerimal(head.Value);
            return new ChoiceClause<IN,OUT>(choice);
        }
        
        [Production("choices : IDENTIFIER OR choices ")]
        public IClause<IN,OUT> ChoicesMany(Token<EbnfTokenGeneric> head, Token<EbnfTokenGeneric> discardOr, ChoiceClause<IN,OUT> tail)
        {
            var headClause = BuildTerminalOrNonTerimal(head.Value); 
            return new ChoiceClause<IN,OUT>(headClause,tail.Choices);
        }
        

        [Production("clause : IDENTIFIER ")]
        public IClause<IN,OUT> SimpleClause(Token<EbnfTokenGeneric> id)
        {
            var clause = BuildTerminalOrNonTerimal(id.Value);
            return clause;
        }


        #region  groups

        [Production("clause : LPAREN  groupclauses RPAREN ")]
        public GroupClause<IN,OUT> Group(Token<EbnfTokenGeneric> discardLeft, GroupClause<IN,OUT> clauses,
            Token<EbnfTokenGeneric> discardRight)
        {
            return clauses;
        }
        
        [Production("clause : choiceclause ONEORMORE ")]
        public IClause<IN,OUT> ChoiceOneOrMore(ChoiceClause<IN,OUT> choices,Token<EbnfTokenGeneric> discardOneOrMore)
        {
            return new OneOrMoreClause<IN,OUT>(choices);
        }

        [Production("clause : choiceclause ZEROORMORE ")]
        public IClause<IN,OUT> ChoiceZeroOrMore(ChoiceClause<IN,OUT> choices,Token<EbnfTokenGeneric> discardZeroOrMore)
        {
            return new ZeroOrMoreClause<IN,OUT>(choices);
        }
        

        [Production("clause : choiceclause OPTION ")]
        public IClause<IN,OUT> ChoiceOptional(ChoiceClause<IN,OUT> choices,Token<EbnfTokenGeneric> discardOption)
        {
            return new OptionClause<IN,OUT>(choices);
        }

        [Production("clause : LPAREN  groupclauses RPAREN ONEORMORE ")]
        public IClause<IN,OUT> GroupOneOrMore(Token<EbnfTokenGeneric> discardLeft, GroupClause<IN,OUT> clauses,
            Token<EbnfTokenGeneric> discardRight, Token<EbnfTokenGeneric> oneZeroOrMore)
        {
            return new OneOrMoreClause<IN,OUT>(clauses);
        }

        [Production("clause : LPAREN  groupclauses RPAREN ZEROORMORE ")]
        public IClause<IN,OUT> GroupZeroOrMore(Token<EbnfTokenGeneric> discardLeft, GroupClause<IN,OUT> clauses,
            Token<EbnfTokenGeneric> discardRight, Token<EbnfTokenGeneric> discardZeroOrMore)
        {
            return new ZeroOrMoreClause<IN,OUT>(clauses);
        }

        [Production("clause : LPAREN  groupclauses RPAREN OPTION ")]
        public IClause<IN,OUT> GroupOptional(Token<EbnfTokenGeneric> discardLeft, GroupClause<IN,OUT> group,
            Token<EbnfTokenGeneric> discardRight, Token<EbnfTokenGeneric> option)
        {
            return new OptionClause<IN,OUT>(group);
        }


        [Production("groupclauses : groupclause groupclauses")]
        public GroupClause<IN,OUT> GroupClauses(GroupClause<IN,OUT> group, GroupClause<IN,OUT> groups)
        {
            if (groups != null) group.AddRange(groups);
            return group;
        }

        [Production("groupclauses : groupclause")]
        public GroupClause<IN,OUT> GroupClausesOne(GroupClause<IN,OUT> group)
        {
            return group;
        }

        [Production("groupclause : IDENTIFIER ")]
        public GroupClause<IN,OUT> GroupClause(Token<EbnfTokenGeneric> id)
        {
            var clause = BuildTerminalOrNonTerimal(id.Value);
            return new GroupClause<IN,OUT>(clause);
        }

        [Production("groupclause : IDENTIFIER DISCARD ")]
        public GroupClause<IN,OUT> GroupClauseDiscarded(Token<EbnfTokenGeneric> id, Token<EbnfTokenGeneric> discarded)
        {
            var clause = BuildTerminalOrNonTerimal(id.Value, true);
            return new GroupClause<IN,OUT>(clause);
        }

        [Production("groupclause : choiceclause ")]
        public GroupClause<IN,OUT> GroupChoiceClause(ChoiceClause<IN,OUT> choices)
        {
            return new GroupClause<IN,OUT>(choices);
        }



        #endregion


        private IClause<IN,OUT> BuildTerminalOrNonTerimal(string name, bool discard = false)
        {
            var token = default(IN);
            IClause<IN,OUT> clause;
            var isTerminal = false;
            var b = Enum.TryParse(name, out token);
            if (b)
            {
                isTerminal = true;
            }

            if (isTerminal)
                clause = new TerminalClause<IN,OUT>(token, discard);
            else
            {
                if (name == "INDENT")
                {
                    clause = new IndentTerminalClause<IN,OUT>(IndentationType.Indent,discard);
                }
                else if (name == "UINDENT")
                {
                    clause = new IndentTerminalClause<IN,OUT>(IndentationType.UnIndent,discard);
                }
                else
                {
                    clause = new NonTerminalClause<IN,OUT>(name);
                }
            }

            return clause;
        }

        #endregion
    }
}