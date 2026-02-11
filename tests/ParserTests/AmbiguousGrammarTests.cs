using System;
using System.Collections.Generic;
using System.Linq;
using NFluent;
using sly.buildresult;
using sly.lexer;
using sly.parser;
using sly.parser.generator;
using sly.parser.parser;
using sly.parser.syntax.tree;
using Xunit;

namespace ParserTests
{
    // Grammaire ambiguë de référence : S ::= 'a' S 'a' | 'a' 'a' S | 'a'
    public enum AmbiguousToken
    {
        [Lexeme("a")] A,
        EOF
    }

    public class AmbiguousGrammarParser
    {
        // S ::= 'a' S 'a'
        [Production("S : A S A")]
        public string Rule1(Token<AmbiguousToken> a1, string s, Token<AmbiguousToken> a2)
        {
            return $"aSa({s})";
        }

        // S ::= 'a' 'a' S
        [Production("S : A A S")]
        public string Rule2(Token<AmbiguousToken> a1, Token<AmbiguousToken> a2, string s)
        {
            return $"aaS({s})";
        }

        // S ::= 'a'
        [Production("S : A")]
        public string Rule3(Token<AmbiguousToken> a)
        {
            return "a";
        }
    }

    public class OptionAmbiguousParser
    {

        [Production("S : [a|b]")]
        public string RootOption(string option)
        {
            return $"S({option})";
        }


        [Production("a : A")]
        public string RuleA(Token<AmbiguousToken> a)
        {
            return "a(A)";
        }   
        
        [Production("b : A")]
        public string RuleB(Token<AmbiguousToken> a)
        {
            return "b(A)";
        }
    }

    public class AmbiguousGrammarTests
    {
        private BuildResult<Parser<AmbiguousToken, string>> BuildParser(bool captureAmbiguities = false,
            AmbiguityResolutionStrategy strategy = AmbiguityResolutionStrategy.First)
        {
            var parserInstance = new AmbiguousGrammarParser();
            var builder = new ParserBuilder<AmbiguousToken, string>();
            var buildResult = builder.BuildParser(parserInstance, ParserType.LL_RECURSIVE_DESCENT, "S");
            
            if (buildResult.IsOk && captureAmbiguities)
            {
                buildResult.Result.Configuration.CaptureAmbiguities = true;
                buildResult.Result.Configuration.AmbiguityStrategy = strategy;
            }
            
            return buildResult;
        }

        [Fact]
        public void TestParserBuilds()
        {
            var buildResult = BuildParser();
            Check.That(buildResult).IsOk();
            Check.That(buildResult.Result).IsNotNull();
        }

        [Fact]
        public void TestNonAmbiguousInput_SingleA()
        {
            var buildResult = BuildParser();
            Check.That(buildResult).IsOk();
            
            var parser = buildResult.Result;
            var parseResult = parser.Parse("a");
            
            Check.That(parseResult).IsOkParsing();
            Check.That(parseResult.Result).IsEqualTo("a");
            Check.That(parseResult.IsAmbiguous).IsFalse();
        }

        [Fact]
        public void TestAmbiguousInput_AAA_WithoutCapture()
        {
            // Sans capture d'ambiguïté, le parseur retourne la première dérivation
            var buildResult = BuildParser(captureAmbiguities: false);
            Check.That(buildResult).IsOk();
            
            var parser = buildResult.Result;
            var parseResult = parser.Parse("aaa");
            
            Check.That(parseResult.IsOk).IsTrue();
            Check.That(parseResult.IsAmbiguous).IsFalse();
            // Première dérivation : aSa(a) ou aaS(a) selon l'ordre des règles
        }

