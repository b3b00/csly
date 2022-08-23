using csly.whileLang.compiler;
using csly.whileLang.interpreter;
using csly.whileLang.model;
using csly.whileLang.parser;
using NFluent;
using sly.buildresult;
using sly.parser;
using sly.parser.generator;
using Xunit;

namespace ParserTests.samples
{
    public class WhileTests
    {
        private static BuildResult<Parser<WhileToken, WhileAST>> Parser;


        public BuildResult<Parser<WhileToken, WhileAST>> buildParser()
        {
            if (Parser == null)
            {
                var whileParser = new WhileParser();
                var builder = new ParserBuilder<WhileToken, WhileAST>();
                Parser = builder.BuildParser(whileParser, ParserType.EBNF_LL_RECURSIVE_DESCENT, "statement");
                Check.That(Parser).IsOkParser();
            }

            return Parser;
        }


        [Fact]
        public void TestAssignAdd()
        {
            var buildResult = buildParser();
            var parser = buildResult.Result;
            var result = parser.Parse("(a:=1+1)");
            Check.That(result).IsOkParseResult();

            Check.That(result.Result).IsInstanceOf<SequenceStatement>();
            var seq = result.Result as SequenceStatement;
            Check.That(seq.Get(0)).IsInstanceOf<AssignStatement>();
            var assign = seq.Get(0) as AssignStatement;
            Check.That(assign.VariableName).IsEqualTo("a");
            var val = assign.Value;
            Check.That(val).IsInstanceOf<BinaryOperation>();
            var bin = val as BinaryOperation;
            Check.That(bin.Operator).IsEqualTo(BinaryOperator.ADD);
            Check.That((bin.Left as IntegerConstant)?.Value).IsEqualTo(1);
            Check.That((bin.Right as IntegerConstant)?.Value).IsEqualTo(1);
        }

        [Fact]
        public void TestBuildParser()
        {
            var buildResult = buildParser();
            var parser = buildResult.Result;
        }

        [Fact]
        public void TestCounterProgram()
        {
            var buildResult = buildParser();
            var parser = buildResult.Result;
            var result = parser.Parse("(a:=0; while a < 10 do (print a; a := a +1 ))");
            Check.That(result).IsOkParseResult();
        }

        [Fact]
        public void TestCounterProgramExec()
        {
            var buildResult = buildParser();
            var parser = buildResult.Result;
            var result = parser.Parse("(a:=0; while a < 10 do (print a; a := a +1 ))");
            Check.That(result).IsOkParseResult();
            var interpreter = new Interpreter();
            var context = interpreter.Interprete(result.Result, true);
            Check.That(context.variables).IsSingle();
            Check.That(context).HasVariableWithValue("a", 10);
            
        }

        [Fact]
        public void TestFactorialProgramExec()
        {
            var program = @"
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
            var parser = buildResult.Result;
            var result = parser.Parse(program);
            Check.That(result).IsOkParseResult();
            var interpreter = new Interpreter();
            var context = interpreter.Interprete(result.Result, true);
            Assert.Equal(2, context.variables.Count);
            Check.That(context).HasVariableWithValue("i", 11);
            Check.That(context).HasVariableWithValue("r", 3628800);
            
        }


        [Fact]
        public void TestFactorialProgramExecAsIL()
        {
            var program = @"
(
    r:=1;
    i:=1;
    while i < 11 do 
    ( 
    r := r * i;
    print """".r;
    print """".i;
    i := i + 1 );
return r
)";
            var compiler = new WhileCompiler();
            var func = compiler.CompileToFunction(program,true);
            Assert.NotNull(func);
            var f = func();
            Assert.Equal(3628800, f);
        }

