using NetworkService.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace NetworkService.Services
{
    public class MeasurmentsReader
    {
        private readonly string _measurmentsfilePath;

        public MeasurmentsReader(string measurmentsfilePath)
        {
            _measurmentsfilePath = measurmentsfilePath;
        }

        public List<Measurement> ReadMeasurments(int idx) 
        {
            Queue<Measurement> buffer = new Queue<Measurement>(5);
            try
            {
                using (var sr = new StreamReader(_measurmentsfilePath,true))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        var parts = line.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        if (parts.Length != 4)
                            continue;

                        if (!int.TryParse(parts[2], out int objectIdx))
                            continue;

                        if (objectIdx != idx)
                            continue;

                        if (!double.TryParse(parts[3], out double value))
                            continue;

                        string time = parts[1];

                        buffer.Enqueue(new Measurement(time, value));
                        if(buffer.Count > 4)
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