        [Fact]
        public void TestAmbiguousInput_AAA_WithCapture()
        {
            // "aaa" peut être parsé de 2 façons :
            // 1. S -> a S a -> a (a) a  [Rule1 avec S=a]
            // 2. S -> a a S -> a a (a)  [Rule2 avec S=a]
            var buildResult = BuildParser(captureAmbiguities: true);
            Check.That(buildResult).IsOk();
            
            var parser = buildResult.Result;
            var parseResult = parser.Parse("aaa");
            
            Check.That(parseResult.IsOk).IsTrue();
            Check.That(parseResult.IsAmbiguous).IsTrue();
            Check.That(parseResult.Forest).IsNotNull();
            Check.That(parseResult.Forest.Count).IsEqualTo(2);
            Check.That(parseResult.Forest.Ambiguities).IsNotNull();
            Check.That(parseResult.Forest.Ambiguities).Not.IsEmpty();
        }

        [Fact]
        public void TestAmbiguousInput_AAAAA_WithCapture()
        {
            // "aaaaa" est encore plus ambigu - plusieurs dérivations possibles
            var buildResult = BuildParser(captureAmbiguities: true);
            Check.That(buildResult).IsOk();
            
            var parser = buildResult.Result;
            var parseResult = parser.Parse("aaaaa");
            
            Check.That(parseResult.IsOk).IsTrue();
            Check.That(parseResult.IsAmbiguous).IsTrue();
            Check.That(parseResult.Forest.Count).IsEqualTo(4);
            var visited = parseResult.VisitAllTrees(parser.Visitor);
            Check.That(visited).IsNotNull();
        }

        [Fact]
        public void TestAmbiguityException_Strategy()
        {
            // Avec la stratégie ThrowException, une exception doit être levée
            var buildResult = BuildParser(captureAmbiguities: true, 
                strategy: AmbiguityResolutionStrategy.ThrowException);
            Check.That(buildResult).IsOk();
            
            var parser = buildResult.Result;
            
            Check.ThatCode(() => parser.Parse("aaa"))
                .Throws<sly.parser.exceptions.AmbiguousGrammarException<AmbiguousToken, string>>();
        }

        [Fact]
        public void TestAmbiguityException_ContainsForest()
        {
            var buildResult = BuildParser(captureAmbiguities: true, 
                strategy: AmbiguityResolutionStrategy.ThrowException);
            Check.That(buildResult).IsOk();
            
            var parser = buildResult.Result;
            
            try
            {
                var parseResult = parser.Parse("aaa");
                Check.That(parseResult).IsOkParsing();
            }
            catch (sly.parser.exceptions.AmbiguousGrammarException<AmbiguousToken, string> ex)
            {
                Check.That(ex.Forest).IsNotNull();
                Check.That(ex.Forest.Count).IsEqualTo(2);
                Check.That(ex.Message).Contains("2 alternative parse trees");
            }
        }

        [Fact]
        public void TestVisitAllTrees()
        {
            var buildResult = BuildParser(captureAmbiguities: true, 
                strategy: AmbiguityResolutionStrategy.All);
            Check.That(buildResult).IsOk();
            
            var parser = buildResult.Result;
            var parseResult = parser.Parse("aaa");
            
            Check.That(parseResult.IsOk).IsTrue();
            Check.That(parseResult.IsAmbiguous).IsTrue();
            
            // Visiter tous les arbres
            var allResults = parseResult.VisitAllTrees(parser.Visitor);
            
            Check.That(allResults).IsNotNull();
            Check.That(allResults).CountIs(2);
            Check.That(allResults).ContainsExactly("aSa(a)", "aaS(a)");
        }

        [Fact]
        public void TestSelectSpecificTree()
        {
            var buildResult = BuildParser(captureAmbiguities: true, 
                strategy: AmbiguityResolutionStrategy.All);
            Check.That(buildResult).IsOk();
            
            var parser = buildResult.Result;
            var parseResult = parser.Parse("aaa");
            
            Check.That(parseResult.IsOk).IsTrue();
            Check.That(parseResult.IsAmbiguous).IsTrue();
            
            // Sélectionner le premier arbre
            var result0 = parseResult.SelectTree(0, parser.Visitor);
            Check.That(result0).IsNotNull();
            
            // Sélectionner le deuxième arbre
            var result1 = parseResult.SelectTree(1, parser.Visitor);
            Check.That(result1).IsNotNull();
            
            // Les deux résultats doivent être différents
            Check.That(result0).Not.IsEqualTo(result1);
        }

