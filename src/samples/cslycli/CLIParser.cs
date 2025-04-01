using sly.lexer;
using sly.parser.generator;
using System.Collections.Generic;
using sly.parser.parser;

namespace csly.cli
{
    [ParserRoot("root")]
    public class CLIParser
    {
        [Production("root : genericRoot parserRoot")]
        public object root_genericRoot_parserRoot(object p0, object p1)
        {
            return default(object);
        }

        [Production("parserRoot : PARSER ID SEMICOLON parser_optimization * rule *")]
        public object parserRoot_PARSER_ID_SEMICOLON_parseroptimization_rule_(Token<CLIToken> p0, Token<CLIToken> p1, Token<CLIToken> p2, List<object> p3, List<object> p4)
        {
            return default(object);
        }

        [Production("parser_optimization : LEFTBRACKET [ USEMEMOIZATION | BROADENTOKENWINDOW | AUTOCLOSEINDENTATION ] RIGHTBRACKET")]
        public object parseroptimization_LEFTBRACKET_USEMEMOIZATION_BROADENTOKENWINDOW_AUTOCLOSEINDENTATION_RIGHTBRACKET(Token<CLIToken> p0, Token<CLIToken> p1, Token<CLIToken> p2)
        {
            return default(object);
        }

        [Production("genericRoot : GENERICLEXER ID SEMICOLON lexer_option * modedToken *")]
        public object genericRoot_GENERICLEXER_ID_SEMICOLON_lexeroption_modedToken_(Token<CLIToken> p0, Token<CLIToken> p1, Token<CLIToken> p2, List<object> p3, List<object> p4)
        {
            return default(object);
        }

        [Production("modedToken : mode * token")]
        public object modedToken_mode_token(List<object> p0, object p1)
        {
            return default(object);
        }

        [Production("mode : LEFTBRACKET PUSH LEFTPAREN STRING RIGHTPAREN RIGHTBRACKET")]
        public object mode_LEFTBRACKET_PUSH_LEFTPAREN_STRING_RIGHTPAREN_RIGHTBRACKET(Token<CLIToken> p0, Token<CLIToken> p1, Token<CLIToken> p2, Token<CLIToken> p3, Token<CLIToken> p4, Token<CLIToken> p5)
        {
            return default(object);
        }

        [Production("mode : LEFTBRACKET POP RIGHTBRACKET")]
        public object mode_LEFTBRACKET_POP_RIGHTBRACKET(Token<CLIToken> p0, Token<CLIToken> p1, Token<CLIToken> p2)
        {
            return default(object);
        }

        [Production("mode : LEFTBRACKET MODE LEFTPAREN STRING (COMMA STRING) * RIGHTPAREN RIGHTBRACKET")]
        public object mode_LEFTBRACKET_MODE_LEFTPAREN_STRING_COMMA_STRING_RIGHTPAREN_RIGHTBRACKET(Token<CLIToken> p0, Token<CLIToken> p1, Token<CLIToken> p2, Token<CLIToken> p3, List<Group<CLIToken, object>> p4, Token<CLIToken> p5, Token<CLIToken> p6)
        {
            return default(object);
        }

        [Production("mode : LEFTBRACKET MODE RIGHTBRACKET")]
        public object mode_LEFTBRACKET_MODE_RIGHTBRACKET(Token<CLIToken> p0, Token<CLIToken> p1, Token<CLIToken> p2)
        {
            return default(object);
        }

        [Production("token : attribute * LEFTBRACKET [ SUGARTOKEN | SINGLELINECOMMENT | HEXATOKEN ] RIGHTBRACKET IdentifierOrString COLON STRING SEMICOLON")]
        public object token_attribute_LEFTBRACKET_SUGARTOKEN_SINGLELINECOMMENT_HEXATOKEN_RIGHTBRACKET_IdentifierOrString_COLON_STRING_SEMICOLON(List<object> p0, Token<CLIToken> p1, Token<CLIToken> p2, Token<CLIToken> p3, object p4, Token<CLIToken> p5, Token<CLIToken> p6, Token<CLIToken> p7)
        {
            return default(object);
        }