        [Fact]
        public void TestIfThenElse()
        {
            var buildResult = buildParser();
            var parser = buildResult.Result;
            var result = parser.Parse("if true then (a := \"hello\") else (b := \"world\")");
            Check.That(result).IsOkParseResult();
            Assert.IsType<SequenceStatement>(result.Result);
            var seq = result.Result as SequenceStatement;
            Assert.IsType<IfStatement>(seq.Get(0));
            var si = seq.Get(0) as IfStatement;
            var cond = si.Condition;
            Assert.IsType<BoolConstant>(cond);
            Assert.True((cond as BoolConstant).Value);
            var s = si.ThenStmt;

            Assert.IsType<SequenceStatement>(si.ThenStmt);
            var thenBlock = si.ThenStmt as SequenceStatement;
            Assert.Equal(1, thenBlock.Count);
            Assert.IsType<AssignStatement>(thenBlock.Get(0));
            var thenAssign = thenBlock.Get(0) as AssignStatement;
            Assert.Equal("a", thenAssign.VariableName);
            Assert.IsType<StringConstant>(thenAssign.Value);
            Assert.Equal("hello", (thenAssign.Value as StringConstant).Value);

            Assert.IsType<SequenceStatement>(si.ElseStmt);
            var elseBlock = si.ElseStmt as SequenceStatement;
            Assert.Equal(1, elseBlock.Count);
            Assert.IsType<AssignStatement>(elseBlock.Get(0));
            var elseAssign = elseBlock.Get(0) as AssignStatement;
            Assert.Equal("b", elseAssign.VariableName);
            Assert.IsType<StringConstant>(elseAssign.Value);
            Assert.Equal("world", (elseAssign.Value as StringConstant).Value);
        }

        [Fact]
        public void TestInfiniteWhile()
        {
            var buildResult = buildParser();
            var parser = buildResult.Result;
            var result = parser.Parse("while true do (skip)");
            Check.That(result).IsOkParseResult();

            Assert.IsType<SequenceStatement>(result.Result);
            var seq = result.Result as SequenceStatement;
            Assert.IsType<WhileStatement>(seq.Get(0));
            var whil = seq.Get(0) as WhileStatement;
            var cond = whil.Condition;
            Assert.IsType<BoolConstant>(cond);
            Assert.True((cond as BoolConstant).Value);
            var s = whil.BlockStmt;
            Assert.IsType<SequenceStatement>(whil.BlockStmt);
            var seqBlock = whil.BlockStmt as SequenceStatement;
            Assert.Equal(1, seqBlock.Count);
            Assert.IsType<SkipStatement>(seqBlock.Get(0));
        }

        [Fact]
        public void TestPrintBoolExpression()
        {
            var buildResult = buildParser();
            var parser = buildResult.Result;
            var result = parser.Parse("print true and false");
            Check.That(result).IsOkParseResult();

            Assert.IsType<SequenceStatement>(result.Result);
            var seq = result.Result as SequenceStatement;
            Assert.IsType<PrintStatement>(seq.Get(0));
            var print = seq.Get(0) as PrintStatement;
            var expr = print.Value;
            Assert.IsType<BinaryOperation>(expr);
            var bin = expr as BinaryOperation;
            Assert.Equal(BinaryOperator.AND, bin.Operator);
            Assert.True((bin.Left as BoolConstant)?.Value);
            Assert.False((bin.Right as BoolConstant)?.Value);
        }


        [Fact]
        public void TestSkip()
        {
            var buildResult = buildParser();
            var parser = buildResult.Result;
            var result = parser.Parse("skip");
            Check.That(result).IsOkParseResult();

            Assert.IsType<SequenceStatement>(result.Result);
            var seq = result.Result as SequenceStatement;
            Assert.IsType<SkipStatement>(seq.Get(0));
        }

        [Fact]
        public void TestSkipAssignSequence()
        {
            var buildResult = buildParser();
            var parser = buildResult.Result;
            var result = parser.Parse("(a:=1; b:=2; c:=3)");
            Check.That(result).IsOkParseResult();
            Assert.IsType<SequenceStatement>(result.Result);
            var seq = result.Result as SequenceStatement;
            Assert.Equal(3, seq.Count);

            string[] names = {"a", "b", "c"};
            for (var i = 0; i < names.Length; i++)
            {
                Assert.IsType<AssignStatement>(seq.Get(i));
                var assign = seq.Get(i) as AssignStatement;
                Assert.Equal(names[i], assign.VariableName);
                Assert.Equal(i + 1, (assign.Value as IntegerConstant).Value);
            }
        }


        [Fact]
        public void TestSkipSkipSequence()
        {
            var buildResult = buildParser();
            var parser = buildResult.Result;
            var result = parser.Parse("(skip; skip; skip)");
            Check.That(result).IsOkParseResult();
            Assert.IsType<SequenceStatement>(result.Result);
            var seq = result.Result as SequenceStatement;
            Assert.Equal(3, seq.Count);
            Assert.IsType<SkipStatement>(seq.Get(0));
            Assert.IsType<SkipStatement>(seq.Get(1));
            Assert.IsType<SkipStatement>(seq.Get(2));
        }
    }
}