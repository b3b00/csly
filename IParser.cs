using System;
using System.Collections.Generic;

namespace parser.parsergenerator.parser
{

    public interface IParser<T>
    {

        object Parse(IList<T> tokens);

    }