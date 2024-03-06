using System;
using sly.parser.generator;

namespace sly
{
    public static class EnumConverter
    {

        public static T ConvertIntToEnum<T>(int value) where T : struct
        {
            if (Enum.IsDefined(typeof(T), value))
            {
                return (T) Enum.ToObject(typeof(T), value);
            }
            return default;
        }


        
        public static IN ConvertStringToEnum<IN>(string name)  where IN : struct
        {
            IN token = default(IN);
            if (!Enum.TryParse(name, out token))
            {
                throw new ParserConfigurationException($"bad enum name {name} on Operation definition.");
            }
            return token;
        }
        
        public static bool IsEnumValue<IN>(string name)  where IN : struct
        {
            return Enum.TryParse(name, out IN token);
        }
    }
}