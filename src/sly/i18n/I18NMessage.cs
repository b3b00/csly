namespace sly.i18n
{
    public enum I18NMessage
    {
        UnexpectedTokenExpecting,
        UnexpectedEosExpecting,
        UnexpectedToken,
        UnexpectedEos,
        UnexpectedChar,
        
        CannotMixGenericAndRegex,
        DuplicateStringCharDelimiters,
        TooManyComment,
        TooManyMultilineComment,
        TooManySingleLineComment,
        CannotMixCommentAndSingleOrMulti,
        SameValueUsedManyTime,
        StringDelimiterMustBe1Char,
        StringDelimiterCannotBeLetterOrDigit,
        StringEscapeCharMustBe1Char,
        StringEscapeCharCannotBeLetterOrDigit,
        CharDelimiterMustBe1Char,
        CharDelimiterCannotBeLetter,
        CharEscapeCharMustBe1Char,
        CharEscapeCharCannotBeLetterOrDigit,
        SugarTokenCannotStartWithLetter,
        
        
        MissingOperand,
        ReferenceNotFound,
        MixedChoices,
        NonTerminalChoiceCannotBeDiscarded,
        IncorrectVisitorReturnType,
        IncorrectVisitorParameterType,
        IncorrectVisitorParameterNumber,
        LeftRecursion,
        NonTerminalNeverUsed,
        CannotUseExplicitTokensWithRegexLexer,
        ManyLexemWithSamelabel
        
    }
}