using System.Collections.Generic;
using sly.lexer;
using sly.parser.generator;
using sly.parser.parser;

namespace ParserExample
{
    public class ScriptParser
    {
        [Production("test : LBEG [d] fun_call LEND [d]")]
        public object TestStatement(object statement)
        {
            return null;
        }

        [Production("fun_call : ID LPAR [d] ( args2 )? RPAR [d]")]
        public object FuncCall(Token<ScriptToken> id, ValueOption<Group<ScriptToken, object>> args)
        {
            return null;
        }

        [Production("fun_actual_args : kw_args")]
        public object FuncActualArgsA(object kwArgs)
        {
            return null;
        }

        [Production("fun_actual_args2 : args2")]
        public object actargs2(object args)
        {
            return null;
        }

        [Production("args2 : arg (COMMA[d] arg)*")]
        public object argies(object head, List<Group<ScriptToken, object>> tail)
        {
            return null;
        }

        [Production("arg : kw_arg")]
        [Production("arg : arith_expr")]
        public object a(object v)
        {
            return null;
        }

        //[Production("fun_actual_args : pos_args ( COMMA [d] kw_args )?")]
        
        [Production("fun_actual_args : pos_args optional_kw_args?")]
        public object FuncActualArgsB(object posArgs, ValueOption<Group<ScriptToken, object>> kwArgs)
        {
            return null;
        }

        [Production("optional_kw_args : COMMA[d] kw_args")]
        public object optionals(object o)
        {
            return null;
        }
        

        [Production("pos_args : arith_expr ( COMMA [d] arith_expr )*")]
        public object Posobject(object initial, List<Group<ScriptToken, object>> subsequent)
        {
            return null;
        }

        [Production("kw_args : kw_arg ( COMMA [d] kw_arg )*")]
        public object Kwobject(object initial, List<Group<ScriptToken, object>> subsequent)
        {
            return null;
        }

        [Production("kw_arg : ID DEFINE [d] arith_expr")]
        public object KeywordArg(Token<ScriptToken> id, object expression)
        {
            return null;
        }

        [Production("arith_expr: INT")]
        [Production("arith_expr: ID")]
        [Production("arith_expr: STRING")]
        public object arith(Token<ScriptToken> tok)
        {
            return null;
        }
    }
    
    
    public class ScriptParserRight
    {
        [Production("test : LBEG [d] fun_call LEND [d]")]
        public object TestStatement(object statement)
        {
            return null;
        }

        [Production("fun_call : ID LPAR [d] ( fun_actual_args )? RPAR [d]")]
        public object FuncCall(Token<ScriptToken> id, ValueOption<Group<ScriptToken, object>> args)
        {
            return null;
        }

        [Production("fun_actual_args : kw_args")]
        public object FuncActualArgsA(object kwArgs)
        {
            return null;
        }

        [Production("fun_actual_args2 : args2")]
        public object actargs2(object args)
        {
            return null;
        }

        [Production("args2 : arg (COMMA[d] arg)*")]
        public object argies(object head, List<Group<ScriptToken, object>> tail)
        {
            return null;
        }

        [Production("arg : kw_arg")]
        [Production("arg : arith_expr")]
        public object a(object v)
        {
            return null;
        }

        //[Production("fun_actual_args : pos_args ( COMMA [d] kw_args )?")]
        
        [Production("fun_actual_args : pos_args kw_args?")]
        public object FuncActualArgsB(object posArgs, ValueOption<object> kwArgs)
        {
            return null;
        }

        // [Production("optional_kw_args : COMMA[d] kw_args")]
        // public object optionals(object o)
        // {
        //     return null;
        // }
        
        [Production("pos_args :  (  arith_expr COMMA [d] )* kw_arg")]
        public object Posobject(List<Group<ScriptToken, object>> head, object tail)
        {
            return null;
        }

        [Production("pos_args :  (  arith_expr COMMA [d] )* arith_expr")]
        public object Posobject2(List<Group<ScriptToken, object>> head, object tail)
        {
            return null;
        }

        [Production("kw_args : ( COMMA [d] kw_arg )* kw_arg ")]
        public object Kwobject(List<Group<ScriptToken, object>> head, object tail )
        {
            return null;
        }
        
        [Production("kw_args : ( COMMA [d] kw_arg )* ")]
        public object Kwobject2(List<Group<ScriptToken, object>> head )
        {
            return null;
        }

        [Production("kw_arg : ID DEFINE [d] arith_expr")]
        public object KeywordArg(Token<ScriptToken> id, object expression)
        {
            return null;
        }

        [Production("arith_expr: INT")]
        [Production("arith_expr: ID")]
        [Production("arith_expr: STRING")]
        public object arith(Token<ScriptToken> tok)
        {
            return null;
        }
    }
}