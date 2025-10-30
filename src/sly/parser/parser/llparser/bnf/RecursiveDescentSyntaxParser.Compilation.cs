using System;
using sly.parser.parser.compilation;
using sly.parser.syntax.grammar;

namespace sly.parser.llparser.bnf
{
    /// <summary>
    /// Extension to RecursiveDescentSyntaxParser for rule compilation support
    /// </summary>
    public partial class RecursiveDescentSyntaxParser<IN, OUT> where IN : struct, Enum
    {
        // Lazy initialization of rule compiler
        private RuleCompiler<IN, OUT> _ruleCompiler;
        
        protected bool UseRuleCompilation { get; set; } = false;

        /// <summary>
        /// Enable rule compilation for performance optimization
        /// </summary>
        public void EnableRuleCompilation()
        {
            UseRuleCompilation = true;
            _ruleCompiler = new RuleCompiler<IN, OUT>();
            
            // Pre-compile frequently used rules
            PrecompileCommonRules();
        }

        /// <summary>
        /// Disable rule compilation
        /// </summary>
        public void DisableRuleCompilation()
        {
            UseRuleCompilation = false;
            _ruleCompiler?.Clear();
            _ruleCompiler = null;
        }

        private void PrecompileCommonRules()
        {
            if (_ruleCompiler == null || Configuration?.NonTerminals == null)
                return;

            // Compile all simple terminal and sequence rules
            foreach (var nonTerminal in Configuration.NonTerminals.Values)
            {
                foreach (var rule in nonTerminal.Rules)
                {
                    try
                    {
                        _ruleCompiler.Compile(rule, rule.RuleString ?? nonTerminal.Name);
                    }
                    catch
                    {
                        // If compilation fails, fall back to interpretation
                        // This is safe as we always have the interpreter as fallback
                    }
                }
            }
        }

        /// <summary>
        /// Try to use compiled rule if available, otherwise fall back to interpretation
        /// </summary>
        protected SyntaxParseResult<IN, OUT> TryUseCompiledRule(
            lexer.Token<IN>[] tokens,
            Rule<IN, OUT> rule,
            int position,
            string ruleName,
            SyntaxParsingContext<IN, OUT> parsingContext)
        {
            if (UseRuleCompilation && _ruleCompiler != null)
            {
                try
                {
                    var compiledRule = _ruleCompiler.Compile(rule, ruleName);
                    return compiledRule(tokens, position, parsingContext);
                }
                catch
                {
                    // Fall back to interpretation on any error
                }
            }

            // Return null to indicate compiled version not used
            return null;
        }

        /// <summary>
        /// Get compilation statistics
        /// </summary>
        public (int compiledRules, bool compilationEnabled) GetCompilationStats()
        {
            return (
                _ruleCompiler?.CompiledRuleCount ?? 0,
                UseRuleCompilation
            );
        }
    }
}

