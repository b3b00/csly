using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace sly.sourceGenerator;

public class CslySyntaxWalker : CSharpSyntaxWalker
{
    protected string GetAttributeArgs(AttributeSyntax attribute, List<string> modes = null, int skip = 0, bool withLeadingComma = true)
    {

        if (attribute.ArgumentList != null && attribute.ArgumentList.Arguments.Count > 0)
        {
            var args = attribute.ArgumentList.Arguments.Skip(skip).Select(x =>
            {
                var value = x.Expression.ToString();
                if (x.NameColon != null && x.NameColon.Name.Identifier.Text != "")
                {
                    return $"{x.NameColon.Name.Identifier.Text} :{value}";
                }

                return value;
            }).ToList();
            if (args.Count > 0)
            {
                var strargs = string.Join(", ", args);
                if (modes != null && modes.Count > 0)
                {
                    strargs += ", modes:new[]{" + string.Join(", ", modes) + "}";
                }

                if (withLeadingComma)
                {
                    return ", " + strargs;
                }

                return strargs;
            }
        }

        return string.Empty;
    }

  
}