using System;
using System.Collections.Generic;
using System.Text;

namespace sly.lexer
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true,Inherited =true)]
    public class LexemeAttribute : Attribute
    {

        public GenericToken GenericToken { get; set; }

        public string[] GenericTokenParameters { get; set; }

        public IdentifierType IdentifierType { get; set; } = IdentifierType.Alpha;

        public string Pattern { get; set; }

        public bool IsSkippable { get; set; }

        public bool IsLineEnding { get; set; }


        public bool IsStaticGeneric => ( GenericTokenParameters == null || GenericTokenParameters.Length == 0) && GenericToken != GenericToken.String && GenericToken != GenericToken.Extension;

        public bool IsKeyWord => GenericToken == GenericToken.KeyWord;

        public bool IsSugar => GenericToken == GenericToken.SugarToken;

        public bool IsString => GenericToken == GenericToken.String;

        public bool IsIdentifier => GenericToken == GenericToken.Identifier;

        public bool IsExtension => GenericToken == GenericToken.Extension;

        public LexemeAttribute(string pattern, bool isSkippable = false, bool isLineEnding = false)        {
            Pattern = pattern;
            IsSkippable = isSkippable;
            IsLineEnding = isLineEnding;
        }

        public LexemeAttribute(GenericToken generic, params string[] parameters)
        {
            GenericToken = generic;
            GenericTokenParameters = parameters;
        }

        public LexemeAttribute(GenericToken generic, IdentifierType idType)
        {
            GenericToken = generic;
            IdentifierType = idType;
        }
    }
}
