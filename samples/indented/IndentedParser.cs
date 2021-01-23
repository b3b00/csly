using System.Collections.Generic;
using sly.lexer;
using sly.parser.generator;

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
        
        [Production("ifthenelse: IF[d] cond block ELSE[d] block")]
        public Ast ifthenelse(Cond cond, Block thenblk, Block elseblk)
        {
            return new IfThenElse(cond, thenblk, elseblk);
        }

        [Production("block : INDENT[d] statement* UINDENT[d]")]
        public Ast Block(List<Ast> statements)
        {
            return new Block(statements);
        }
    }
}