using System.Collections.Generic;

namespace benchgen.jsonparser.JsonModel
{
    public class JObject : JSon
    {
        public JObject(string key, JSon value)
        {
            Values = new Dictionary<string, JSon>();
            Values[key] = value;
        }

        public JObject()
        {
            Values = new Dictionary<string, JSon>();
        }

        public JObject(Dictionary<string, JSon> dic)
        {
            Values = dic;
        }

        public override bool IsObject => true;

        public override bool IsList => true;
        public override string ToJson()
        {
            Func<KeyValuePair<string,JSon>, string> tojson = x => x.Key.ToString()+":"+x.Value.ToJson(); 
            return $"{{{string.Join(",", Values.Select(i => tojson(i)))}}}";
        }

        private Dictionary<string, JSon> Values { get; }

        public int Count => Values.Count;

        public JSon this[string key]
        {
            get => Values[key];
            set => Values[key] = value;
        }


        public void Merge(JObject obj)
        {
            foreach (var pair in obj.Values) this[pair.Key] = pair.Value;
        }

        public bool ContainsKey(string key)
        {
            return Values.ContainsKey(key);
        }
    }
}