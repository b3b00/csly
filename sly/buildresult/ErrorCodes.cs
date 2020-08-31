namespace sly.buildresult
{
    public enum ErrorCodes
    {
        
        NOT_AN_ERROR = -1,
        
        #region Lexer
        
        
        LEXER_UNKNOWN_ERROR = 0,
        
        LEXER_CANNOT_MIX_GENERIC_AND_REGEX = 1,
        
        LEXER_DUPLICATE_STRING_CHAR_DELIMITERS = 2,
        
        LEXER_TOO_MANY_COMMNENT = 3,
        
        LEXER_TOO_MANY_MULTILINE_COMMNENT = 4,
        
        LEXER_TOO_MANY_SINGLELINE_COMMNENT = 5,
        
        LEXER_CANNOT_MIX_COMMENT_AND_SINGLE_OR_MULTI = 6,
        
        LEXER_SAME_VALUE_USED_MANY_TIME = 7,
        
        #endregion

        #region Parser

        PARSER_UNKNOWN_ERROR = 100,
        
        PARSER_MISSING_OPERAND = 101,
        
        PARSER_REFERENCE_NOT_FOUND = 102,
        
        PARSER_MIXED_CHOICES = 103,
        
        PARSER_NON_TERMINAL_CHOICE_CANNOT_BE_DISCARDED = 104,
        
        PARSER_INCORRECT_VISITOR_RETURN_TYPE = 105,
        
        PARSER_INCORRECT_VISITOR_PARAMETER_TYPE = 106,
        
        PARSER_INCORRECT_VISITOR_PARAMETER_NUMBER = 107,
        
        

        #endregion
        
        
    }
}