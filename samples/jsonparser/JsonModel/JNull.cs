using System;

namespace jsonparser.JsonModel
{
    public class JNull : JSon
    {
        public override Boolean IsNull => true;
    }
}