using System.Collections.Generic;
using NFluent;
using sly.buildresult;
using sly.lexer;
using Xunit;

namespace ParserTests.issue457;

public class Issue457Test
{
    [Fact]
    public void TestIssue457SingleLineCommentwithNotContent()
    {
        BuildResult<ILexer<Issue457Lexer>> _lexer = LexerBuilder.BuildLexer<Issue457Lexer>();
        Check.That(_lexer).IsOk();
        string source = @"
1
0xABC
#
""""""
multi
line
comment
""""""
""double quoted string""
'single quoted string'
";
        var result = _lexer.Result.Tokenize(source);
        Check.That(result).IsOkLexing();
        var tokens = result.Tokens;
        var mainTokens = tokens.GetChannel(Channels.Main); 
        Check.That(mainTokens.Count).IsEqualTo(7); //(5 non comment tokens and 2 null comment tokens)
        Check.That(mainTokens[2]).IsNull();
        Check.That(mainTokens[3]).IsNull();
        var doubleQuotedString = mainTokens[4];
        Check.That(doubleQuotedString.TokenID).IsEqualTo(Issue457Lexer.STRING);
        Check.That(doubleQuotedString.StringWithoutQuotes).IsEqualTo("double quoted string");

        Check.That(tokens.GetChannels()).CountIs(2);
        Check.That(tokens.GetChannel(Channels.Comments)).IsNotNull();
        var commentTokens = tokens.GetChannel(Channels.Comments);
        Check.That(commentTokens.Count).IsEqualTo(4); // 2 leading null tokens (1 and 0xABC)
        var singleLineComment = commentTokens[2];
        Check.That(singleLineComment).IsNotNull();
        Check.That(singleLineComment.TokenID).IsEqualTo(Issue457Lexer.COMMENT);
        Check.That(singleLineComment.Value).IsEmpty();
    }
    
    [Fact]
    public void TestIssue457EmptyLineWithIndents()
    {
        BuildResult<ILexer<Issue457Lexer>> _lexer = LexerBuilder.BuildLexer<Issue457Lexer>();
        Check.That(_lexer).IsOk();
        string source = @"
label recap_emily_questions:
	hide screen phone_icon
	scene black
	if _return == ""Forgive her"":
		# Set variables here 

		scene recap02_01 # v5 s411a
		menu:

			""Stay friends"":
				# Set variables for this decision here 
				# Emily Friend here 
			
			""Start having sex again"":
				# Set variables for this decision here
				# Emily RS here
				$ CharacterService.set_relationship(emily, Relationship.FWB)

";
        var result = _lexer.Result.Tokenize(source);
        Check.That(result).IsOkLexing();
    }

    [Fact]
    public void TestUnderscoreIdentifierConflict()
    {
	    BuildResult<ILexer<Issue457ConflictingLexer>> _lexer = LexerBuilder.BuildLexer<Issue457ConflictingLexer>();
	    Check.That(_lexer).Not.IsOk();
	    Check.That(_lexer.Errors).CountIs(1);
	    var error = _lexer.Errors[0];
	    Check.That(error.Code).IsEqualTo(ErrorCodes.LEXER_SUGAR_TOKEN_CANNOT_START_WITH_LETTER);
    }
}