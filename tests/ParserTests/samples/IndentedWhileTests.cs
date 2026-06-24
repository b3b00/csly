using System.Linq;
using csly.indentedWhileLang.compiler;
using csly.indentedWhileLang.parser;
using csly.whileLang.interpreter;
using csly.whileLang.model;
using NFluent;
using sly.buildresult;
using sly.lexer;
using sly.parser;
using sly.parser.generator;
using sly.parser.generator.visitor;
using Xunit;

namespace ParserTests.samples
{

    [CollectionDefinition("while", DisableParallelization = true)]
    public class IndentedWhileTests
    {
        


        public BuildResult<Parser<IndentedWhileTokenGeneric, WhileAST>> buildParser(ParserType parserType)
        {
                var whileParser = new IndentedWhileParserGeneric();
                var builder = new ParserBuilder<IndentedWhileTokenGeneric, WhileAST>();
                var parser = builder.BuildParser(whileParser, parserType, "program");
                Check.That(parser).IsOk();
                return parser;
        }


        [Theory]
        [InlineData(ParserType.EBNF_LL_RECURSIVE_DESCENT)]
        [InlineData(ParserType.EBNF_LL_STACK)]
        public void TestAssignAdd(ParserType parserType)
        {
            var buildResult = buildParser(parserType);
            var parser = buildResult.Result;
            var result = parser.Parse("a:=1+1");
            Check.That(result).IsOkParsing();

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

        [Theory]
        [InlineData(ParserType.EBNF_LL_RECURSIVE_DESCENT)]
        [InlineData(ParserType.EBNF_LL_STACK)]
        public void TestBuildParser(ParserType parserType)
        {
            var buildResult = buildParser(parserType);            
            var parser = buildResult.Result;
        }

                [Theory]
        [InlineData(ParserType.EBNF_LL_RECURSIVE_DESCENT)]
        [InlineData(ParserType.EBNF_LL_STACK)]
        public void TestCounterProgram(ParserType parserType)
        {
            var buildResult = buildParser(parserType);
            var parser = buildResult.Result;
            string program = @"
a:=0 
while a < 10 do 
    print a
    a := a +1
";
            var result = parser.Parse(program);
            Check.That(result).IsOkParsing();
        }

                [Theory]
        [InlineData(ParserType.EBNF_LL_RECURSIVE_DESCENT)]
        [InlineData(ParserType.EBNF_LL_STACK)]
        public void TestCounterProgramExec(ParserType parserType)
        {
            var buildResult = buildParser(parserType);
            var parser = buildResult.Result;
            string program = @"
a:=0 
while a < 10 do 
    print a
    a := a +1
";
            var result = parser.Parse(program);
            Check.That(result).IsOkParsing();
            var interpreter = new Interpreter();
            var context = interpreter.Interprete(result.Result, true);
            Check.That(context.variables).IsSingle();
            Check.That(context).HasVariableWithIntValue("a", 10);
            
        }

                [Theory]
        [InlineData(ParserType.EBNF_LL_RECURSIVE_DESCENT)]
        [InlineData(ParserType.EBNF_LL_STACK)]
        public void TestFactorialProgramExec(ParserType parserType)
        {
            var program = @"
# TestFactorialProgramExec
r:=1
i:=1
while i < 11 do 
    r := r * i
    print r
    print i
    i := i + 1 
";
            var buildResult = buildParser(parserType);
            var parser = buildResult.Result;
            var result = parser.Parse(program);
            Check.That(result).IsOkParsing();
            var interpreter = new Interpreter();
            var context = interpreter.Interprete(result.Result, true);
            Check.That(context.variables).CountIs(2);
            Check.That(context).HasVariableWithIntValue("i", 11);
            Check.That(context).HasVariableWithIntValue("r", 3628800);
        }


                [Theory]
        [InlineData(ParserType.EBNF_LL_RECURSIVE_DESCENT)]
        [InlineData(ParserType.EBNF_LL_STACK)]
        public void TestFactorialProgramExecAsIL(ParserType parserType)
        {
            var program = @"
# TestFactorialProgramExec
r:=1
i:=1
while i < 11 do 
    r := r * i
    print $""r="".r
    print $""i="".i
    i := i + 1
return r";
            var compiler = new IndentedWhileCompiler();
            var func = compiler.CompileToFunction(program,true);
            Check.That(func).IsNotNull();
            var f = func();
            Check.That(f).IsEqualTo(3628800);
        }

                [Theory]
        [InlineData(ParserType.EBNF_LL_RECURSIVE_DESCENT)]
        [InlineData(ParserType.EBNF_LL_STACK)]
        public void TestIfThenElse(ParserType parserType)
        {
            var buildResult = buildParser(parserType);
            var parser = buildResult.Result;
            var program = @"
# TestIfThenElse
if true then
    a := $""hello""
else
    b := $""world""
";
            var result = parser.Parse(program);
            Check.That(result).IsOkParsing();

            Check.That(result.Result).IsInstanceOf<SequenceStatement>();
            var seq = result.Result as SequenceStatement;
            Check.That(seq.Get(0)).IsInstanceOf<IfStatement>();
            var si = seq.Get(0) as IfStatement;
            var cond = si.Condition;
            Check.That(cond).IsInstanceOf<BoolConstant>();
            Check.That((cond as BoolConstant).Value).IsTrue();
            var s = si.ThenStmt;

            Check.That(si.ThenStmt).IsInstanceOf<SequenceStatement>();
            var thenBlock = si.ThenStmt as SequenceStatement;
            Check.That(thenBlock).CountIs(1);
            Check.That(thenBlock.Get(0)).IsInstanceOf<AssignStatement>();
            var thenAssign = thenBlock.Get(0) as AssignStatement;
            Check.That(thenAssign.VariableName).IsEqualTo("a");
            Check.That(thenAssign.Value).IsInstanceOf<StringConstant>();
            var fstring = thenAssign.Value as StringConstant;
            Check.That(fstring).IsNotNull();
            Check.That(fstring.Value).IsEqualTo("hello");

            Check.That(si.ElseStmt).IsInstanceOf<SequenceStatement>();
            var elseBlock = si.ElseStmt as SequenceStatement;
            Check.That(elseBlock).CountIs(1);
            Check.That(elseBlock.Get(0)).IsInstanceOf<AssignStatement>();
            var elseAssign = elseBlock.Get(0) as AssignStatement;
            Check.That(elseAssign.VariableName).IsEqualTo("b");
            Check.That(elseAssign.Value).IsInstanceOf<StringConstant>();
            fstring = elseAssign.Value as StringConstant;
            Check.That(fstring).IsNotNull();
            Check.That(fstring.Value).IsEqualTo("world");
        }

                [Theory]
        [InlineData(ParserType.EBNF_LL_RECURSIVE_DESCENT)]
        [InlineData(ParserType.EBNF_LL_STACK)]
        public void TestNestedIfThenElse(ParserType parserType)
        {
            var program = @"
# TestIfThenElse
a := -111
if true then
    if true then
        a := 1
    else
        a := 2
else
    a := 3
    b := $""world""
return a
";
            var compiler = new IndentedWhileCompiler();
            var func = compiler.CompileToFunction(program,true);
            Check.That(func).IsNotNull();
            var f = func();
            Check.That(f).IsEqualTo(1);
        }


                [Theory]
        [InlineData(ParserType.EBNF_LL_RECURSIVE_DESCENT)]
        [InlineData(ParserType.EBNF_LL_STACK)]
        public void TestFString(ParserType parserType)
        {
            var buildResult = buildParser(parserType);
            var parser = buildResult.Result;
            var program = @"
# fstring
v1 := 48
v2 := 152
b := true
fstring := $""v1 :> {v1} < v2 :> {v2} < v3 :> {v1+v2} <  v4 :>{$""hello,"".$"" world""}< v5 :>{(? b -> $""true"" | $""false"")}< - end""
print fstring
return 100
";
            
            var result = parser.Parse(program);
            Check.That(result).IsOkParsing();
            Check.That(result.Result).IsNotNull();
            Check.That(result.Result).IsInstanceOf<SequenceStatement>();
            SequenceStatement seq = result.Result as SequenceStatement;
            Check.That(seq.Count).IsEqualTo(6);
            var fstringAssign = seq.Get(3) as AssignStatement;
            Check.That(fstringAssign).IsNotNull();
            Check.That(fstringAssign.VariableName).IsEqualTo("fstring");
            Check.That(fstringAssign.Value).IsInstanceOf<BinaryOperation>();
            var fString = fstringAssign.Value as BinaryOperation;
            Check.That(fString.Operator).IsEqualTo(BinaryOperator.CONCAT);
            var interpreter = new Interpreter();
            var context = interpreter.Interprete(result.Result, true);
            Check.That(context.variables).CountIs(4);
            Check.That(context).HasVariableWithIntValue("v1", 48);
            Check.That(context).HasVariableWithIntValue("v2", 152);
            Check.That(context).HasVariableWithBoolValue("b", true);
            string expected = "v1 :> 48 < v2 :> 152 < v3 :> 200 <  v4 :>hello, world< v5 :>true< - end";
            Check.That(context).HasVariableWithStringValue("fstring", expected);
            
            var compiler = new IndentedWhileCompiler();
            var func = compiler.CompileToFunction(program,true);
            Check.That(func).IsNotNull();
            Printer.Clear();
            var f = func();
            Check.That(Printer.lines).CountIs(1);
            Check.That(Printer.lines[0]).IsEqualTo(expected);
            
        }

                [Theory]
        [InlineData(ParserType.EBNF_LL_RECURSIVE_DESCENT)]
        [InlineData(ParserType.EBNF_LL_STACK)]
        public void TestInfiniteWhile(ParserType parserType)
        {
            var buildResult = buildParser(parserType);
            var parser = buildResult.Result;
            var program = @"
# infinite loop
while true do
    skip
";
            var result = parser.Parse(program);
            Check.That(result).IsOkParsing();

            Check.That(result.Result).IsInstanceOf<SequenceStatement>();
            var seq = result.Result as SequenceStatement;
            Check.That(seq.Get(0)).IsInstanceOf<WhileStatement>();
            var whil = seq.Get(0) as WhileStatement;
            var cond = whil.Condition;
            Check.That(cond).IsInstanceOf<BoolConstant>();
            Check.That((cond as BoolConstant).Value).IsTrue();
            var s = whil.BlockStmt;
            Check.That(whil.BlockStmt).IsInstanceOf<SequenceStatement>();
            var seqBlock = whil.BlockStmt as SequenceStatement;
            Check.That(seqBlock).CountIs(1);
            Check.That(seqBlock.Get(0)).IsInstanceOf<SkipStatement>();
        }

                [Theory]
        [InlineData(ParserType.EBNF_LL_RECURSIVE_DESCENT)]
        [InlineData(ParserType.EBNF_LL_STACK)]
        public void TestPrintBoolExpression(ParserType parserType)
        {
            var buildResult = buildParser(parserType);
            var parser = buildResult.Result;
            var result = parser.Parse("print true and false");
            Check.That(result).IsOkParsing();
            

            Check.That(result.Result).IsInstanceOf<SequenceStatement>();
            var seq = result.Result as SequenceStatement;
            Check.That(seq.Get(0)).IsInstanceOf<PrintStatement>();
            var print = seq.Get(0) as PrintStatement;
            var expr = print.Value;
            Check.That(expr).IsInstanceOf<BinaryOperation>();
            var bin = expr as BinaryOperation;
            Check.That(bin.Operator).IsEqualTo(BinaryOperator.AND);
            Check.That((bin.Left as BoolConstant)?.Value).IsEqualTo(true);
            Check.That((bin.Right as BoolConstant)?.Value).IsEqualTo(false);
        }


                [Theory]
        [InlineData(ParserType.EBNF_LL_RECURSIVE_DESCENT)]
        [InlineData(ParserType.EBNF_LL_STACK)]
        public void TestSkip(ParserType parserType)
        {
            var buildResult = buildParser(parserType);
            var parser = buildResult.Result;
            var result = parser.Parse("skip");
            Check.That(result).IsOkParsing();

            Check.That(result.Result).IsInstanceOf<SequenceStatement>();
            var seq = result.Result as SequenceStatement;
            Check.That(seq.Get(0)).IsInstanceOf<SkipStatement>();
        }

                [Theory]
        [InlineData(ParserType.EBNF_LL_RECURSIVE_DESCENT)]
        [InlineData(ParserType.EBNF_LL_STACK)]
        public void TestSkipAssignSequence(ParserType parserType)
        {
            var buildResult = buildParser(parserType);
            var parser = buildResult.Result;
            var program = @"a:=1
b:=2
c:=3";
            var result = parser.Parse(program);
            Check.That(result).IsOkParsing();
            Check.That(result.Result).IsInstanceOf<SequenceStatement>();
            var seq = result.Result as SequenceStatement;
            Check.That(seq).CountIs(3);



            (string name, int value)[] values = new []{ ("a",1), ("b",2), ("c",3) };

            Check.That(seq.Cast<AssignStatement>().Extracting(x => (x.VariableName,(x.Value as IntegerConstant).Value))).ContainsExactly(values);
            
        }


                [Theory]
        [InlineData(ParserType.EBNF_LL_RECURSIVE_DESCENT)]
        [InlineData(ParserType.EBNF_LL_STACK)]
        public void TestSkipSkipSkipSequence(ParserType parserType)
        {
            var buildResult = buildParser(parserType);
            var parser = buildResult.Result;
            var result = parser.Parse(@"
skip
skip
skip");
            Check.That(result).IsOkParsing();
            Check.That(result.Result).IsInstanceOf<SequenceStatement>();
            var seq = result.Result as SequenceStatement;
            Check.That(seq).CountIs(3);
            Check.That(seq.Get(0)).IsInstanceOf<SkipStatement>();
            Check.That(seq.Get(1)).IsInstanceOf<SkipStatement>();
            Check.That(seq.Get(2)).IsInstanceOf<SkipStatement>();
        }


                [Theory]
        [InlineData(ParserType.EBNF_LL_RECURSIVE_DESCENT)]
        [InlineData(ParserType.EBNF_LL_STACK)]
        public void TestIndentationError(ParserType parserType)
        {
            var buildResult = buildParser(parserType);
            var parser = buildResult.Result;
            var result = parser.Parse(@"
# infinite loop
while true do
    skip
  skip");
            Check.That(result.IsError).IsTrue();
            Check.That(result.Errors).IsSingle();
            var error = result.Errors.First();
            Check.That(error.ErrorType).IsEqualTo(ErrorType.IndentationError);
            Check.That(error.Line).IsEqualTo(4);
            Check.That(error.ErrorMessage).Contains("Indentation error");
            
            result = parser.Parse(@"
# infinite loop
while true do
    skip
skip");
            Check.That(result.IsOk).IsTrue();
           // TODO : more tests?
        }
        
                [Theory]
        [InlineData(ParserType.EBNF_LL_RECURSIVE_DESCENT)]
        [InlineData(ParserType.EBNF_LL_STACK)]
        public void TestEmptyLines(ParserType parserType)
        {
            var buildResult = buildParser(parserType);
            var parser = buildResult.Result;
            var program = @"
# infinite loop
i:= 2

while true do

    skip
";
            var result = parser.Parse(program);
            Check.That(result).IsOkParsing();
        }
        
                [Theory]
        [InlineData(ParserType.EBNF_LL_RECURSIVE_DESCENT)]
        [InlineData(ParserType.EBNF_LL_STACK)]
        public void TestMissingUIndents(ParserType parserType)
        {
            var buildResult = buildParser(parserType);
            var parser = buildResult.Result;
            var program = @"
if true then
        if false then
            x := 28";
            var result = parser.Parse(program);
            var root = result.Result;
            Check.That(root).IsInstanceOf<SequenceStatement>();
            var seq = result.Result as SequenceStatement;
            var dump = seq.Dump("  ");
            Check.That(seq.Statements).CountIs(1);
            Check.That(seq.Statements[0]).IsInstanceOf<IfStatement>();
            Check.That(result).IsOkParsing();
        }
        
                [Theory]
        [InlineData(ParserType.EBNF_LL_RECURSIVE_DESCENT)]
        [InlineData(ParserType.EBNF_LL_STACK)]
        public void TestIssue413_incompleteProgram(ParserType parserType)
        {
            var buildResult = buildParser(parserType);
            var parser = buildResult.Result;
            var program = @"
if 
";
            var result = parser.Parse(program);
            Check.That(result).Not.IsOkParsing();
            Check.That(result.Errors).CountIs(1);
            var error = result.Errors.First();
            Check.That(error.ErrorType).IsEqualTo(ErrorType.UnexpectedEOS);
            var unexpectedEosError = error as UnexpectedTokenSyntaxError<IndentedWhileTokenGeneric>;
            Check.That(unexpectedEosError).IsNotNull();
            Check.That(unexpectedEosError.ExpectedTokens.Extracting(x => x.TokenId)).IsEquivalentTo(
                IndentedWhileTokenGeneric.MINUS,IndentedWhileTokenGeneric.NOT, IndentedWhileTokenGeneric.QUESTION,
                IndentedWhileTokenGeneric.OPEN_PAREN,IndentedWhileTokenGeneric.OPEN_FSTRING,IndentedWhileTokenGeneric.TRUE,
                IndentedWhileTokenGeneric.INT,IndentedWhileTokenGeneric.FALSE,IndentedWhileTokenGeneric.IDENTIFIER);
        }
        
                [Theory]
        [InlineData(ParserType.EBNF_LL_RECURSIVE_DESCENT)]
        [InlineData(ParserType.EBNF_LL_STACK)]
        public void TestIndentationError_emptyIndentLine(ParserType parserType)
        {
            BuildResult<ILexer<IndentedWhileTokenGeneric>> _lexer = LexerBuilder.BuildLexer<IndentedWhileTokenGeneric>();

            Check.That(_lexer).IsOk();
            
            var program = @"
if true then

        if false then
            x := 28";
            var lexed = _lexer.Result.Tokenize(program);
            Check.That(lexed).IsOkLexing();
            var mainTokens = lexed.Tokens.MainTokens();
            Check.That(mainTokens).Not.IsEmpty();
            Check.That(mainTokens.Last().IsEOS).IsTrue();
            var lastToken = mainTokens[mainTokens.Count - 2];
            Check.That(lastToken).IsNotNull();
            Check.That(lastToken.TokenID)
                .IsEqualTo(IndentedWhileTokenGeneric.INT);
            Check.That(lastToken.IntValue).IsEqualTo(28);
        }
    }
}