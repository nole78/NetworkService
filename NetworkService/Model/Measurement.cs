using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkService.Model
{
    public class Measurement
    {
        public string Time { get; set; }
        public double Value { get; set; }
        public Measurement() { }
        public Measurement(string time, double value)
        {
            Time = time;
            Value = value;
        }
    }
}
