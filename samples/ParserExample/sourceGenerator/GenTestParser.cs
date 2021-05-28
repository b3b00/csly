using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.Serialization;
using sly.lexer;
using sly.parser;
using sly.parser.syntax.tree;

namespace ParserExample.sourceGenerator
{
    public class GenTestParser
    {

        private static IList<T> IList<T>(params T[] items)
        {
            return items.ToList();
        }
        
        private static List<ISyntaxNode<TestLexer>> ListSyntax(params ISyntaxNode<TestLexer>[] items)
        {
            return items.ToList();
        }
        private static SyntaxParseResult<TestLexer> Terminal(IList<Token<TestLexer>> tokens, int position, TestLexer expected, bool discarded)
        {
            var currentToken = tokens[position];
            var result = new SyntaxParseResult<TestLexer>();
            var isError = !currentToken.TokenID.Equals(expected);
            if (isError)
            {
                return Error(position, currentToken, expected);
            }
            else
            {
                result.IsError = false;
                result.EndingPosition = !result.IsError ? position + 1 : position;
                var token = tokens[position];
                token.Discarded = discarded;
                result.Root = new SyntaxLeaf<TestLexer>(token, discarded);
                result.IsEnded = result.EndingPosition >= tokens.Count - 1;
                return result;
            }
        }

        private static SyntaxParseResult<TestLexer> Choice(string name, params SyntaxParseResult<TestLexer>[] results)
        {
            if (results.Any(x => x == null))
            {
                ;
            }

            var ordered = results.Where(x => x != null).OrderBy(x => x.EndingPosition);
            var oks = ordered.Where(x => x.IsOk);
            if (oks.Any())
            {
                SyntaxParseResult<TestLexer> result = null;
                ISyntaxNode<TestLexer> root = null;
                if (oks.Any(x => x.IsEnded))
                { 
                    result = oks.FirstOrDefault(x => x.IsEnded);
                }
                else
                {
                    result = oks.Last();
                }
                root = new SyntaxNode<TestLexer>(name, ListSyntax(result.Root), null);
                result = new SyntaxParseResult<TestLexer>()
                {
                    Errors = new List<UnexpectedTokenSyntaxError<TestLexer>>(),
                    IsError = false,
                    EndingPosition = result.EndingPosition,
                    IsEnded = result.IsEnded,
                    Root = root
                };
                return result;
            }

            return ordered.Last();
        }

        private static SyntaxParseResult<TestLexer> Error(int position, Token<TestLexer> unexpected, params TestLexer[] expected)
        {
            var result = new SyntaxParseResult<TestLexer>();
            result.IsError = true;
            var error = new UnexpectedTokenSyntaxError<TestLexer>(unexpected,"", expected);
            result.Errors = new List<UnexpectedTokenSyntaxError<TestLexer>>() {error};
            result.EndingPosition = position;
            return result;
        }
        
        public static SyntaxParseResult<TestLexer> PrimaryInt(IList<Token<TestLexer>> tokens, int position)
        {
            var expected = TestLexer.INT;
            bool discarded = false;

            return Terminal(tokens, position, expected, discarded);
        }
        

        public static SyntaxParseResult<TestLexer> Group(IList<Token<TestLexer>> tokens, int position)
        {
            SyntaxParseResult<TestLexer> result = null;

            SyntaxParseResult<TestLexer> r1 = null;
            SyntaxParseResult<TestLexer> r2 = null;
            SyntaxParseResult<TestLexer> r3 = null;
            
            r1 = Terminal(tokens, position, TestLexer.LPAREN, true);
            if (r1.IsOk)
            {
                
                position = r1.EndingPosition;
                r2 = Expression(tokens, position);
                if (r2.IsOk)
                {
                    position = r2.EndingPosition;
                    r3 = Terminal(tokens, position, TestLexer.RPAREN, true);
                    if (r3.IsOk)
                    {
                        var root = new SyntaxNode<TestLexer>("group",
                            new List<ISyntaxNode<TestLexer>>() {r1.Root, r2.Root, r3.Root},
                            null
                        );
                        var r = new SyntaxParseResult<TestLexer>()
                        {
                            EndingPosition = r3.EndingPosition,
                            Errors = new List<UnexpectedTokenSyntaxError<TestLexer>>(),
                            IsError = false,
                            IsEnded = r3.IsEnded,
                            Root = root                            
                        };
                        return r;
                    }
                    else
                    {
                        return r3;
                    }
                }
                else
                {
                    return r2;
                }
            }
            else
            {
                return r1;
            }

            return null;
        }