        [Production("token : attribute * LEFTBRACKET [ STRINGTOKEN | CHARTOKEN | MULTILINECOMMENT ] RIGHTBRACKET IdentifierOrString COLON STRING STRING SEMICOLON")]
        public object token_attribute_LEFTBRACKET_STRINGTOKEN_CHARTOKEN_MULTILINECOMMENT_RIGHTBRACKET_IdentifierOrString_COLON_STRING_STRING_SEMICOLON(List<object> p0, Token<CLIToken> p1, Token<CLIToken> p2, Token<CLIToken> p3, object p4, Token<CLIToken> p5, Token<CLIToken> p6, Token<CLIToken> p7, Token<CLIToken> p8)
        {
            return default(object);
        }

        [Production("token : attribute * LEFTBRACKET [ KEYWORDTOKEN ] RIGHTBRACKET IdentifierOrString COLON STRING * SEMICOLON")]
        public object token_attribute_LEFTBRACKET_KEYWORDTOKEN_RIGHTBRACKET_IdentifierOrString_COLON_STRING_SEMICOLON(List<object> p0, Token<CLIToken> p1, Token<CLIToken> p2, Token<CLIToken> p3, object p4, Token<CLIToken> p5, List<Token<CLIToken>> p6, Token<CLIToken> p7)
        {
            return default(object);
        }

        [Production("token : attribute * LEFTBRACKET DATETOKEN RIGHTBRACKET IdentifierOrString COLON [ DDMMYYYY | YYYYMMDD ] CHAR SEMICOLON")]
        public object token_attribute_LEFTBRACKET_DATETOKEN_RIGHTBRACKET_IdentifierOrString_COLON_DDMMYYYY_YYYYMMDD_CHAR_SEMICOLON(List<object> p0, Token<CLIToken> p1, Token<CLIToken> p2, Token<CLIToken> p3, object p4, Token<CLIToken> p5, Token<CLIToken> p6, Token<CLIToken> p7, Token<CLIToken> p8)
        {
            return default(object);
        }

        [Production("token : attribute * LEFTBRACKET UPTOTOKEN RIGHTBRACKET IdentifierOrString COLON STRING * SEMICOLON")]
        public object token_attribute_LEFTBRACKET_UPTOTOKEN_RIGHTBRACKET_IdentifierOrString_COLON_STRING_SEMICOLON(List<object> p0, Token<CLIToken> p1, Token<CLIToken> p2, Token<CLIToken> p3, object p4, Token<CLIToken> p5, List<Token<CLIToken>> p6, Token<CLIToken> p7)
        {
            return default(object);
        }

        [Production("token : attribute * LEFTBRACKET [ STRINGTOKEN | INTTOKEN | ALPHAIDTOKEN | ALPHANUMIDTOKEN | ALPHANUMDASHIDTOKEN | DOUBLETOKEN | HEXATOKEN ] RIGHTBRACKET IdentifierOrString SEMICOLON")]
        public object token_attribute_LEFTBRACKET_STRINGTOKEN_INTTOKEN_ALPHAIDTOKEN_ALPHANUMIDTOKEN_ALPHANUMDASHIDTOKEN_DOUBLETOKEN_HEXATOKEN_RIGHTBRACKET_IdentifierOrString_SEMICOLON(List<object> p0, Token<CLIToken> p1, Token<CLIToken> p2, Token<CLIToken> p3, object p4, Token<CLIToken> p5)
        {
            return default(object);
        }

        [Production("token : attribute * LEFTBRACKET EXTENSIONTOKEN RIGHTBRACKET IdentifierOrString extension")]
        public object token_attribute_LEFTBRACKET_EXTENSIONTOKEN_RIGHTBRACKET_IdentifierOrString_extension(List<object> p0, Token<CLIToken> p1, Token<CLIToken> p2, Token<CLIToken> p3, object p4, object p5)
        {
            return default(object);
        }

        [Production("extension : OPEN_EXT transition_chain + CLOSE_EXT")]
        public object extension_OPENEXT_transitionchain_CLOSEEXT(Token<CLIToken> p0, List<object> p1, Token<CLIToken> p2)
        {
            return default(object);
        }

