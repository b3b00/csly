using System.Collections.Generic;

namespace rules.model;

public class RuleComment
{
    public string Text { get; }
    public bool IsMultiLine { get; }

    public RuleComment(string text, bool isMultiLine)
    {
        Text = text;
        IsMultiLine = isMultiLine;
    }

    public override string ToString() =>
        IsMultiLine ? $"/*{Text}*/" : $"//{Text}";
}
