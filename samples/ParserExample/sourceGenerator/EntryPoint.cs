using System;
using System.IO;
using expressionparser;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;
using sly.lexer;
using sly.parser.generator.visitor;

namespace ParserExample.sourceGenerator
{
    public class EntryPoint
    {
        public static void Basic()
        {
            try
            {
                var source = "1 +(3 + 48)";
                var lexer = LexerBuilder.BuildLexer<TestLexer>();
                ;
                if (lexer.IsOk)
                {
                    ;
                    var tokens = lexer.Result.Tokenize(source);
                    if (tokens.IsOk)
                    {
                        var r = GenTestParser.Expression(tokens.Tokens, 0);
                        Console.WriteLine("parse ended " + r.IsOk);
                        if (r.IsError)
                        {
                            foreach (var error in r.Errors)
                            {
                                Console.WriteLine(error.ErrorMessage);
                            }
                        }
                        else
                        {
                            var tree = r.Root;
                            var graphviz = new GraphVizEBNFSyntaxTreeVisitor<TestLexer>();
                            var root = graphviz.VisitTree(tree);
                            string graph = graphviz.Graph.Compile();
                            File.Delete("c:\\temp\\tree.dot");
                            File.AppendAllText("c:\\temp\\tree.dot", graph);
                        }
                        ;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        
        public static void Basic2()
        {
            try
            {
                var source = "1 + 3 * 48";
                var lexer = LexerBuilder.BuildLexer<TestLexer>();
                ;
                if (lexer.IsOk)
                {
                    ;
                    var tokens = lexer.Result.Tokenize(source);
                    if (tokens.IsOk)
                    {
                        var r = GenTestParser.Expression(tokens.Tokens, 0);
                        Console.WriteLine("parse ended " + r.IsOk);
                        if (r.IsError)
                        {
                            foreach (var error in r.Errors)
                            {
                                Console.WriteLine(error.ErrorMessage);
                            }
                        }
                        else
                        {
                            var tree = r.Root;
                            var graphviz = new GraphVizEBNFSyntaxTreeVisitor<TestLexer>();
                            var root = graphviz.VisitTree(tree);
                            string graph = graphviz.Graph.Compile();
                            File.Delete("c:\\temp\\tree.dot");
                            File.AppendAllText("c:\\temp\\tree.dot", graph);
                        }
                        ;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        
    }
}