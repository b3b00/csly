using System.Collections.Generic;
using sly.lexer;
using sly.parser.generator;
using sly.parser.parser;

namespace indented
{
    public class IndentedParser
    {
        
        [Production("id : ID")]
        public Ast id(Token<IndentedLangLexer> tok)
        {
            return new Identifier(tok.Value);
        }

        [Production("int : INT")]
        public Ast integer(Token<IndentedLangLexer> tok)
        {
            return new Integer(tok.IntValue);
        }

        [Production("statement: [set|ifthenelse]")]
        public Ast Statement(Ast stat)
        {
            return stat as Statement;
        }
        
        [Production("set : id SET[d] int")]
        public Ast Set(Identifier id, Integer i)
        {
            return new Set(id, i);
        }
        
        [Production("cond : id EQ[d] int")]
        public Ast Condi(Identifier id, Integer i)
        {
            return new Cond(id, i);
        }

        [Production("root: statement*")]
        public Ast Root(List<Ast> statements)
        {
            return new Block(statements);
        }
        
        [Production("ifthenelse: IF cond block (ELSE[d] block)?")]
        public Ast ifthenelse(Token<IndentedLangLexer> si, Cond cond, Block thenblk, ValueOption<Group<IndentedLangLexer,Ast>> elseblk)
        {
            
            var previous = si.Previous(Channels.Comments);
            string comment = null;
            // previous token may not be a comment so we have to check if not null
            if (previous != null && (previous.TokenID == IndentedLangLexer.SINGLE_COMMENT || previous.TokenID == IndentedLangLexer.MULTI_COMMENT))
            {
                comment = previous?.Value;
            }
            
            var eGrp = elseblk.Match(
                x => {
                return x;
            }, () =>
            {
                return null;
            });
            var eBlk = eGrp?.Value(0) as Block;
            return new IfThenElse(cond, thenblk, eBlk, comment);
        }

        [Production("block : INDENT[d] statement* UINDENT[d]")]
        public Ast Block(List<Ast> statements)
        {
            return new Block(statements);
        }
    }
}