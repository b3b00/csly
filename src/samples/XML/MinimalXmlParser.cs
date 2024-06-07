using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using sly.lexer;
using sly.parser.generator;

namespace XML
{
    [ParserRoot("document")]
    public class MinimalXmlParser
    {

        [Production("document : misc* element misc*")]
        public string Document(List<string> startingMiscs, string root, List<string> endingMiscs)
        {
            var b = new StringBuilder();
            foreach (var startingMisc in startingMiscs)
            {
                b.AppendLine(startingMisc);
            }

            b.AppendLine(root);

            foreach (var endingMisc in endingMiscs)
            {
                b.AppendLine(endingMisc);
            }

            return b.ToString();
        }

        [Production("element : OPEN[d] ID attributes SLASH[d] CLOSE[d]")]
        public string AutoElement(Token<MinimalXmlLexer> id, string attributes)
        {
            return $"autoTag({id.Value}, {attributes})";
        }

        [Production("opentag : OPEN[d] ID attributes CLOSE[d]")]
        public string OpenTag(Token<MinimalXmlLexer> tagName, string attributes)
        {
            return $"open ({tagName.Value}, {attributes})";
        }

        [Production("closetag : OPEN[d] SLASH[d] ID CLOSE[d]")]
        public string CloseTag(Token<MinimalXmlLexer> id)
        {
            return $"close({id.Value})";
        }

        [Production("element : opentag [element|pi|comment|content]* closetag")]
        public string CompoundElement(string open, List<string> subs, string close)
        {
            StringBuilder b = new StringBuilder();
            return $@"tag({open}, {string.Join(",", subs)}, {close})";
        }

        [Production("misc : [comment | pi | content]")]
        public string Misc(string misc)
        {
            return misc;
        }

        [Production("comment : COMMENT")]
        public string Comment(Token<MinimalXmlLexer> comment)
        {
            return $"comment({comment.Value})";
        }

        [Production("pi : OPEN_PI[d] ID attributes CLOSE_PI[d]")]
        public string Pi(Token<MinimalXmlLexer> id , string attributes)
        {
            return $"pi({id.Value} :: {attributes.ToString()})";
        }

        
        [Production("attributes : attribute*")]
        public string Attributes(List<string> attributes)
        {
            return string.Join(", ",attributes.Select(x => x.ToString()));
        }

        [Production("attribute: ID EQUALS[d] VALUE")]
        public string Attribute(Token<MinimalXmlLexer> id, Token<MinimalXmlLexer> value)
        {
            return $"{id.Value} = {value.StringWithoutQuotes}";
        }

        [Production("content : CONTENT")]
        public string Content(Token<MinimalXmlLexer> content)
        {
            return $"text({content.Value})";
        }
        
    }
}