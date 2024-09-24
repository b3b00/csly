using System;
using sly.lexer;
using handExpressions;
using handExpressions.jsonparser;
using System;
using System.Collections.Generic;
using handExpressions.jsonparser.JsonModel;
using sly.lexer;
using sly.parser.generator;

namespace handExpressions.jsonparser;

public class GeneratedEbnfJsonGenericParser : BaseParser<JsonTokenGeneric,JSon> {

    EbnfJsonGenericParser _instance;

    public GeneratedEbnfJsonGenericParser(EbnfJsonGenericParser instance) {
        _instance = instance;
    }

    public Match<JsonTokenGeneric,JSon> Root(IList<Token<JsonTokenGeneric>> tokens, int position) {
        Func<object[],JSon> visitor = (object[] args) => {
            var result = _instance.Root((JSon)args[0]);
            return result;
        };
        var parser = Sequence(Value);
        var result = parser(tokens,position);
        return result;
    }


    private Match<JsonTokenGeneric,JSon> Value_0(IList<Token<JsonTokenGeneric>> tokens, int position) {
        Func<object[],JSon> visitor = (object[] args) => {
            var result = _instance.StringValue((Token<JsonTokenGeneric>)args[0]);
            return result;
        };
        var parser = Sequence(TerminalParser(expectedTokens:JsonTokenGeneric.STRING));
        var result = parser(tokens,position);
        return result;
    }


    private Match<JsonTokenGeneric,JSon> Value_1(IList<Token<JsonTokenGeneric>> tokens, int position) {
        Func<object[],JSon> visitor = (object[] args) => {
            var result = _instance.IntValue((Token<JsonTokenGeneric>)args[0]);
            return result;
        };
        var parser = Sequence(TerminalParser(expectedTokens:JsonTokenGeneric.INT));
        var result = parser(tokens,position);
        return result;
    }


    private Match<JsonTokenGeneric,JSon> Value_2(IList<Token<JsonTokenGeneric>> tokens, int position) {
        Func<object[],JSon> visitor = (object[] args) => {
            var result = _instance.DoubleValue((Token<JsonTokenGeneric>)args[0]);
            return result;
        };
        var parser = Sequence(TerminalParser(expectedTokens:JsonTokenGeneric.DOUBLE));
        var result = parser(tokens,position);
        return result;
    }


    private Match<JsonTokenGeneric,JSon> Value_3(IList<Token<JsonTokenGeneric>> tokens, int position) {
        Func<object[],JSon> visitor = (object[] args) => {
            var result = _instance.BooleanValue((Token<JsonTokenGeneric>)args[0]);
            return result;
        };
        var parser = Sequence(TerminalParser(expectedTokens:JsonTokenGeneric.BOOLEAN));
        var result = parser(tokens,position);
        return result;
    }


    private Match<JsonTokenGeneric,JSon> Value_4(IList<Token<JsonTokenGeneric>> tokens, int position) {
        Func<object[],JSon> visitor = (object[] args) => {
            var result = _instance.NullValue();
            return result;
        };
        var parser = Sequence(DiscardedTerminalParser(expectedTokens:JsonTokenGeneric.NULL));
        var result = parser(tokens,position);
        return result;
    }


    private Match<JsonTokenGeneric,JSon> Value_5(IList<Token<JsonTokenGeneric>> tokens, int position) {
        Func<object[],JSon> visitor = (object[] args) => {
            var result = _instance.ObjectValue((JSon)args[0]);
            return result;
        };
        var parser = Sequence(Object);
        var result = parser(tokens,position);
        return result;
    }


    private Match<JsonTokenGeneric,JSon> Value_6(IList<Token<JsonTokenGeneric>> tokens, int position) {
        Func<object[],JSon> visitor = (object[] args) => {
            var result = _instance.ListValue((JList)args[0]);
            return result;
        };
        var parser = Sequence(List);
        var result = parser(tokens,position);
        return result;
    }


    private Match<JsonTokenGeneric,JSon> Object_0(IList<Token<JsonTokenGeneric>> tokens, int position) {
        Func<object[],JSon> visitor = (object[] args) => {
            var result = _instance.EmptyObjectValue();
            return result;
        };
        var parser = Sequence(DiscardedTerminalParser(expectedTokens:JsonTokenGeneric.ACCG), DiscardedTerminalParser(expectedTokens:JsonTokenGeneric.ACCD));
        var result = parser(tokens,position);
        return result;
    }


