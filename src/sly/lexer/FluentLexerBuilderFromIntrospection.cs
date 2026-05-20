using System;
using System.Collections.Generic;
using System.Reflection;
using sly.buildresult;
using sly.i18n;
using sly.lexer.fluent;

namespace sly.lexer;



public class FluentLexerBuilderFromIntrospection
{
    
    
    public static IFluentLexerBuilder<IN> BuildFluentLexerBuilder<IN>() where IN : struct, Enum
    {
        var builder = FluentLexerBuilder<IN>.NewBuilder();
        BuildResult<ILexer<IN>> result = new BuildResult<ILexer<IN>>();
        var attributes = LexerBuilder.GetLexemesWithReflection<IN>(result);
        var lexerAttribute = typeof(IN).GetCustomAttribute<LexerAttribute>();


        if (lexerAttribute != null)
        {
            builder.IgnoreEol(lexerAttribute.IgnoreEOL)
                .IgnoreEol(lexerAttribute.KeyWordIgnoreCase)
                .IgnoreWhiteSpace(lexerAttribute.IgnoreWS)
                .IsIndentationAware(lexerAttribute.IndentationAWare);
        }
        foreach (var attribute in attributes)
        {
            var key = attribute.Key;
            var value = attribute.Value;
            (List<LexemeAttribute> lexemes, List<LexemeLabelAttribute> labels) = attribute.Value;
            foreach (var lexeme in lexemes)
            {
                if (lexeme.IsDouble)
                    builder.Double(key, lexeme.GetSafeGenericTokenParameter(0));
                if (lexeme.IsIdentifier)
                {
                    switch (lexeme.IdentifierType)
                    {
                        case IdentifierType.Alpha:
                        {
                            builder.AlphaId(key);
                            break;
                        }
                        case IdentifierType.AlphaNumeric:
                        {
                            builder.AlphaNumId(key);
                            break;
                        }
                        case IdentifierType.AlphaNumericDash:
                        {
                            builder.AlphaNumDashId(key);
                            break;
                        }
                        case IdentifierType.Custom:
                        {
                            builder.CustomId(key, lexeme.GenericTokenParameters[0], lexeme.GenericTokenParameters[1]);
                            break;
                        }
                    }
                }

                if (lexeme.IsInteger)
                {
                    builder.Int(key);
                }
                if (lexeme.IsString)
                {
                    builder.String(key,lexeme.GetSafeGenericTokenParameter(0),lexeme.GetSafeGenericTokenParameter(1));
                }
                if (lexeme.IsDate)
                {
                    builder.Date(key, EnumHelper.ParseEnum<DateFormat>(lexeme.GetSafeGenericTokenParameter(0)), lexeme.GetSafeGenericTokenParameter(1));
                }
                if (lexeme.IsHexa)
                {
                    builder.Hexa(key, lexeme.GetSafeGenericTokenParameter(0));
                }
                if (lexeme.IsKeyWord)
                {
                    builder.Keyword(key, lexeme.GetSafeGenericTokenParameter(0));
                }
                if (lexeme.IsSugar)
                {
                    builder.Sugar(key, lexeme.GetSafeGenericTokenParameter(0));
                }
                if (lexeme.IsChar)
                {
                    builder.String(key,lexeme.GetSafeGenericTokenParameter(0),lexeme.GetSafeGenericTokenParameter(1));
                }
                if (lexeme.IsComment)
                {
                    var startSingle =  lexeme.GetSafeGenericTokenParameter(0);
                    var startMulti = lexeme.GetSafeGenericTokenParameter(1);
                    var endMulti = lexeme.GetSafeGenericTokenParameter(2);
                    builder.Comment(key, startSingle, startMulti, endMulti);
                }
                
                // --- modes
                
                if (lexeme.IsPop)
                {
                    builder.Pop(key);
                }
                if (lexeme.IsPush)
                {
                    builder.Push(key, lexeme.Pushtarget);
                }   
            }
        }
                
                
            
        return builder;
    }
}