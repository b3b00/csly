using System;
using NFluent;
using sly.lexer.fsm;
using Xunit;

namespace ParserTests
{
    public class EOLTests
    {
        [Fact]
        public void TestGetToEndOfLine()
        {
            var source = "test\r\nretest";
            ReadOnlyMemory<char> memory = new ReadOnlyMemory<char>(source.ToCharArray());
            var end = memory.GetToEndOfLine(0);
            Check.That(end.ToString()).IsEqualTo("test\r\n");
        }

        [Fact]
        public void TestRemoveEOLChars()
        {
            var source = "test\r\n\n";
            ReadOnlyMemory<char> memory = new ReadOnlyMemory<char>(source.ToCharArray());
            var end = memory.RemoveEndOfLineChars();
            Check.That(end.ToString()).IsEqualTo("test");
        }
        
        [Fact]
        public void TestRemoveEOLCharsFromEmptyLine()
        {
            var source = "\r\n";
            ReadOnlyMemory<char> memory = new ReadOnlyMemory<char>(source.ToCharArray());
            var end = memory.RemoveEndOfLineChars();
            Check.That(end.ToString()).IsEmpty();
        }
    }
}