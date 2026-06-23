using NetworkService.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace NetworkService.Services
{
    public class MeasurmentsReader
    {
        private readonly string _measurmentsfilePath;
        private const int MEASURMENT_WINDOW = 5;

        public MeasurmentsReader(string measurmentsfilePath)
        {
            _measurmentsfilePath = measurmentsfilePath;
        }

        public List<Measurement> ReadMeasurments(int id) 
        {
            Queue<Measurement> buffer = new Queue<Measurement>(MEASURMENT_WINDOW);
            string pattern = @",\s*(?<time>\d{1,2}:\d{2}):\s*(?<id>\d+),\s*(?<value>[\d.,]+)";
            Regex regex = new Regex(pattern);
            try
            {
                using (var sr = new StreamReader(_measurmentsfilePath,true))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        Match match = regex.Match(line);

                        if (!match.Success)
                            continue;

                        int objectIdx = int.Parse(match.Groups["id"].Value);
                        if (objectIdx != id)
                            continue;

                        string time = match.Groups["time"].Value;

                        string valueStr = match.Groups["value"].Value.Replace(',', '.');
                        double value = double.Parse(valueStr, System.Globalization.CultureInfo.InvariantCulture);

                        buffer.Enqueue(new Measurement(time, value));
                        if (buffer.Count > MEASURMENT_WINDOW) 
                        {
                            buffer.Dequeue();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to read measurment: {ex.Message}");
                return new List<Measurement>();
            }

            return buffer.ToList();
        }
    }
}