        [Fact]
        public void TestResolveAmbiguity_CustomSelector()
        {
            var buildResult = BuildParser(captureAmbiguities: true, 
                strategy: AmbiguityResolutionStrategy.All);
            Check.That(buildResult).IsOk();
            
            var parser = buildResult.Result;
            var parseResult = parser.Parse("aaa");
            
            Check.That(parseResult.IsOk).IsTrue();
            Check.That(parseResult.IsAmbiguous).IsTrue();
            
            // Résoudre en choisissant le dernier arbre
            var result = parseResult.ResolveAmbiguity(
                trees => trees.Last(),
                parser.Visitor
            );
            
            Check.That(result).IsNotNull();
            Check.That(result).IsEqualTo("aaS(a)");
        }

        [Fact]
        public void TestAmbiguityInfo()
        {
            var buildResult = BuildParser(captureAmbiguities: true);
            Check.That(buildResult).IsOk();
            
            var parser = buildResult.Result;
            var parseResult = parser.Parse("aaa");
            
            Check.That(parseResult.IsOk).IsTrue();
            Check.That(parseResult.Forest.Ambiguities).IsNotNull();
            Check.That(parseResult.Forest.Ambiguities).Not.IsEmpty();
            
            var ambiguity = parseResult.Forest.Ambiguities.First();
            Check.That(ambiguity.NonTerminalName).IsEqualTo("S");
            Check.That(ambiguity.AlternativeCount).IsEqualTo(2);
            Check.That(ambiguity.Position).IsEqualTo(0);
        }

        [Fact]
        public void TestBackwardCompatibility_DefaultBehavior()
        {
            // Sans activer CaptureAmbiguities, le comportement doit être identique à avant
            var buildResult = BuildParser(captureAmbiguities: false);
            Check.That(buildResult).IsOk();
            
            var parser = buildResult.Result;
            
            // Test avec entrée non ambiguë
            var result1 = parser.Parse("a");
            Check.That(result1.IsOk).IsTrue();
            Check.That(result1.IsAmbiguous).IsFalse();
            
            // Test avec entrée ambiguë (mais pas de capture)
            var result2 = parser.Parse("aaa");
            Check.That(result2.IsOk).IsTrue();
            Check.That(result2.IsAmbiguous).IsFalse(); // Pas d'ambiguïté détectée
            Check.That(result2.Result).IsNotNull(); // Un résultat est retourné
        }

        [Fact]
        public void TestEbnfChoiceAmbiguity_WithCapture()
        {
            var parserInstance = new OptionAmbiguousParser();
            var builder = new ParserBuilder<AmbiguousToken, string>();
            var buildResult = builder.BuildParser(parserInstance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "S");
            Check.That(buildResult).IsOk();

            buildResult.Result.Configuration.CaptureAmbiguities = true;
            buildResult.Result.Configuration.AmbiguityStrategy = AmbiguityResolutionStrategy.All;

            var parser = buildResult.Result;
            var parseResult = parser.Parse("a");

            Check.That(parseResult.IsOk).IsTrue();
            Check.That(parseResult.IsAmbiguous).IsTrue();
            Check.That(parseResult.Forest).IsNotNull();
            Check.That(parseResult.Forest.Count).IsEqualTo(2);

            var allResults = parseResult.VisitAllTrees(parser.Visitor);
            Check.That(allResults).CountIs(2);
            Check.That(allResults).Contains("S(a(A))", "S(b(A))");
        }
    }
}
