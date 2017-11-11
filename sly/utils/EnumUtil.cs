using System;
using System.Collections.Generic;
using System.Text;

namespace sly.utils
{
    public class EnumUtil
    {

        public static IEnumerable<IN> GetValues<IN>() where IN : struct
        {
            return (IN[])(Enum.GetValues(typeof(IN)));
        }

        private static Dictionary<string,IN> GetValuesByLabel<IN>() where IN : struct
        {
            var dic = new Dictionary<string, IN>();
            var values = GetValues<IN>();
            foreach(IN v in values)
            {
                dic[v.ToString()] = v;
            }
            return dic;
        }

        public static bool TryGetValue<IN>(string label,  out IN  v ) where IN: struct {
            var found = false;
            v = default(IN);
            var dict = GetValuesByLabel<IN>();
            if (found = dict.ContainsKey(label))
            {
                found = true;
                v = dict[label];
            }
            return found;
        }

    }
}
