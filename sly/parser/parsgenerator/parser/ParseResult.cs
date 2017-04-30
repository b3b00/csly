using System;
using System.Collections.Generic;
using System.Text;

namespace cpg.parser.parsgenerator.parser
{
    public class ParseResult<T>
    {

        public object Result { get; set; }

        public bool IsError { get; set; }

        public List<ParseError> Errors { get; set; }

        public int EndingPosition { get; set; }

        public bool IsEnded { get; set; }
    }
}
