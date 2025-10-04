using System;
using System.Collections.Generic;
using sly.lexer;
using sly.parser.parser;

namespace sly.parser.generator.visitor;

public class SyntaxVisitorResult<IN, OUT> where IN : struct, Enum
{
    public List<Group<IN, OUT>> GroupListResult;

    public Group<IN, OUT> GroupResult;
        
    public Group<IN, object> RelaxedGroupResult;

    public ValueOption<Group<IN, OUT>> OptionGroupResult;

    public ValueOption<Group<IN,object>> RelaxedOptionGroupResult;

    public ValueOption<OUT> OptionResult;

    public ValueOption<object> RelaxedOptionResult;

    public List<Token<IN>> TokenListResult;

    public Token<IN> TokenResult;

    public List<OUT> ValueListResult;
        
    public object  RelaxedValueListResult;

    public OUT ValueResult;

    public object RelaxedValueResult;

    public bool IsOption => OptionResult != null || RelaxedOptionResult != null;
    public bool IsOptionGroup => OptionGroupResult != null  || RelaxedOptionGroupResult != null;

    public bool IsToken { get; private set; }

    public bool Discarded => IsToken && TokenResult != null && TokenResult.Discarded;
    public bool IsValue { get; private set; }
    public bool IsValueList { get; private set; }

    public bool IsGroupList { get; private set; }

    public bool IsTokenList { get; private set; }

    public bool IsGroup { get; private set; }

    public static SyntaxVisitorResult<IN, OUT> NewToken(Token<IN> tok)
    {
        var res = new SyntaxVisitorResult<IN, OUT>();
        res.TokenResult = tok;
        res.IsToken = true;
        return res;
    }

    public static SyntaxVisitorResult<IN, OUT> NewValue(OUT val)
    {
        var res = new SyntaxVisitorResult<IN, OUT>();
        res.ValueResult = val;
        res.IsValue = true;
        return res;
    }
        
    public static SyntaxVisitorResult<IN, OUT> NewRelaxedValue(object val)
    {
        var res = new SyntaxVisitorResult<IN, OUT>();
        res.RelaxedValueResult = val;
        res.IsValue = true;
        return res;
    }

    public static SyntaxVisitorResult<IN, OUT> NewValueList(List<OUT> values)
    {
        var res = new SyntaxVisitorResult<IN, OUT>();
        res.ValueListResult = values;
        res.IsValueList = true;
        return res;
    }
        
    public static SyntaxVisitorResult<IN, OUT> NewRelaxedValueList(List<object> values)
    {
        var res = new SyntaxVisitorResult<IN, OUT>();
        res.RelaxedValueListResult = values;
        res.IsValueList = true;
        return res;
    }
        
       

    public static SyntaxVisitorResult<IN, OUT> NewGroupList(List<Group<IN, OUT>> values)
    {
        var res = new SyntaxVisitorResult<IN, OUT>();
        res.GroupListResult = values;
        res.IsGroupList = true;
        return res;
    }
        
    public static SyntaxVisitorResult<IN, OUT> NewRelaxedGroupList(List<Group<IN, object>> values)
    {
        var res = new SyntaxVisitorResult<IN, OUT>();
        res.RelaxedGroupListResult = values;
        res.IsGroupList = true;
        return res;
    }

    public static SyntaxVisitorResult<IN, OUT> NewRelaxedGroupOption(ValueOption<Group<IN, object>> option)
    {
        var res = new SyntaxVisitorResult<IN, OUT>();
        res.RelaxedOptionGroupResult = option;
        return res;
    }

    public List<Group<IN, object>> RelaxedGroupListResult { get; set; }

    public static SyntaxVisitorResult<IN, OUT> NewTokenList(List<Token<IN>> tokens)
    {
        var res = new SyntaxVisitorResult<IN, OUT>();
        res.TokenListResult = tokens;
        res.IsTokenList = true;
        return res;
    }

    public static SyntaxVisitorResult<IN, OUT> NewOptionSome(OUT value)
    {
        var res = new SyntaxVisitorResult<IN, OUT>();
        res.OptionResult = ValueOptionConstructors.Some<OUT>(value);
        return res;
    }
        
    public static SyntaxVisitorResult<IN, OUT> NewOptionGroupSome(Group<IN,object> value)
    {
        var res = new SyntaxVisitorResult<IN, OUT>();
        res.RelaxedOptionGroupResult = ValueOptionConstructors.Some<Group<IN,object>>(value);
        return res;
    }
        
    public static SyntaxVisitorResult<IN, OUT> NewOptionSomeRelaxed(object value)
    {
        var res = new SyntaxVisitorResult<IN, OUT>();
        res.RelaxedOptionResult = ValueOptionConstructors.Some<object>(value);
        return res;
    }
        

    public static SyntaxVisitorResult<IN, OUT> NewOptionGroupSome(Group<IN, OUT> group)
    {
        var res = new SyntaxVisitorResult<IN, OUT>();
        res.OptionGroupResult = ValueOptionConstructors.Some<Group<IN, OUT>>(group);
        return res;
    }
        
    public static SyntaxVisitorResult<IN, OUT> NewOptionGroupNone()
    {
        var res = new SyntaxVisitorResult<IN, OUT>();
        res.OptionGroupResult = ValueOptionConstructors.NoneGroup<IN,OUT>();
        return res;
    }
    

    public static SyntaxVisitorResult<IN, OUT> NewOptionNone()
    {
        var res = new SyntaxVisitorResult<IN, OUT>();
        res.OptionResult = ValueOptionConstructors.None<OUT>();
        return res;
    }

    public static SyntaxVisitorResult<IN, OUT> NewOptionNoneRelaxed()
    {
        var res = new SyntaxVisitorResult<IN, OUT>();
        res.RelaxedOptionGroupResult = ValueOptionConstructors.None<Group<IN, object>>();
        return res;
    }
    
    public static SyntaxVisitorResult<IN, OUT> NewOptionGroupNoneRelaxed()
    {
        var res = new SyntaxVisitorResult<IN, OUT>();
        res.RelaxedOptionGroupResult = ValueOptionConstructors.None<Group<IN, object>>();
        return res;
    }

    public static SyntaxVisitorResult<IN, OUT> NewGroup(Group<IN, OUT> group)
    {
        var res = new SyntaxVisitorResult<IN, OUT>();
        res.GroupResult = group;
        res.IsGroup = true;
        return res;
    }
        
    public static SyntaxVisitorResult<IN, OUT> NewRelaxedGroup(Group<IN, object> group)
    {
        var res = new SyntaxVisitorResult<IN, OUT>();
        res.RelaxedGroupResult = group;
        res.IsGroup = true;
        return res;
    }

    public static SyntaxVisitorResult<IN, OUT> NoneResult()
    {
        var res = new SyntaxVisitorResult<IN, OUT>();
        return res;
    }

}