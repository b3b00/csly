using System.Collections.Generic;
using sly.lexer;
using sly.parser.generator;
using sly.parser.parser;

namespace indented
{
    public class IndentedParser2
    {
        
        [Production("id : ID")]
        public Ast id(Token<IndentedLangLexer2> tok)
        {
            return new Identifier(tok.Value);
        }

        [Production("int : INT")]
        public Ast integer(Token<IndentedLangLexer2> tok)
        {
            return new Integer(tok.IntValue);
        }

        // [Production("empty: EOL[d]")]
        // public Ast empty()
        // {
        //     return new EmptyLine();
        // }


        [Production("statement: [set|ifthenelse]? EOL[d]")]
        public Ast Statement(ValueOption<Ast> stat)
        {
            return stat.Match(
                x => x as Statement,
                () => new EmptyLine()
            );
        }
        
        [Production("set : id SET[d] int ")]
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
        
        [Production("ifthenelse: IF cond EOL[d] block ELSE[d] EOL[d] block ")]
        public Ast ifthenelse(Token<IndentedLangLexer2> si, Cond cond, Block thenblk, Block elseblk)
        {
            var previous = si.Previous(Channels.Comments);
            string comment = null;
            // previous token may not be a comment so we have to check if not null
            if (previous != null && (previous.TokenID == IndentedLangLexer2.SINGLE_COMMENT || previous.TokenID == IndentedLangLexer2.MULTI_COMMENT))
            {
                comment = previous?.Value;
            }
            return new IfThenElse(cond, thenblk, elseblk,comment);
        }

        [Production("block : INDENT[d] statement* UINDENT[d]")]
        public Ast Block(List<Ast> statements)
        {
            return new Block(statements);
        }
    }
}