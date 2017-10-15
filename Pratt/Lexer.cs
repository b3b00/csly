

import java.util.HashMap;
import java.util.Iterator;
import java.util.Map;

/**
 * A very primitive lexer. Takes a string and splits it into a series of
 * Tokens. Operators and punctuation are mapped to unique keywords. Names,
 * which can be any series of letters, are turned into NAME tokens. All other
 * characters are ignored (except to separate names). Numbers and strings are
 * not supported. This is really just the bare minimum to give the parser
 * something to work with.
 */

namespace com.stuffwithstuff.bantam
{
    public class Lexer : IEnumerable<Token> {
        /**
         * Creates a new Lexer to tokenize the given string.
         * @param text String to tokenize.
         */
        public Lexer(String text) {
            mIndex = 0;
            mText = text;

            // Register all of the TokenTypes that are explicit punctuators.
            foreach (TokenType type in TokenType.values()) {
                Character punctuator = type.punctuator();
                if (punctuator != null) {
                    mPunctuators.put(punctuator, type);
                }
            }
        }

        
  public boolean hasNext() {
            return true;
        }

        
  public Token next() {
            while (mIndex < mText.length()) {
                char c = mText.charAt(mIndex++);

                if (mPunctuators.containsKey(c)) {
                    // Handle punctuation.
                    return new Token(mPunctuators.get(c), Character.toString(c));
                } else if (Character.isLetter(c)) {
                    // Handle names.
                    int start = mIndex - 1;
                    while (mIndex < mText.length()) {
                        if (!Character.isLetter(mText.charAt(mIndex))) break;
                        mIndex++;
                    }

                    String name = mText.substring(start, mIndex);
                    return new Token(TokenType.NAME, name);
                } else {
                    // Ignore all other characters (whitespace, etc.)
                }
            }

            // Once we've reached the end of the string, just return EOF tokens. We'll
            // just keeping returning them as many times as we're asked so that the
            // parser's lookahead doesn't have to worry about running out of tokens.
            return new Token(TokenType.EOF, "");
        }

  public void remove() {
            throw new UnsupportedOperationException();
        }

        private final Map<Character, TokenType> mPunctuators =
            new HashMap<Character, TokenType>();
        private final String mText;
  private int mIndex = 0;
    }

}