        public static SyntaxParseResult<TestLexer> Primary(IList<Token<TestLexer>> tokens, int position)
        {
            var r1 = PrimaryInt(tokens, position);

            var r2 = Group(tokens, position);

            return Choice("primary",r1, r2);
        }


        public static SyntaxParseResult<TestLexer> ExpressionTermPlusExpression(IList<Token<TestLexer>> tokens, int position)
        {
            SyntaxParseResult<TestLexer> result = null;

            SyntaxParseResult<TestLexer> r1 = null;
            SyntaxParseResult<TestLexer> r2 = null;
            SyntaxParseResult<TestLexer> r3 = null;
            
            r1 = Term(tokens, position);
            if (r1.IsOk)
            {
                
                position = r1.EndingPosition;
                r2 = Terminal(tokens, position,TestLexer.PLUS,true);
                if (r2.IsOk)
                {
                    position = r2.EndingPosition;
                    r3 = Expression(tokens, position);
                    if (r3.IsOk)
                    {
                        var root = new SyntaxNode<TestLexer>("plus",
                            new List<ISyntaxNode<TestLexer>>() {r1.Root, r2.Root, r3.Root},
                            null
                        );
                        var r = new SyntaxParseResult<TestLexer>()
                        {
                            EndingPosition = r3.EndingPosition,
                            Errors = new List<UnexpectedTokenSyntaxError<TestLexer>>(),
                            IsError = false,
                            IsEnded = r3.IsEnded,
                            Root = root                            
                        };
                        return r;
                    }
                    else
                    {
                        return r3;
                    }
                }
                else
                {
                    return r2;
                }
            }
            else
            {
                return r1;
            }

            return null;
        }
        
        public static SyntaxParseResult<TestLexer> ExpressionTermMinusExpression(IList<Token<TestLexer>> tokens, int position)
        {
            SyntaxParseResult<TestLexer> result = null;

            SyntaxParseResult<TestLexer> r1 = null;
            SyntaxParseResult<TestLexer> r2 = null;
            SyntaxParseResult<TestLexer> r3 = null;
            
            r1 = Term(tokens, position);
            if (r1.IsOk)
            {
                
                position = r1.EndingPosition;
                r2 = Terminal(tokens, position,TestLexer.MINUS,true);
                if (r2.IsOk)
                {
                    position = r2.EndingPosition;
                    r3 = Expression(tokens, position);
                    if (r3.IsOk)
                    {
                        var root = new SyntaxNode<TestLexer>("minus",
                            new List<ISyntaxNode<TestLexer>>() {r1.Root, r2.Root, r3.Root},
                            null
                        );
                        var r = new SyntaxParseResult<TestLexer>()
                        {
                            EndingPosition = r3.EndingPosition,
                            Errors = new List<UnexpectedTokenSyntaxError<TestLexer>>(),
                            IsError = false,
                            IsEnded = r3.IsEnded,
                            Root = root                            
                        };
                        return r;
                    }
                    else
                    {
                        return r3;
                    }
                }
                else
                {
                    return r2;
                }
            }
            else
            {
                return r1;
            }

            return null;
        }

        
        
        public static SyntaxParseResult<TestLexer> Expression(IList<Token<TestLexer>> tokens, int position)
        {
            var r1 = Term(tokens, position);
            
            var r2 = ExpressionTermPlusExpression(tokens, position);
            
            var r3 = ExpressionTermMinusExpression(tokens, position);
            return Choice("expression",r1, r2,r3);
        }

