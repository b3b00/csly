using System;

namespace sly
{
    public class EnumConverter
    {
        public static IN ConvertIntToEnum<IN>(int intValue)
        {
            var genericType = typeof(IN);
            if (genericType.IsEnum)
                foreach (IN value in Enum.GetValues(genericType))
                {
                    var test = Enum.Parse(typeof(IN), value.ToString()) as Enum;
                    var val = Convert.ToInt32(test);
                    if (val == intValue)
                    {
                        return value;
                    }
                }

            return default(IN);
        }
    }
}