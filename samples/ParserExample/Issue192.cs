using System;
using System.Collections.Generic;
using System.Linq;
using sly.lexer;
using sly.parser;
using sly.parser.generator;

namespace ParserExample
{


    public enum Tokens
    {
        [Lexeme(@"-?((\d+([.]\d+)?)|([.]\d+))")]
        DOUBLE = 1,
        [Lexeme("[A-Z]")] WORD_CHAR = 2,
        [Lexeme("[ \\t]+", true)] WS = 3,
        
        EOS = 0
    }

    public interface IDumbo
    {
        
    }
    
    public class Word : IDumbo
    {
        public string Char;
        public double Value;
    }

    public class Block: IDumbo
    {
        public IList<Word> Words;
    }

    public class Grammar
    {
        [Production("word: WORD_CHAR DOUBLE")]
        public IDumbo Word(Token<Tokens> charToken, Token<Tokens> valueToken)
        {
            return new Word {Char = charToken.CharValue.ToString(), Value = valueToken.DoubleValue};
        }

        [Production("block: word+")]
        public IDumbo Block(List<IDumbo> tail)
        {
            return new Block {Words = tail.Cast<Word>().ToArray()}; 
        }
    }

    public class Issue192
    {

        public static Parser<Tokens, IDumbo> CreateBlockParser()
        {
            var builder = new ParserBuilder<Tokens, IDumbo>();

            var result = builder.BuildParser(new Grammar(), ParserType.EBNF_LL_RECURSIVE_DESCENT, "block");
            if (!result.IsOk)
                throw new Exception(result.Errors.First().ToString());

            return result.Result;
        }



    }
}