        [Production("transition_chain : (LEFTPAREN ID RIGHTPAREN)? transition + (ARROW ENDTOKEN)?")]
        public object transitionchain_LEFTPAREN_ID_RIGHTPAREN_transition_ARROW_ENDTOKEN_(ValueOption<Group<CLIToken, object>> p0, List<object> p1, ValueOption<Group<CLIToken, object>> p2)
        {
            return default(object);
        }

        [Production("transition : ARROW (LEFTPAREN ID RIGHTPAREN)? pattern repeater? (AT ID)?")]
        public object transition_ARROW_LEFTPAREN_ID_RIGHTPAREN_pattern_repeater_AT_ID_(Token<CLIToken> p0, ValueOption<Group<CLIToken, object>> p1, object p2, ValueOption<object> p3, ValueOption<Group<CLIToken, object>> p4)
        {
            return default(object);
        }

        [Production("repeater : ZEROORMORE")]
        public object repeater_ZEROORMORE(Token<CLIToken> p0)
        {
            return default(object);
        }

        [Production("repeater : ONEORMORE")]
        public object repeater_ONEORMORE(Token<CLIToken> p0)
        {
            return default(object);
        }

        [Production("repeater : LEFTCURL INT RIGHTCURL")]
        public object repeater_LEFTCURL_INT_RIGHTCURL(Token<CLIToken> p0, Token<CLIToken> p1, Token<CLIToken> p2)
        {
            return default(object);
        }

        [Production("pattern : CHAR")]
        public object pattern_CHAR(Token<CLIToken> p0)
        {
            return default(object);
        }

        [Production("pattern : LEFTBRACKET range (COMMA range) * RIGHTBRACKET")]
        public object pattern_LEFTBRACKET_range_COMMA_range_RIGHTBRACKET(Token<CLIToken> p0, object p1, List<Group<CLIToken, object>> p2, Token<CLIToken> p3)
        {
            return default(object);
        }

        [Production("range : CHAR DASH CHAR")]
        public object range_CHAR_DASH_CHAR(Token<CLIToken> p0, Token<CLIToken> p1, Token<CLIToken> p2)
        {
            return default(object);
        }

        [Production("lexer_option : LEFTBRACKET [ IGNOREKEYWORDCASING | INDENTATIONAWARE | IGNOREWHITESPACES | IGNOREEOL ] LEFTPAREN [ TRUE | FALSE ] RIGHTPAREN RIGHTBRACKET")]
        public object lexeroption_LEFTBRACKET_IGNOREKEYWORDCASING_INDENTATIONAWARE_IGNOREWHITESPACES_IGNOREEOL_LEFTPAREN_TRUE_FALSE_RIGHTPAREN_RIGHTBRACKET(Token<CLIToken> p0, Token<CLIToken> p1, Token<CLIToken> p2, Token<CLIToken> p3, Token<CLIToken> p4, Token<CLIToken> p5)
        {
            return default(object);
        }

        [Production("IdentifierOrString : [ ID | GENERICLEXER | STRINGTOKEN | PARSER | CHARTOKEN | INTTOKEN | DATETOKEN | DOUBLETOKEN | HEXATOKEN | ALPHAIDTOKEN | ALPHANUMIDTOKEN | ALPHANUMDASHIDTOKEN | KEYWORDTOKEN | SUGARTOKEN | SINGLELINECOMMENT | UPTOTOKEN | MULTILINECOMMENT | EXTENSIONTOKEN | PUSH | MODE | POP | TRUE | FALSE | INDENT | UINDENT | YYYYMMDD | DDMMYYYY | STRING ]")]
        public object IdentifierOrString_ID_GENERICLEXER_STRINGTOKEN_PARSER_CHARTOKEN_INTTOKEN_DATETOKEN_DOUBLETOKEN_HEXATOKEN_ALPHAIDTOKEN_ALPHANUMIDTOKEN_ALPHANUMDASHIDTOKEN_KEYWORDTOKEN_SUGARTOKEN_SINGLELINECOMMENT_UPTOTOKEN_MULTILINECOMMENT_EXTENSIONTOKEN_PUSH_MODE_POP_TRUE_FALSE_INDENT_UINDENT_YYYYMMDD_DDMMYYYY_STRING_(Token<CLIToken> p0)
        {
            return default(object);
        }

