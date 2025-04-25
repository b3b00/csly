using System;
using System.Collections.Generic;
using sly.buildresult;
using sly.lexer.fsm;

namespace sly.lexer.fluent;

public interface IFluentLexemeBuilder<IN> : IFluentLexerBuilder<IN> where IN : struct, Enum {
    
    IFluentLexemeBuilder<IN> WithLabel(string lang, string label);
    
    IFluentLexemeBuilder<IN> WithLabels(params (string lang, string label)[] labels);
    
    IFluentLexemeBuilder<IN> WithModes(params string[] modes);
    
    IFluentLexemeBuilder<IN> OnChannel(int channel);
    
    IFluentLexemeBuilder<IN> PopMode();
    
    IFluentLexemeBuilder<IN> PushToMode(string targetMode);
    
}