using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using sly.lexer;
using sly.parser.generator;

namespace XML
{
    [ParserRoot("document")]
    public class MinimalXmlParser2
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

        [Production("element : OPEN[d] ID attribute* SLASH[d] CLOSE[d]")]
        public string AutoElement(Token<MinimalXmlLexer2> id, List<string> attributes)
        {
            return $"autoTag({id.Value}, {string.Join(", ",attributes.Select(x => x.ToString()))})";
        }

        [Production("opentag : OPEN[d] ID attribute* CLOSE[d]")]
        public string OpenTag(Token<MinimalXmlLexer2> tagName, List<string> attributes)
        {
            return $"open ({tagName.Value}, {string.Join(", ",attributes.Select(x => x.ToString()))})";
        }

        [Production("closetag : OPEN[d] SLASH[d] ID CLOSE[d]")]
        public string CloseTag(Token<MinimalXmlLexer2> id)
        {
            return $"close({id.Value})";
        }

        [SubNodeNames(null, "elements",null)]
        [NodeName("compoundElement")]
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
        public string Comment(Token<MinimalXmlLexer2> content)
        {
            return $"comment({content.Value})";
        }

        [Production("pi : OPEN_PI[d] ID attribute* CLOSE_PI[d]")]
        public string Pi(Token<MinimalXmlLexer2> id , List<string> attributes)
        {
            return $"pi({id.Value} :: {string.Join(", ",attributes.Select(x => x.ToString()))})";
        }


        [Production("attribute: ID EQUALS[d] VALUE")]
        public string Attribute(Token<MinimalXmlLexer2> id, Token<MinimalXmlLexer2> value)
        {
            return $"{id.Value} = {value.StringWithoutQuotes}";
        }

        [Production("content : CONTENT")]
        public string Content(Token<MinimalXmlLexer2> content)
        {
            return $"text({content.Value})";
        }
        
    }
}