        [Production("operand : LEFTBRACKET OPERAND RIGHTBRACKET")]
        public object operand_LEFTBRACKET_OPERAND_RIGHTBRACKET(Token<CLIToken> p0, Token<CLIToken> p1, Token<CLIToken> p2)
        {
            return default(object);
        }

        [Production("rule : attribute * ARROW? operand? IdentifierOrString COLON clause + SEMICOLON")]
        public object rule_attribute_ARROW_operand_IdentifierOrString_COLON_clause_SEMICOLON(List<object> p0, Token<CLIToken> p1, ValueOption<object> p2, object p3, Token<CLIToken> p4, List<object> p5, Token<CLIToken> p6)
        {
            return default(object);
        }

        [Production("rule : attribute * operand? IdentifierOrString + SEMICOLON")]
        public object rule_attribute_operand_IdentifierOrString_SEMICOLON(List<object> p0, ValueOption<object> p1, List<object> p2, Token<CLIToken> p3)
        {
            return default(object);
        }

        [Production("rule : attribute * LEFTBRACKET PREFIX INT RIGHTBRACKET IdentifierOrString * SEMICOLON")]
        public object rule_attribute_LEFTBRACKET_PREFIX_INT_RIGHTBRACKET_IdentifierOrString_SEMICOLON(List<object> p0, Token<CLIToken> p1, Token<CLIToken> p2, Token<CLIToken> p3, Token<CLIToken> p4, List<object> p5, Token<CLIToken> p6)
        {
            return default(object);
        }

        [Production("rule : attribute * LEFTBRACKET POSTFIX INT RIGHTBRACKET IdentifierOrString * SEMICOLON")]
        public object rule_attribute_LEFTBRACKET_POSTFIX_INT_RIGHTBRACKET_IdentifierOrString_SEMICOLON(List<object> p0, Token<CLIToken> p1, Token<CLIToken> p2, Token<CLIToken> p3, Token<CLIToken> p4, List<object> p5, Token<CLIToken> p6)
        {
            return default(object);
        }

        [Production("rule : attribute * LEFTBRACKET [ RIGHT | LEFT ] INT RIGHTBRACKET IdentifierOrString + SEMICOLON")]
        public object rule_attribute_LEFTBRACKET_RIGHT_LEFT_INT_RIGHTBRACKET_IdentifierOrString_SEMICOLON(List<object> p0, Token<CLIToken> p1, Token<CLIToken> p2, Token<CLIToken> p3, Token<CLIToken> p4, List<object> p5, Token<CLIToken> p6)
        {
            return default(object);
        }

        [Production("item : IdentifierOrString")]
        public object item_IdentifierOrString(object p0)
        {
            return default(object);
        }

        [Production("item : choiceclause")]
        public object item_choiceclause(object p0)
        {
            return default(object);
        }

        [Production("clause : item ZEROORMORE")]
        public object clause_item_ZEROORMORE(object p0, Token<CLIToken> p1)
        {
            return default(object);
        }

        [Production("clause : item ONEORMORE")]
        public object clause_item_ONEORMORE(object p0, Token<CLIToken> p1)
        {
            return default(object);
        }

        [Production("clause : item LEFTCURL INT RIGHTCURL")]
        public object clause_item_LEFTCURL_INT_RIGHTCURL(object p0, Token<CLIToken> p1, Token<CLIToken> p2, Token<CLIToken> p3)
        {
            return default(object);
        }

        [Production("clause : item LEFTCURL INT DASH INT RIGHTCURL")]
        public object clause_item_LEFTCURL_INT_DASH_INT_RIGHTCURL(object p0, Token<CLIToken> p1, Token<CLIToken> p2, Token<CLIToken> p3, Token<CLIToken> p4, Token<CLIToken> p5)
        {
            return default(object);
        }

        [Production("clause : item OPTION")]
        public object clause_item_OPTION(object p0, Token<CLIToken> p1)
        {
            return default(object);
        }

