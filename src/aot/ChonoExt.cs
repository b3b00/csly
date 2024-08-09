using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Chrono.Ext
{
    public  class ChronoExts
    {
        public List<long> ElapsedMilliseconds { get; set; }
        
        public IDictionary<string,long> LabeledElapsedMilliseconds { get; set; }
        
        private Stopwatch chrono;
        public ChronoExts()
        {
            chrono = new Stopwatch();
            ElapsedMilliseconds = new List<long>();
            LabeledElapsedMilliseconds = new Dictionary<string, long>();
        }

        public void Start()
        {
            chrono.Start();
        }

        public void Stop(string label = null)
        {
            chrono.Stop();
            ElapsedMilliseconds.Add(chrono.ElapsedMilliseconds);
            if (!string.IsNullOrEmpty(label))
            {
                LabeledElapsedMilliseconds[label] = chrono.ElapsedMilliseconds;
            }
        }

        public void Tick(string label = null)
        {
            chrono.Stop();
            ElapsedMilliseconds.Add(chrono.ElapsedMilliseconds);
            if (!string.IsNullOrEmpty(label))
            {
                LabeledElapsedMilliseconds[label] = chrono.ElapsedMilliseconds;
            }
            chrono.Reset();
            chrono.Start();
        }
        
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            foreach (var step in LabeledElapsedMilliseconds)
            {
                builder.AppendLine($"{step.Key} : {step.Value} ms");
            }
            return builder.ToString();
        }
    }
}