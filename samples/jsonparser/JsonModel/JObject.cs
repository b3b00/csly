using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;


namespace jsonparser.JsonModel
{
    public class JObject : JSon
    {
        public override bool IsObject => true;
        
        public override bool IsList => true;

        private Dictionary<string,JSon> values;

        private Dictionary<string, JSon> Values => values;

        public int Count => values.Count;
        
        public JSon this[string key]
        {
            get
            {
                return values[key];
            }
            set
            {
                values[key] = value;
            }
        }

        public JObject(string key, JSon value)
        {
            values = new Dictionary<string, JSon>();
            values[key] = value;
        }

        public JObject()
        {
            values = new Dictionary<string, JSon>();
        }
        
        public JObject(Dictionary<string, JSon> dic)
        {
            values = dic;
        }

        
        
        public void Merge(JObject obj)
        {
            foreach (KeyValuePair<String, JSon> pair in obj.Values)
            {
                this[pair.Key] = pair.Value;
            } 
        }

        public bool ContainsKey(string key) => values.ContainsKey(key);
    }
}