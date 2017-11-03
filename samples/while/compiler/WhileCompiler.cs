using System;
using System.Text;
using csly.whileLang.model;
using csly.whileLang.parser;
using sly.parser;
using sly.parser.generator;

using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Sigil;

namespace csly.whileLang.compiler
{
    public class WhileCompiler
    {

        private Parser<WhileToken, WhileAST> whileParser;





        public WhileCompiler()
        {
            WhileParser parser = new WhileParser();
            ParserBuilder<WhileToken, WhileAST> builder = new ParserBuilder<WhileToken, WhileAST>();
            var whileParserBuildResult = builder.BuildParser(parser, ParserType.EBNF_LL_RECURSIVE_DESCENT, "statement");
            whileParser = whileParserBuildResult.Result;
        }


        private string GetNameSpace(string id)
        {
            return $"NS{id.Replace("-","")}";
        }

        private string GetClassName(string id)
        {
            return $"Class{id.Replace("-", "")}";
        }


        private string GetCSharpCode(string code, string id)
        {
            StringBuilder classCode = new StringBuilder();
            classCode.AppendLine("using System;");
            classCode.AppendLine("using csly.whileLang.compiler;");
            classCode.AppendLine($"namespace {GetNameSpace(id)} {{");
            classCode.AppendLine($"     public class {GetClassName(id)} : WhileFunction {{");
            classCode.AppendLine($"         public void Run() {{");
            classCode.AppendLine(code);
            classCode.AppendLine("         }");
            classCode.AppendLine("      }");
            classCode.AppendLine("}");
            return classCode.ToString();
        }


        public string TranspileToCSharp(string whileCode)
        {
            string sharpCode = null;

            try
            {
                ParseResult<WhileToken, WhileAST> result = whileParser.Parse(whileCode);
                if (result.IsOk)
                {

                    WhileAST ast = result.Result;

                    SemanticChecker checker = new SemanticChecker();

                    CompilerContext context = checker.SemanticCheck(ast);

                    sharpCode = ast.Transpile(context);
                    sharpCode = GetCSharpCode(sharpCode, Guid.NewGuid().ToString());
                }
            }
            catch (Exception e)
            {
                sharpCode = null;
            }


            return sharpCode;
        }


        public Func<int> CompileToFunction(string whileCode)
        {
            Func<int> function = null;

            try
            {
                ParseResult<WhileToken, WhileAST> result = whileParser.Parse(whileCode);
                if (result.IsOk)
                {

                    WhileAST ast = result.Result;

                    SemanticChecker checker = new SemanticChecker();

                    CompilerContext context = checker.SemanticCheck(ast);

                    Emit<Func<int>>  emiter = Emit<Func<int>>.NewDynamicMethod("Method"+Guid.NewGuid().ToString());

                    emiter = ast.EmitByteCode(context, emiter);
                    //emiter.LoadConstant(42);                    
                    //emiter.Return();
                    function = emiter.CreateDelegate();
                    object res = function.Invoke();
                    ;
                }
            }
            catch (Exception e)
            {
                function = null;
            }


            return function;
        }



        

    }
}