        [Production("clause : discardeditem")]
        public object clause_discardeditem(object p0)
        {
            return default(object);
        }

        [Production("clause : item")]
        public object clause_item(object p0)
        {
            return default(object);
        }

        [Production("clause : choiceclause")]
        public object clause_choiceclause(object p0)
        {
            return default(object);
        }

        [Production("choiceclause : LEFTBRACKET item (OR item) * RIGHTBRACKET")]
        public object choiceclause_LEFTBRACKET_item_OR_item_RIGHTBRACKET(Token<CLIToken> p0, object p1, List<Group<CLIToken, object>> p2, Token<CLIToken> p3)
        {
            return default(object);
        }

        [Production("clause : choiceclause ONEORMORE")]
        public object clause_choiceclause_ONEORMORE(object p0, Token<CLIToken> p1)
        {
            return default(object);
        }

        [Production("clause : choiceclause ZEROORMORE")]
        public object clause_choiceclause_ZEROORMORE(object p0, Token<CLIToken> p1)
        {
            return default(object);
        }

        [Production("clause : choiceclause LEFTCURL INT RIGHTCURL")]
        public object clause_choiceclause_LEFTCURL_INT_RIGHTCURL(object p0, Token<CLIToken> p1, Token<CLIToken> p2, Token<CLIToken> p3)
        {
            return default(object);
        }

        [Production("clause : choiceclause LEFTCURL INT DASH INT RIGHTCURL")]
        public object clause_choiceclause_LEFTCURL_INT_DASH_INT_RIGHTCURL(object p0, Token<CLIToken> p1, Token<CLIToken> p2, Token<CLIToken> p3, Token<CLIToken> p4, Token<CLIToken> p5)
        {
            return default(object);
        }

        [Production("clause : choiceclause OPTION")]
        public object clause_choiceclause_OPTION(object p0, Token<CLIToken> p1)
        {
            return default(object);
        }

        [Production("clause : group")]
        public object clause_group(object p0)
        {
            return default(object);
        }

        [Production("group : LEFTPAREN discardeditem * RIGHTPAREN")]
        public object group_LEFTPAREN_discardeditem_RIGHTPAREN(Token<CLIToken> p0, List<object> p1, Token<CLIToken> p2)
        {
            return default(object);
        }

        [Production("discardeditem : item DISCARD?")]
        public object discardeditem_item_DISCARD_(object p0, Token<CLIToken> p1)
        {
            return default(object);
        }

        [Production("clause : group ONEORMORE")]
        public object clause_group_ONEORMORE(object p0, Token<CLIToken> p1)
        {
            return default(object);
        }

        [Production("clause : group ZEROORMORE")]
        public object clause_group_ZEROORMORE(object p0, Token<CLIToken> p1)
        {
            return default(object);
        }

        [Production("clause : group LEFTCURL INT RIGHTCURL")]
        public object clause_group_LEFTCURL_INT_RIGHTCURL(object p0, Token<CLIToken> p1, Token<CLIToken> p2, Token<CLIToken> p3)
        {
            return default(object);
        }

        [Production("clause : group LEFTCURL INT DASH INT RIGHTCURL")]
        public object clause_group_LEFTCURL_INT_DASH_INT_RIGHTCURL(object p0, Token<CLIToken> p1, Token<CLIToken> p2, Token<CLIToken> p3, Token<CLIToken> p4, Token<CLIToken> p5)
        {
            return default(object);
        }

        [Production("clause : group OPTION")]
        public object clause_group_OPTION(object p0, Token<CLIToken> p1)
        {
            return default(object);
        }

        [Production("attribute : AT ID LEFTPAREN [ ID | STRING ] (COMMA [ ID | STRING ]) * RIGHTPAREN SEMICOLON")]
        public object attribute_AT_ID_LEFTPAREN_ID_STRING_COMMA_ID_STRING_RIGHTPAREN_SEMICOLON(Token<CLIToken> p0, Token<CLIToken> p1, Token<CLIToken> p2, Token<CLIToken> p3, List<Group<CLIToken, object>> p4, Token<CLIToken> p5, Token<CLIToken> p6)
        {
            return default(object);
        }
    }
}