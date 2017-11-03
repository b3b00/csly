using System;
using System.Collections.Generic;
using System.Text;

namespace csly.whileLang.compiler
{
    public class TypeConverter
    {

        public static string WhileToCSharp(WhileType whileType)
        {
            string cSharpType = "";
            switch(whileType)
            {
                case WhileType.INT:
                    {
                        cSharpType = "int";
                        break;
                    }
                case WhileType.BOOL:
                    {
                        cSharpType = "bool";
                        break;
                    }
                case WhileType.STRING:
                    {
                        cSharpType = "string";
                        break;
                    }
            }
            return cSharpType;
        }

        public static Type WhileToType(WhileType whileType)
        {
            Type type = typeof(object);
            switch (whileType)
            {
                case WhileType.INT:
                    {
                        type = typeof(int);
                        break;
                    }
                case WhileType.BOOL:
                    {
                        type = typeof(bool);
                        break;
                    }
                case WhileType.STRING:
                    {
                        type = typeof(string);
                        break;
                    }
            }
            return type;
        }

    }
}
