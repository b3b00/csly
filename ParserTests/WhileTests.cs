using Xunit;
using sly.parser;
using sly.parser.generator;
using csly.whileLang.parser;
using csly.whileLang.model;
using csly.whileLang.interpreter;
using sly.buildresult;
using csly.whileLang.compiler;

namespace ParserTests
{

    public class WhileTests
    {

        private static BuildResult<Parser<WhileToken, WhileAST>> Parser;


        public WhileTests()
        {
        }


        public BuildResult<Parser<WhileToken, WhileAST>> buildParser()
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

           var buildResult = buildParser();
            Assert.False(buildResult.IsError);
            var parser = buildResult.Result;
        }

        #endregion

        #region compilation

        [Fact]
        public void TestAssignAdd()
        {

            var buildResult = buildParser();
            Assert.False(buildResult.IsError);
            var parser = buildResult.Result;
            ParseResult<WhileToken, WhileAST> result = parser.Parse("(a:=1+1)");
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

            var buildResult = buildParser();
            Assert.False(buildResult.IsError);
            var parser = buildResult.Result;
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

            var buildResult = buildParser();
            Assert.False(buildResult.IsError);
            var parser = buildResult.Result;
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

            var buildResult = buildParser();
            Assert.False(buildResult.IsError);
            var parser = buildResult.Result;
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

            var buildResult = buildParser();
            Assert.False(buildResult.IsError);
            var parser = buildResult.Result;
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


        [Fact]
        public void TestSkipSkipSequence()
        {

            var buildResult = buildParser();
            Assert.False(buildResult.IsError);
            var parser = buildResult.Result;
            ParseResult<WhileToken, WhileAST> result = parser.Parse("(skip; skip; skip)");
            Assert.False(result.IsError);
            Assert.NotNull(result.Result);
            Assert.IsAssignableFrom(typeof(SequenceStatement), result.Result);
            SequenceStatement seq = result.Result as SequenceStatement;
            Assert.Equal(3, seq.Count);
            Assert.IsAssignableFrom(typeof(SkipStatement), seq.Get(0));
            Assert.IsAssignableFrom(typeof(SkipStatement), seq.Get(1));
            Assert.IsAssignableFrom(typeof(SkipStatement), seq.Get(2));
        }

        public void TestSkipAssignSequence()
        {

            var buildResult = buildParser();
            Assert.False(buildResult.IsError);
            var parser = buildResult.Result;
            ParseResult<WhileToken, WhileAST> result = parser.Parse("(a:=1; b:=2; c:=3)");
            Assert.False(result.IsError);
            Assert.NotNull(result.Result);
            Assert.IsAssignableFrom(typeof(SequenceStatement), result.Result);
            SequenceStatement seq = result.Result as SequenceStatement;
            Assert.Equal(3, seq.Count);

            string[] names = new string[] { "a", "b", "c" };
            for (int i = 0; i < names.Length; i++)
            {
                Assert.IsAssignableFrom(typeof(AssignStatement), seq.Get(i));
                AssignStatement assign = seq.Get(i) as AssignStatement;
                Assert.Equal(names[i], assign.VariableName);
                Assert.Equal(i + 1, (assign.Value as IntegerConstant).Value);
            }
        }

        [Fact]
        public void TestCounterProgram()
        {

            var buildResult = buildParser();
            Assert.False(buildResult.IsError);
            var parser = buildResult.Result;
            ParseResult<WhileToken, WhileAST> result = parser.Parse("(a:=0; while a < 10 do (print a; a := a +1 ))");
            Assert.False(result.IsError);
            Assert.NotNull(result.Result);
            ;
        }

        #endregion

        #region interprete

        [Fact]
        public void TestCounterProgramExec()
        {

            var buildResult = buildParser();
            Assert.False(buildResult.IsError);
            var parser = buildResult.Result;
            ParseResult<WhileToken, WhileAST> result = parser.Parse("(a:=0; while a < 10 do (print a; a := a +1 ))");
            Assert.False(result.IsError);
            Assert.NotNull(result.Result);
            Interpreter interpreter = new Interpreter();
            var context = interpreter.Interprete(result.Result);
            Assert.Equal(1,context.variables.Count);
            Assert.True(CheckIntVariable(context, "a", 10));
            

            ;
        }

        [Fact]
        public void TestFactorialProgramExec()
        {
            string program = @"
(
    r:=1;
    i:=1;
    while i < 11 do 
    ( 
    r := r * i;
    print r;
    print i;
    i := i + 1 )
)";
            var buildResult = buildParser();
            Assert.False(buildResult.IsError);
            var parser = buildResult.Result;
            ParseResult<WhileToken, WhileAST> result = parser.Parse(program);
            Assert.False(result.IsError);
            Assert.NotNull(result.Result);
            Interpreter interpreter = new Interpreter();
            var context = interpreter.Interprete(result.Result);
            Assert.Equal(2, context.variables.Count);
            Assert.True(CheckIntVariable(context, "i", 11));
            Assert.True(CheckIntVariable(context, "r", 3628800));


            ;
        }


        [Fact]
        public void TestFactorialProgramExecAsIL()
        {
            string program = @"
(
    r:=1;
    i:=1;
    while i < 11 do 
    ( 
    r := r * i;
    print "".r;
    print "".i;
    i := i + 1 );
return r
)";
            WhileCompiler compiler = new WhileCompiler();
            var func = compiler.CompileToFunction(program);
            Assert.NotNull(func);
            int f = func();
            Assert.Equal(3628800, f);
                       ;
        }


        public bool CheckIntVariable(InterpreterContext context, string variable, int value)
        {
            bool ok = false;
            if (context.GetVariable(variable) != null)
            {
                int v = context.GetVariable(variable).IntValue;
                ok = v == value;
            }
            return ok;
        }


        #endregion
    }
}
