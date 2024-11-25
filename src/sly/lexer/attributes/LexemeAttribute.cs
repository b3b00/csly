using System;

namespace sly.lexer
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class LexemeAttribute : Attribute
    {
        
        internal bool IsPop { get; set; }
        
        internal bool IsPush { get; set; }
        
        internal string Pushtarget { get; set; }
        
        internal string Mode { get; set; }
        
        public LexemeAttribute(string pattern, bool isSkippable = false, bool isLineEnding = false)
        {
            Pattern = pattern;
            IsSkippable = isSkippable;
            IsLineEnding = isLineEnding;
        }

        public LexemeAttribute(GenericToken generic, params string[] parameters)
        {
            GenericToken = generic;
            GenericTokenParameters = parameters;
        }
        public LexemeAttribute(GenericToken generic, int channel = 0, params string[] parameters)
        {
            Channel = channel;
            GenericToken = generic;
            GenericTokenParameters = parameters;
        }

        public LexemeAttribute(GenericToken generic, IdentifierType idType, string startPattern = null, string restPattern = null, int channel = 0)
        {
            Channel = channel;
            GenericToken = generic;
            IdentifierType = idType;
            if (idType == IdentifierType.Custom)
            {
                IdentifierStartPattern = startPattern ?? throw new ArgumentNullException(nameof(startPattern));
                IdentifierRestPattern = restPattern ?? startPattern;
            }
        }

        public GenericToken GenericToken { get; set; }

        public string[] GenericTokenParameters { get; set; }

        public IdentifierType IdentifierType { get; set; } = IdentifierType.Alpha;
        
        public string IdentifierStartPattern { get; }
        
        public string IdentifierRestPattern { get; }

        public string Pattern { get; set; }

        public bool IsSkippable { get; set; }

        public bool IsLineEnding { get; set; }

        private int _channel = int.MinValue;
        public int Channel { get => _channel != int.MinValue ? _channel : 0; set => _channel = value; }


        public bool HasGenericTokenParameters => GenericTokenParameters != null && GenericTokenParameters.Length > 0;
        
        public bool IsStaticGeneric => !HasGenericTokenParameters &&
                                       GenericToken != GenericToken.String && GenericToken != GenericToken.Extension && GenericToken != GenericToken.Double;

        public bool IsKeyWord => GenericToken == GenericToken.KeyWord;

        public bool IsSugar => GenericToken == GenericToken.SugarToken;

        public bool IsString => GenericToken == GenericToken.String;
        
        public bool IsChar => GenericToken == GenericToken.Char;

        public bool IsIdentifier => GenericToken == GenericToken.Identifier;

        public bool IsExtension => GenericToken == GenericToken.Extension;
        
        public bool IsDouble => GenericToken == GenericToken.Double;
        public bool IsUpTo => GenericToken == GenericToken.UpTo;

        public bool IsHexa => GenericToken == GenericToken.Hexa;
        
        public bool IsDate => GenericToken == GenericToken.Date;
    }
}