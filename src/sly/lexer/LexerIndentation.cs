using System;
using System.Collections.Generic;
using System.Linq;

namespace sly.lexer;

public class LexerIndentation
{
    private IList<string> _indentations = new List<string>() {""};
    private int _currentLevel = 0;

    public int CurrentLevel => _currentLevel;

    public string Current => _indentations.Any() && _currentLevel < _indentations.Count && _currentLevel >= 0 ? _indentations[_currentLevel] : "";

    private void DoIndent(string shift)
    {
        if (!_indentations.Contains(shift))
        {
            _indentations.Add(shift);
            _indentations = _indentations.OrderBy(x => x.Length).ToList();
        }

        _currentLevel ++;
    } 
        
    private void DoUindent()
    {
        _currentLevel = Math.Max(0,_currentLevel-1);
    }
    public (bool isIndent, LexerIndentationType type) Indent(string shift)
    {
        if (!_indentations.Any())
        {
            if (shift.Length > 0)
            {
                DoIndent(shift);
                return (true, LexerIndentationType.Indent);
            }

            return (true, LexerIndentationType.None);
        }
        else
        {
            if (IsError(shift))
            {
                return (false, LexerIndentationType.Error);
            } 
            if (shift.Length > Current.Length)
            {
                DoIndent(shift);
                return (true, LexerIndentationType.Indent);
            }
            if (shift.Length < Current.Length)
            {
                DoUindent();
                return (true, LexerIndentationType.UIndent);
            }

            if (shift.Length == Current.Length)
            {
                return (true, LexerIndentationType.None);
            }
                
        }
            
        return (true,LexerIndentationType.None);
    }

    private bool IsError(string shift)
    {
        // indent case : shift must match all previous indentations
        if (shift.Length > _indentations[_indentations.Count-1].Length)
        {
            return !_indentations.All(x => shift.StartsWith(x));
        }

        if (shift.Length == 0)
        {
            return false;
        }
        var level = _indentations.IndexOf(shift);
        return level < 0;
    }

    public LexerIndentation Clone()
    {
        return new LexerIndentation()
        {
            _currentLevel = _currentLevel,
            _indentations = _indentations.Select(x => x).ToList()
        };
    }
        
        
}