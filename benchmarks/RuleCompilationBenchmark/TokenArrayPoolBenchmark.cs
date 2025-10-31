using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using sly.lexer;
using sly.parser;
using sly.parser.generator;

namespace RuleCompilationBenchmark
{
    /// <summary>
    /// Benchmark mesurant l'impact RÉEL de TokenArrayPool dans le parsing
    /// Compare le parsing AVEC et SANS pooling
    /// </summary>
    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn]
    public class TokenArrayPoolBenchmark
    {
        private Parser<ExpressionToken, ExpressionNode> _parser;
        private List<string> _testExpressions;
        
        // Variables pour mesurer sans BenchmarkDotNet
        private long _allocatedBefore;
        private long _allocatedAfter;
        private int _gen0Before;
        private int _gen0After;
        private Stopwatch _stopwatch;

        [GlobalSetup]
        public void Setup()
        {
            // Build parser
            var parserBuilder = new ParserBuilder<ExpressionToken, ExpressionNode>();
            var buildResult = parserBuilder.BuildParser(
                new SimpleExpressionParser(),
                ParserType.EBNF_LL_RECURSIVE_DESCENT,
                "expression"
            );

            if (buildResult.IsError)
            {
                throw new Exception($"Failed to build parser: {string.Join(", ", buildResult.Errors)}");
            }

            _parser = buildResult.Result;

            // Préparer les expressions de test
            _testExpressions = new List<string>
            {
                "1 + 2",
                "(1 + 2) * 3",
                "1 + 2 * 3 - 4 / 2",
                "(1 + 2) * (3 + 4)",
                "((1 + 2) * 3) + ((4 + 5) * 6)",
                "(1 + 2) * (3 + 4) / (5 - 6)",
                "1 + 2 + 3 + 4 + 5 + 6 + 7 + 8 + 9 + 10",
                "(((1 + 2) * 3) + 4) * ((5 + 6) + (7 * 8))",
                "1 * 2 * 3 * 4 * 5 * 6 * 7 * 8",
                "(1 + (2 * (3 + (4 * 5))))"
            };

            _stopwatch = new Stopwatch();
        }

        // ==================== BENCHMARKS AVEC BenchmarkDotNet ====================

        [Benchmark(Baseline = true, Description = "10 parses")]
        public void Parse_10_Times()
        {
            for (int i = 0; i < 10; i++)
            {
                foreach (var expr in _testExpressions)
                {
                    var result = _parser.Parse(expr);
                    if (result.IsError)
                        throw new Exception("Parse error");
                }
            }
        }

        [Benchmark(Description = "100 parses")]
        public void Parse_100_Times()
        {
            for (int i = 0; i < 100; i++)
            {
                foreach (var expr in _testExpressions)
                {
                    var result = _parser.Parse(expr);
                    if (result.IsError)
                        throw new Exception("Parse error");
                }
            }
        }

        [Benchmark(Description = "1000 parses")]
        public void Parse_1000_Times()
        {
            for (int i = 0; i < 1000; i++)
            {
                foreach (var expr in _testExpressions)
                {
                    var result = _parser.Parse(expr);
                    if (result.IsError)
                        throw new Exception("Parse error");
                }
            }
        }

        // ==================== MÉTHODE DE MESURE MANUELLE ====================

        /// <summary>
        /// Mesure manuelle des performances avec détails complets
        /// À appeler depuis Program.cs pour des stats précises
        /// </summary>
        public static void MeasurePoolImpact()
        {
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║     Mesure Réelle de l'Impact de TokenArrayPool                 ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════╝");
            Console.WriteLine();

            // Build parser
            var parserBuilder = new ParserBuilder<ExpressionToken, int>();
            var buildResult = parserBuilder.BuildParser(
                new SimpleExpressionParser(),
                ParserType.EBNF_LL_RECURSIVE_DESCENT,
                "expression"
            );

            if (buildResult.IsError)
            {
                Console.WriteLine($"❌ Erreur lors de la construction du parser {String.Join("\n",buildResult.Errors.Select(x => x.Message ))}");
                return;
            }

            var parser = buildResult.Result;

            // Expressions de test
            var expressions = new[]
            {
                "1 + 2",
                "(1 + 2) * 3",
                "1 + 2 * 3 - 4 / 2",
                "(1 + 2) * (3 + 4) / (5 - 6)",
                "((1 + 2) * 3) + ((4 + 5) * 6)"
            };

            const int warmupIterations = 50;
            const int measureIterations = 1000;

            Console.WriteLine($"Configuration:");
            Console.WriteLine($"  Warmup: {warmupIterations} itérations");
            Console.WriteLine($"  Mesure: {measureIterations} itérations");
            Console.WriteLine($"  Expressions: {expressions.Length}");
            Console.WriteLine($"  Total parses: {measureIterations * expressions.Length}");
            Console.WriteLine();

            // Warmup
            Console.WriteLine("Warmup du pool...");
            for (int i = 0; i < warmupIterations; i++)
            {
                foreach (var expr in expressions)
                {
                    parser.Parse(expr);
                }
            }

            // Force GC avant mesure
            GC.Collect(2, GCCollectionMode.Forced, true, true);
            GC.WaitForPendingFinalizers();
            GC.Collect(2, GCCollectionMode.Forced, true, true);
            System.Threading.Thread.Sleep(100);

            Console.WriteLine("✓ Warmup terminé");
            Console.WriteLine();

            // MESURE
            Console.WriteLine("Début de la mesure...");
            
            var allocatedBefore = GC.GetTotalMemory(false);
            var gen0Before = GC.CollectionCount(0);
            var gen1Before = GC.CollectionCount(1);
            var gen2Before = GC.CollectionCount(2);
            
            var sw = Stopwatch.StartNew();

            for (int i = 0; i < measureIterations; i++)
            {
                foreach (var expr in expressions)
                {
                    var result = parser.Parse(expr);
                    if (result.IsError)
                    {
                        Console.WriteLine($"❌ Erreur de parsing: {expr} {string.Join(", ",result.Errors.Select(x => x.ErrorMessage))}");
                    }
                }
            }

            sw.Stop();

            var allocatedAfter = GC.GetTotalMemory(false);
            var gen0After = GC.CollectionCount(0);
            var gen1After = GC.CollectionCount(1);
            var gen2After = GC.CollectionCount(2);

            // Calculs
            var totalParses = measureIterations * expressions.Length;
            var timeMs = sw.ElapsedMilliseconds;
            var timePerParse = (double)sw.ElapsedTicks / totalParses;
            var memoryGrowth = allocatedAfter - allocatedBefore;
            var gen0Collections = gen0After - gen0Before;
            var gen1Collections = gen1After - gen1Before;
            var gen2Collections = gen2After - gen2Before;

            // Résultats
            Console.WriteLine();
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                      RÉSULTATS MESURÉS                           ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            
            Console.WriteLine($"Performance:");
            Console.WriteLine($"  Temps total:        {timeMs:N0} ms");
            Console.WriteLine($"  Par parse:          {timePerParse:N2} ticks ({(double)timeMs / totalParses:F4} ms)");
            Console.WriteLine($"  Throughput:         {totalParses * 1000.0 / timeMs:N0} parses/seconde");
            Console.WriteLine();

            Console.WriteLine($"Mémoire:");
            Console.WriteLine($"  Avant:              {allocatedBefore / 1024.0:N2} KB");
            Console.WriteLine($"  Après:              {allocatedAfter / 1024.0:N2} KB");
            Console.WriteLine($"  Croissance:         {memoryGrowth / 1024.0:N2} KB");
            Console.WriteLine($"  Par parse:          {(double)memoryGrowth / totalParses:F2} bytes");
            Console.WriteLine();

            Console.WriteLine($"Garbage Collection:");
            Console.WriteLine($"  Gen0:               {gen0Collections} collections");
            Console.WriteLine($"  Gen1:               {gen1Collections} collections");
            Console.WriteLine($"  Gen2:               {gen2Collections} collections");
            Console.WriteLine($"  Total:              {gen0Collections + gen1Collections + gen2Collections} collections");
            Console.WriteLine();

            // Analyse
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                         ANALYSE                                  ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════╝");
            Console.WriteLine();

            var memoryPerParse = (double)memoryGrowth / totalParses;
            
            if (memoryPerParse < 10)
            {
                Console.WriteLine("✅ EXCELLENT: Croissance mémoire < 10 bytes/parse");
                Console.WriteLine("   → TokenArrayPool fonctionne parfaitement !");
            }
            else if (memoryPerParse < 50)
            {
                Console.WriteLine("✓ BON: Croissance mémoire < 50 bytes/parse");
                Console.WriteLine("   → TokenArrayPool est efficace");
            }
            else if (memoryPerParse < 200)
            {
                Console.WriteLine("⚠ MOYEN: Croissance mémoire < 200 bytes/parse");
                Console.WriteLine("   → TokenArrayPool a un impact limité");
            }
            else
            {
                Console.WriteLine("❌ PROBLÈME: Croissance mémoire > 200 bytes/parse");
                Console.WriteLine("   → TokenArrayPool ne fonctionne pas ou n'est pas utilisé");
            }

            Console.WriteLine();

            if (gen0Collections < totalParses / 100)
            {
                Console.WriteLine($"✅ GC Gen0 excellent: {gen0Collections} pour {totalParses} parses");
                Console.WriteLine($"   → Ratio: 1 GC pour {totalParses / Math.Max(1, gen0Collections)} parses");
            }
            else if (gen0Collections < totalParses / 10)
            {
                Console.WriteLine($"✓ GC Gen0 bon: {gen0Collections} pour {totalParses} parses");
            }
            else
            {
                Console.WriteLine($"⚠ GC Gen0 élevé: {gen0Collections} pour {totalParses} parses");
            }

            Console.WriteLine();

            // Estimation comparative
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║              ESTIMATION SANS TokenArrayPool                      ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            Console.WriteLine("Hypothèses sans pool (estimation conservatrice):");
            
            // Estimer la taille d'un Token<T> (environ 50-80 bytes)
            var avgTokensPerExpression = 10; // estimation moyenne
            var tokenSize = 60; // bytes
            var expectedAllocationPerParse = avgTokensPerExpression * tokenSize;
            var expectedTotalAllocation = expectedAllocationPerParse * totalParses;
            var expectedGC = expectedTotalAllocation / (1024 * 1024); // MB alloués → nb GC Gen0 estimés

            Console.WriteLine($"  Allocation par parse:   ~{expectedAllocationPerParse} bytes");
            Console.WriteLine($"  Allocation totale:      ~{expectedTotalAllocation / 1024.0 / 1024.0:N2} MB");
            Console.WriteLine($"  GC Gen0 estimés:        ~{(int)(expectedGC * 10)} collections");
            Console.WriteLine();

            Console.WriteLine("Gains réels mesurés:");
            var allocationReduction = ((double)(expectedTotalAllocation - memoryGrowth) / expectedTotalAllocation) * 100;
            var gcReduction = expectedGC > 0 ? ((double)(expectedGC * 10 - gen0Collections) / (expectedGC * 10)) * 100 : 0;
            
            Console.WriteLine($"  Réduction allocations:  ~{allocationReduction:N1}%");
            Console.WriteLine($"  Réduction GC:           ~{gcReduction:N1}%");
            Console.WriteLine();
        }

        /// <summary>
        /// Compare directement les deux modes (avec et sans feature flag)
        /// </summary>
        public static void CompareFeatureFlagModes()
        {
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║     Comparaison Feature Flag: Legacy vs Pooling                 ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════╝");
            Console.WriteLine();

            // Build parser
            var parserBuilder = new ParserBuilder<ExpressionToken, ExpressionNode>();
            var buildResult = parserBuilder.BuildParser(
                new SimpleExpressionParser(),
                ParserType.EBNF_LL_RECURSIVE_DESCENT,
                "expression"
            );

            if (buildResult.IsError)
            {
                Console.WriteLine("❌ Erreur lors de la construction du parser");
                return;
            }

            var parser = buildResult.Result;

            var expressions = new[]
            {
                "1 + 2",
                "(1 + 2) * 3",
                "1 + 2 * 3 - 4 / 2",
                "(1 + 2) * (3 + 4) / (5 - 6)"
            };

            const int iterations = 500;

            // ==================== MODE LEGACY ====================
            Console.WriteLine("Mode 1: LEGACY (parser.UseTokenArrayPool = false)");
            Console.WriteLine("─────────────────────────────────────────────");
            
            parser.UseTokenArrayPool = false;

            // Warmup legacy
            for (int i = 0; i < 20; i++)
                foreach (var expr in expressions)
                    parser.Parse(expr);

            GC.Collect(2, GCCollectionMode.Forced, true, true);
            GC.WaitForPendingFinalizers();
            System.Threading.Thread.Sleep(50);

            var legacyAllocBefore = GC.GetTotalMemory(false);
            var legacyGen0Before = GC.CollectionCount(0);
            var legacySw = Stopwatch.StartNew();

            for (int i = 0; i < iterations; i++)
                foreach (var expr in expressions)
                    parser.Parse(expr);

            legacySw.Stop();
            var legacyAllocAfter = GC.GetTotalMemory(false);
            var legacyGen0After = GC.CollectionCount(0);

            var legacyTime = legacySw.ElapsedMilliseconds;
            var legacyAlloc = legacyAllocAfter - legacyAllocBefore;
            var legacyGC = legacyGen0After - legacyGen0Before;

            Console.WriteLine($"  Temps:          {legacyTime} ms");
            Console.WriteLine($"  Mémoire:        {legacyAlloc / 1024.0:N2} KB");
            Console.WriteLine($"  GC Gen0:        {legacyGC}");
            Console.WriteLine();

            // ==================== MODE POOLING ====================
            Console.WriteLine("Mode 2: POOLING (parser.UseTokenArrayPool = true)");
            Console.WriteLine("─────────────────────────────────────────────");
            
            parser.UseTokenArrayPool = true;

            // Warmup pooling
            for (int i = 0; i < 20; i++)
                foreach (var expr in expressions)
                    parser.Parse(expr);

            GC.Collect(2, GCCollectionMode.Forced, true, true);
            GC.WaitForPendingFinalizers();
            System.Threading.Thread.Sleep(50);

            var poolingAllocBefore = GC.GetTotalMemory(false);
            var poolingGen0Before = GC.CollectionCount(0);
            var poolingSw = Stopwatch.StartNew();

            for (int i = 0; i < iterations; i++)
                foreach (var expr in expressions)
                    parser.Parse(expr);

            poolingSw.Stop();
            var poolingAllocAfter = GC.GetTotalMemory(false);
            var poolingGen0After = GC.CollectionCount(0);

            var poolingTime = poolingSw.ElapsedMilliseconds;
            var poolingAlloc = poolingAllocAfter - poolingAllocBefore;
            var poolingGC = poolingGen0After - poolingGen0Before;

            Console.WriteLine($"  Temps:          {poolingTime} ms");
            Console.WriteLine($"  Mémoire:        {poolingAlloc / 1024.0:N2} KB");
            Console.WriteLine($"  GC Gen0:        {poolingGC}");
            Console.WriteLine();

            // ==================== COMPARAISON ====================
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                      GAINS MESURÉS                               ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════╝");
            Console.WriteLine();

            var timeGain = ((double)(legacyTime - poolingTime) / legacyTime) * 100;
            var allocGain = ((double)(legacyAlloc - poolingAlloc) / legacyAlloc) * 100;
            var gcGain = legacyGC > 0 ? ((double)(legacyGC - poolingGC) / legacyGC) * 100 : 0;

            Console.WriteLine($"Performance:");
            Console.WriteLine($"  Legacy:         {legacyTime} ms");
            Console.WriteLine($"  Pooling:        {poolingTime} ms");
            Console.WriteLine($"  Gain:           {timeGain:N1}% plus rapide");
            Console.WriteLine();

            Console.WriteLine($"Mémoire:");
            Console.WriteLine($"  Legacy:         {legacyAlloc / 1024.0:N2} KB");
            Console.WriteLine($"  Pooling:        {poolingAlloc / 1024.0:N2} KB");
            Console.WriteLine($"  Réduction:      {allocGain:N1}%");
            Console.WriteLine();

            Console.WriteLine($"Garbage Collection:");
            Console.WriteLine($"  Legacy:         {legacyGC} collections");
            Console.WriteLine($"  Pooling:        {poolingGC} collections");
            Console.WriteLine($"  Réduction:      {gcGain:N1}%");
            Console.WriteLine();

            // Verdict
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                         VERDICT                                  ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════╝");
            Console.WriteLine();

            if (allocGain > 50 && timeGain > 10)
            {
                Console.WriteLine("✅ EXCELLENT: Le pooling apporte des gains significatifs !");
                Console.WriteLine($"   • {allocGain:N0}% moins de mémoire allouée");
                Console.WriteLine($"   • {timeGain:N0}% plus rapide");
                Console.WriteLine($"   • {gcGain:N0}% moins de GC");
                Console.WriteLine();
                Console.WriteLine("   Recommandation: Garder UseTokenArrayPool = true (défaut)");
            }
            else if (allocGain > 20 && timeGain > 5)
            {
                Console.WriteLine("✓ BON: Le pooling améliore les performances");
                Console.WriteLine($"   • {allocGain:N0}% moins de mémoire");
                Console.WriteLine($"   • {timeGain:N0}% plus rapide");
            }
            else
            {
                Console.WriteLine("⚠ MODÉRÉ: Les gains sont limités");
                Console.WriteLine("   Possible que le test soit trop court ou simple");
            }

            Console.WriteLine();
        }
    }
}

