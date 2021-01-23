using System.Collections.Generic;
using sly.lexer;
using sly.parser.generator;
using System.Linq;
using System.Text;

namespace indented
{
    
    [Lexer(Indentation = "    ",IndentationAWare = true)]
    public enum IndentedLangLexer
    {
        [Lexeme(GenericToken.Identifier, IdentifierType.Alpha)]
        ID = 1,

        [Lexeme(GenericToken.KeyWord, "if")] IF = 2,

        [Lexeme(GenericToken.KeyWord, "else")] ELSE = 3,

        [Lexeme(GenericToken.SugarToken, "==")] EQ = 4,

        [Lexeme(GenericToken.SugarToken, "=")] SET = 5,

        [Lexeme(GenericToken.Int)] INT = 6,
        
    }

    public interface Ast
    {
        public string Dump(string tab);
    }

    public class Identifier : Ast
    {
        public string Name { get; set; }
        
        public Identifier(string name)
        {
            Name = name;
        }

        public string Dump(string tab)
        {
            return Name;
        }
    }

    public class Integer : Ast
    {
        public int Value { get; set; }

        public Integer(int value)
        {
            Value = value;
        }

        public string Dump(string tab)
        {
            return Value.ToString();
        }
    }

    public class Cond : Ast
    {
        public Identifier Id { get; set; }
        
        public Integer Value { get; set; }

        public Cond(Identifier id, Integer integer)
        {
            Id = id;
            Value = integer;
        }

        public string Dump(string tab)
        {
            return $"{Id.Dump(tab)} == {Value.Dump(tab)}";
        }
    }
    
    public class Set : Statement
    {
        public Identifier Id { get; set; }
        
        public Integer Value { get; set; }

        public Set(Identifier id, Integer integer)
        {
            Id = id;
            Value = integer;
        }
        
        public string Dump(string tab)
        {
            return $"{Id.Dump(tab)} = {Value.Dump(tab)}";
        }
    }

    public class IfThenElse : Statement
    {
        public Cond Cond { get; set; }

        public Block Then { get; set; }

        public Block Else { get; set; }

        public IfThenElse(Cond cond, Block thenBlock, Block elseBlock)
        {
            Cond = cond;
            Then = thenBlock;
            Else = elseBlock;
        }

        public string Dump(string tab)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("IF")
                .AppendLine(Cond.Dump(tab))
                .Append(Then.Dump(tab));
            if (Else != null)
            {
                builder.AppendLine("ELSE");
                builder.AppendLine(Else.Dump(tab));
            }
            
            return builder.ToString();
        }
    }

    public interface Statement : Ast
    {
        
    }
    
    public class Block : Statement
    {
        public List<Ast> Statements { get; set; } = new List<Ast>();

        public Block(List<Ast> stats)
        {
            Statements = stats;
        }

        public string Dump(string tab)
        {
            StringBuilder builder = new StringBuilder();
            string newTab = tab + "\t";
            foreach (var statement in Statements)
            {
                builder.Append(newTab).AppendLine(statement.Dump(newTab));
            }

            return builder.ToString();
        }
    } 


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