    private Match<JsonTokenGeneric,JSon> Object_1(IList<Token<JsonTokenGeneric>> tokens, int position) {
        Func<object[],JSon> visitor = (object[] args) => {
            var result = _instance.AttributesObjectValue((JObject)args[0]);
            return result;
        };
        var parser = Sequence(DiscardedTerminalParser(expectedTokens:JsonTokenGeneric.ACCG), Members, DiscardedTerminalParser(expectedTokens:JsonTokenGeneric.ACCD));
        var result = parser(tokens,position);
        return result;
    }


    private Match<JsonTokenGeneric,JSon> List_0(IList<Token<JsonTokenGeneric>> tokens, int position) {
        Func<object[],JSon> visitor = (object[] args) => {
            var result = _instance.EmptyList();
            return result;
        };
        var parser = Sequence(DiscardedTerminalParser(expectedTokens:JsonTokenGeneric.CROG), DiscardedTerminalParser(expectedTokens:JsonTokenGeneric.CROD));
        var result = parser(tokens,position);
        return result;
    }


    private Match<JsonTokenGeneric,JSon> List_1(IList<Token<JsonTokenGeneric>> tokens, int position) {
        Func<object[],JSon> visitor = (object[] args) => {
            var result = _instance.List((JList)args[0]);
            return result;
        };
        var parser = Sequence(DiscardedTerminalParser(expectedTokens:JsonTokenGeneric.CROG), ListElements, DiscardedTerminalParser(expectedTokens:JsonTokenGeneric.CROD));
        var result = parser(tokens,position);
        return result;
    }


    public Match<JsonTokenGeneric,JSon> ListElements(IList<Token<JsonTokenGeneric>> tokens, int position) {
        Func<object[],JSon> visitor = (object[] args) => {
            var result = _instance.listElements((JSon)args[0], (List<JSon>)args[1]);
            return result;
        };
        var parser = Sequence(Value, ZeroOrMoreValue(AdditionalValue));
        var result = parser(tokens,position);
        return result;
    }


    public Match<JsonTokenGeneric,JSon> AdditionalValue(IList<Token<JsonTokenGeneric>> tokens, int position) {
        Func<object[],JSon> visitor = (object[] args) => {
            var result = _instance.ListElementsOne((Token<JsonTokenGeneric>)args[0], (JSon)args[1]);
            return result;
        };
        var parser = Sequence(TerminalParser(expectedTokens:JsonTokenGeneric.COMMA), Value);
        var result = parser(tokens,position);
        return result;
    }


    public Match<JsonTokenGeneric,JSon> Members(IList<Token<JsonTokenGeneric>> tokens, int position) {
        Func<object[],JSon> visitor = (object[] args) => {
            var result = _instance.Members((JObject)args[0], (List<JSon>)args[1]);
            return result;
        };
        var parser = Sequence(Property, ZeroOrMoreValue(AdditionalProperty));
        var result = parser(tokens,position);
        return result;
    }


    public Match<JsonTokenGeneric,JSon> AdditionalProperty(IList<Token<JsonTokenGeneric>> tokens, int position) {
        Func<object[],JSon> visitor = (object[] args) => {
            var result = _instance.property((Token<JsonTokenGeneric>)args[0], (JObject)args[1]);
            return result;
        };
        var parser = Sequence(TerminalParser(expectedTokens:JsonTokenGeneric.COMMA), Property);
        var result = parser(tokens,position);
        return result;
    }


    public Match<JsonTokenGeneric,JSon> Property(IList<Token<JsonTokenGeneric>> tokens, int position) {
        Func<object[],JSon> visitor = (object[] args) => {
            var result = _instance.property((Token<JsonTokenGeneric>)args[0], (JSon)args[1]);
            return result;
        };
        var parser = Sequence(TerminalParser(expectedTokens:JsonTokenGeneric.STRING), DiscardedTerminalParser(expectedTokens:JsonTokenGeneric.COLON), Value);
        var result = parser(tokens,position);
        return result;
    }


    public Match<JsonTokenGeneric,JSon> Value(IList<Token<JsonTokenGeneric>> tokens, int position) {
        var parser = Alternate(Value_0, Value_1, Value_2, Value_3, Value_4, Value_5, Value_6);
        var result = parser(tokens,position);
        return result;
    }

    public Match<JsonTokenGeneric,JSon> Object(IList<Token<JsonTokenGeneric>> tokens, int position) {
        var parser = Alternate(Object_0, Object_1);
        var result = parser(tokens,position);
        return result;
    }

    public Match<JsonTokenGeneric,JSon> List(IList<Token<JsonTokenGeneric>> tokens, int position) {
        var parser = Alternate(List_0, List_1);
        var result = parser(tokens,position);
        return result;
    }
}
