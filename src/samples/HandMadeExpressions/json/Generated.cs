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

    public Match<JsonTokenGeneric,JSon> _root(IList<Token<JsonTokenGeneric>> tokens, int position) {
        var parser = Sequence(_value);
        var result = parser(tokens,position);
        return result;
    }


    public Match<JsonTokenGeneric,JSon> _value_0(IList<Token<JsonTokenGeneric>> tokens, int position) {
        var parser = Sequence(TerminalParser(expectedTokens:JsonTokenGeneric.STRING));
        var result = parser(tokens,position);
        return result;
    }


    public Match<JsonTokenGeneric,JSon> _value_1(IList<Token<JsonTokenGeneric>> tokens, int position) {
        var parser = Sequence(TerminalParser(expectedTokens:JsonTokenGeneric.INT));
        var result = parser(tokens,position);
        return result;
    }


    public Match<JsonTokenGeneric,JSon> _value_2(IList<Token<JsonTokenGeneric>> tokens, int position) {
        var parser = Sequence(TerminalParser(expectedTokens:JsonTokenGeneric.DOUBLE));
        var result = parser(tokens,position);
        return result;
    }


    public Match<JsonTokenGeneric,JSon> _value_3(IList<Token<JsonTokenGeneric>> tokens, int position) {
        var parser = Sequence(TerminalParser(expectedTokens:JsonTokenGeneric.BOOLEAN));
        var result = parser(tokens,position);
        return result;
    }


    public Match<JsonTokenGeneric,JSon> _value_4(IList<Token<JsonTokenGeneric>> tokens, int position) {
        var parser = Sequence(DiscardedTerminalParser(expectedTokens:JsonTokenGeneric.NULL));
        var result = parser(tokens,position);
        return result;
    }


    public Match<JsonTokenGeneric,JSon> _value_5(IList<Token<JsonTokenGeneric>> tokens, int position) {
        var parser = Sequence(_object);
        var result = parser(tokens,position);
        return result;
    }


    public Match<JsonTokenGeneric,JSon> _value_6(IList<Token<JsonTokenGeneric>> tokens, int position) {
        var parser = Sequence(_list);
        var result = parser(tokens,position);
        return result;
    }


    public Match<JsonTokenGeneric,JSon> _object_0(IList<Token<JsonTokenGeneric>> tokens, int position) {
        var parser = Sequence(DiscardedTerminalParser(expectedTokens:JsonTokenGeneric.ACCG), DiscardedTerminalParser(expectedTokens:JsonTokenGeneric.ACCD));
        var result = parser(tokens,position);
        return result;
    }


    public Match<JsonTokenGeneric,JSon> _object_1(IList<Token<JsonTokenGeneric>> tokens, int position) {
        var parser = Sequence(DiscardedTerminalParser(expectedTokens:JsonTokenGeneric.ACCG), _members, DiscardedTerminalParser(expectedTokens:JsonTokenGeneric.ACCD));
        var result = parser(tokens,position);
        return result;
    }


    public Match<JsonTokenGeneric,JSon> _list_0(IList<Token<JsonTokenGeneric>> tokens, int position) {
        var parser = Sequence(DiscardedTerminalParser(expectedTokens:JsonTokenGeneric.CROG), DiscardedTerminalParser(expectedTokens:JsonTokenGeneric.CROD));
        var result = parser(tokens,position);
        return result;
    }


    public Match<JsonTokenGeneric,JSon> _list_1(IList<Token<JsonTokenGeneric>> tokens, int position) {
        var parser = Sequence(DiscardedTerminalParser(expectedTokens:JsonTokenGeneric.CROG), _listElements, DiscardedTerminalParser(expectedTokens:JsonTokenGeneric.CROD));
        var result = parser(tokens,position);
        return result;
    }


    public Match<JsonTokenGeneric,JSon> _listElements(IList<Token<JsonTokenGeneric>> tokens, int position) {
        var parser = Sequence(_value, ZeroOrMoreValue(_additionalValue));
        var result = parser(tokens,position);
        return result;
    }


    public Match<JsonTokenGeneric,JSon> _additionalValue(IList<Token<JsonTokenGeneric>> tokens, int position) {
        var parser = Sequence(TerminalParser(expectedTokens:JsonTokenGeneric.COMMA), _value);
        var result = parser(tokens,position);
        return result;
    }


    public Match<JsonTokenGeneric,JSon> _members(IList<Token<JsonTokenGeneric>> tokens, int position) {
        var parser = Sequence(_property, ZeroOrMoreValue(_additionalProperty));
        var result = parser(tokens,position);
        return result;
    }


    public Match<JsonTokenGeneric,JSon> _additionalProperty(IList<Token<JsonTokenGeneric>> tokens, int position) {
        var parser = Sequence(TerminalParser(expectedTokens:JsonTokenGeneric.COMMA), _property);
        var result = parser(tokens,position);
        return result;
    }


    public Match<JsonTokenGeneric,JSon> _property(IList<Token<JsonTokenGeneric>> tokens, int position) {
        var parser = Sequence(TerminalParser(expectedTokens:JsonTokenGeneric.STRING), DiscardedTerminalParser(expectedTokens:JsonTokenGeneric.COLON), _value);
        var result = parser(tokens,position);
        return result;
    }


    public Match<JsonTokenGeneric,JSon> _value(IList<Token<JsonTokenGeneric>> tokens, int position) {
        var parser = Alternate(_value_0, _value_1, _value_2, _value_3, _value_4, _value_5, _value_6);
        var result = parser(tokens,position);
        return result;
    }

    public Match<JsonTokenGeneric,JSon> _object(IList<Token<JsonTokenGeneric>> tokens, int position) {
        var parser = Alternate(_object_0, _object_1);
        var result = parser(tokens,position);
        return result;
    }

    public Match<JsonTokenGeneric,JSon> _list(IList<Token<JsonTokenGeneric>> tokens, int position) {
        var parser = Alternate(_list_0, _list_1);
        var result = parser(tokens,position);
        return result;
    }
}
