using Xunit;
using sly.parser;
using sly.parser.generator;
using csly.whileLang.parser;
using csly.whileLang.model;


namespace ParserTests
{

    public class WhileTests
    {

        private static Parser<WhileToken, WhileAST> Parser;


        public WhileTests()
        {
        }


        public Parser<WhileToken, WhileAST> buildParser()
        {
            if (Parser == null)
            {
                WhileParser whileParser = new WhileParser();
                ParserBuilder<WhileToken, WhileAST> builder = new ParserBuilder<WhileToken, WhileAST>();
                Parser = builder.BuildParser(whileParser, ParserType.EBNF_LL_RECURSIVE_DESCENT, "statement");
                ;
            }
            return Parser;
        }


        #region BUILD
        [Fact]
        public void TestBuildParser()
        {
            
            Parser<WhileToken, WhileAST> parser = buildParser();            
        }

        #endregion

        [Fact]
        public void TestAssignAdd()
        {

            Parser<WhileToken, WhileAST> parser = buildParser();
            ParseResult<WhileToken,WhileAST> result = parser.Parse("(a:=1+1)");
            Assert.False(result.IsError);
            Assert.NotNull(result.Result);            

            Assert.IsAssignableFrom(typeof(SequenceStatement), result.Result);
            SequenceStatement seq = result.Result as SequenceStatement;
            Assert.IsAssignableFrom(typeof(AssignStatement), seq.Get(0));
            AssignStatement assign = seq.Get(0) as AssignStatement;
            Assert.Equal("a", assign.VariableName);
            Expression val = assign.Value;
            Assert.IsAssignableFrom(typeof(BinaryOperation), val);
            BinaryOperation bin = val as BinaryOperation;
            Assert.Equal(BinaryOperator.ADD, bin.Operator);
            Assert.Equal(1, (bin.Left as IntegerConstant)?.Value);
            Assert.Equal(1, (bin.Right as IntegerConstant)?.Value);
        }


        [Fact]
        public void TestSkip()
        {

            Parser<WhileToken, WhileAST> parser = buildParser();
            ParseResult<WhileToken, WhileAST> result = parser.Parse("skip");
            Assert.False(result.IsError);
            Assert.NotNull(result.Result);

            Assert.IsAssignableFrom(typeof(SequenceStatement), result.Result);
            SequenceStatement seq = result.Result as SequenceStatement;
            Assert.IsAssignableFrom(typeof(SkipStatement), seq.Get(0));            
        }

        [Fact]
        public void TestPrintBoolExpression()
        {

            Parser<WhileToken, WhileAST> parser = buildParser();
            ParseResult<WhileToken, WhileAST> result = parser.Parse("print true and false");
            Assert.False(result.IsError);
            Assert.NotNull(result.Result);

            Assert.IsAssignableFrom(typeof(SequenceStatement), result.Result);
            SequenceStatement seq = result.Result as SequenceStatement;
            Assert.IsAssignableFrom(typeof(PrintStatement), seq.Get(0));
            PrintStatement print = seq.Get(0) as PrintStatement;
            Expression expr = print.Value;
            Assert.IsAssignableFrom(typeof(BinaryOperation), expr);
            BinaryOperation bin = expr as BinaryOperation;
            Assert.Equal(BinaryOperator.AND, bin.Operator);
            Assert.Equal(true, (bin.Left as BoolConstant)?.Value);
            Assert.Equal(false, (bin.Right as BoolConstant)?.Value);
        }

        [Fact]
        public void TestInfiniteWhile()
        {

            Parser<WhileToken, WhileAST> parser = buildParser();
            ParseResult<WhileToken, WhileAST> result = parser.Parse("while true do (skip)");
            Assert.False(result.IsError);
            Assert.NotNull(result.Result);

            Assert.IsAssignableFrom(typeof(SequenceStatement), result.Result);
            SequenceStatement seq = result.Result as SequenceStatement;
            Assert.IsAssignableFrom(typeof(WhileStatement), seq.Get(0));
            WhileStatement whil = seq.Get(0) as WhileStatement;
            Expression cond = whil.Condition;
            Assert.IsAssignableFrom(typeof(BoolConstant), cond);
            Assert.Equal(true, (cond as BoolConstant).Value);
            Statement s = whil.BlockStmt;
            Assert.IsAssignableFrom(typeof(SequenceStatement), whil.BlockStmt);
            SequenceStatement seqBlock = whil.BlockStmt as SequenceStatement;
            Assert.Equal(1, seqBlock.Count);
            Assert.IsAssignableFrom(typeof(SkipStatement), seqBlock.Get(0));
            ;
        }

        [Fact]
        public void TestIfThenElse()
        {

            Parser<WhileToken, WhileAST> parser = buildParser();
            ParseResult<WhileToken, WhileAST> result = parser.Parse("if true then (a := \"hello\") else (b := \"world\")");
            Assert.False(result.IsError);
            Assert.NotNull(result.Result);

            Assert.IsAssignableFrom(typeof(SequenceStatement), result.Result);
            SequenceStatement seq = result.Result as SequenceStatement;
            Assert.IsAssignableFrom(typeof(IfStatement), seq.Get(0));
            IfStatement si = seq.Get(0) as IfStatement;
            Expression cond = si.Condition;
            Assert.IsAssignableFrom(typeof(BoolConstant), cond);
            Assert.Equal(true, (cond as BoolConstant).Value);
            Statement s = si.ThenStmt;

            Assert.IsAssignableFrom(typeof(SequenceStatement), si.ThenStmt);
            SequenceStatement thenBlock = si.ThenStmt as SequenceStatement;
            Assert.Equal(1, thenBlock.Count);
            Assert.IsAssignableFrom(typeof(AssignStatement), thenBlock.Get(0));
            AssignStatement thenAssign = thenBlock.Get(0) as AssignStatement;
            Assert.Equal("a", thenAssign.VariableName);
            Assert.IsAssignableFrom(typeof(StringConstant), thenAssign.Value);
            Assert.Equal("hello", (thenAssign.Value as StringConstant).Value);
            ;

            Assert.IsAssignableFrom(typeof(SequenceStatement), si.ElseStmt);
            SequenceStatement elseBlock = si.ElseStmt as SequenceStatement;
            Assert.Equal(1, elseBlock.Count);
            Assert.IsAssignableFrom(typeof(AssignStatement), elseBlock.Get(0));
            AssignStatement elseAssign = elseBlock.Get(0) as AssignStatement;
            Assert.Equal("b", elseAssign.VariableName);
            Assert.IsAssignableFrom(typeof(StringConstant), elseAssign.Value);
            Assert.Equal("world", (elseAssign.Value as StringConstant).Value);
        }
    }
}