        public static SyntaxParseResult<TestLexer> termfactorTimesTerm(IList<Token<TestLexer>> tokens, int position)
        {
            SyntaxParseResult<TestLexer> result = null;

            SyntaxParseResult<TestLexer> r1 = null;
            SyntaxParseResult<TestLexer> r2 = null;
            SyntaxParseResult<TestLexer> r3 = null;
            
            r1 = Factor(tokens, position);
            if (r1.IsOk)
            {
                
                position = r1.EndingPosition;
                r2 = Terminal(tokens, position,TestLexer.TIMES,true);
                if (r2.IsOk)
                {
                    position = r2.EndingPosition;
                    r3 = Term(tokens, position);
                    if (r3.IsOk)
                    {
                        var root = new SyntaxNode<TestLexer>("times",
                            new List<ISyntaxNode<TestLexer>>() {r1.Root, r2.Root, r3.Root},
                            null
                        );
                        var r = new SyntaxParseResult<TestLexer>()
                        {
                            EndingPosition = r3.EndingPosition,
                            Errors = new List<UnexpectedTokenSyntaxError<TestLexer>>(),
                            IsError = false,
                            IsEnded = r3.IsEnded,
                            Root = root                            
                        };
                        return r;
                    }
                    else
                    {
                        return r3;
                    }
                }
                else
                {
                    return r2;
                }
            }
            else
            {
                return r1;
            }

            return null;
        }
        
        public static SyntaxParseResult<TestLexer> termfactorDivTerm(IList<Token<TestLexer>> tokens, int position)
        {
            SyntaxParseResult<TestLexer> r1 = null;
            SyntaxParseResult<TestLexer> r2 = null;
            SyntaxParseResult<TestLexer> r3 = null;
            
            r1 = Factor(tokens, position);
            if (r1.IsOk)
            {
                
                position = r1.EndingPosition;
                r2 = Terminal(tokens, position,TestLexer.DIVIDE,true);
                if (r2.IsOk)
                {
                    position = r2.EndingPosition;
                    r3 = Term(tokens, position);
                    if (r3.IsOk)
                    {
                        var root = new SyntaxNode<TestLexer>("times",
                            new List<ISyntaxNode<TestLexer>>() {r1.Root, r2.Root, r3.Root},
                            null
                        );
                        var r = new SyntaxParseResult<TestLexer>()
                        {
                            EndingPosition = r3.EndingPosition,
                            Errors = new List<UnexpectedTokenSyntaxError<TestLexer>>(),
                            IsError = false,
                            IsEnded = r3.IsEnded,
                            Root = root                            
                        };
                        return r;
                    }
                    else
                    {
                        return r3;
                    }
                }
                else
                {
                    return r2;
                }
            }
            else
            {
                return r1;
            }

            return null;
        }

        public static SyntaxParseResult<TestLexer> Term(IList<Token<TestLexer>> tokens, int position)
        {
            var r1 = termfactorTimesTerm(tokens, position);
            var r2 = termfactorDivTerm(tokens, position);
            var r3 = Factor(tokens, position);
            return Choice("term",r1, r2, r3);
        }

        public static SyntaxParseResult<TestLexer> Factor(IList<Token<TestLexer>> tokens, int position)
        {
            var r1 = FactorPrimary(tokens, position);
            var r2 = FactorMinusFactor(tokens, position);
            return Choice("factor",r1, r2);
        }
        
        public static SyntaxParseResult<TestLexer> FactorPrimary(IList<Token<TestLexer>> tokens, int position)
        {
            return Primary(tokens,position);
        }
        
        public static SyntaxParseResult<TestLexer> FactorMinusFactor(IList<Token<TestLexer>> tokens, int position)
        {
            SyntaxParseResult<TestLexer> r1 = null;
            SyntaxParseResult<TestLexer> r2 = null;
            r1 = Terminal(tokens, position,TestLexer.MINUS,true);
            if (r1.IsOk)
            {
                position = r1.EndingPosition;
                r2 = Factor(tokens, position);
                if (r2.IsOk)
                {
                    var root = new SyntaxNode<TestLexer>("minusFactor",
                        new List<ISyntaxNode<TestLexer>>() {r1.Root, r2.Root},
                        null
                    );
                    var r = new SyntaxParseResult<TestLexer>()
                    {
                        EndingPosition = r2.EndingPosition,
                        Errors = new List<UnexpectedTokenSyntaxError<TestLexer>>(),
                        IsError = false,
                        IsEnded = r2.IsEnded,
                        Root = root                            
                    };
                    return r;
                }
                else
                {
                    return r2;
                }
            }
            else
            {
                return r1;
            }
        }
        

        
    }
}