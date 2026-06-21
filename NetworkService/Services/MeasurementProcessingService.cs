using NetworkService.Persistance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NetworkService.Services
{
    public class MeasurementProcessingService
    {
        private readonly LoggerService _loggerService;

        public static event Action<int, double> OnMeasurementProcessed;

        public MeasurementProcessingService(LoggerService loggerService)
        {
            _loggerService = loggerService;
        }

        public void ProcessMeasurement(string incoming)
        {
            try
            {
                var parts = incoming.Trim().Split(':');
                int idx = int.Parse(parts[0].Split('_')[1]);
                double value = double.Parse(parts[1]);

                Application.Current.Dispatcher.Invoke(() => {
                    if (AppDatabase.Resources.Count > idx)
                    {
                        var resource = AppDatabase.Resources[idx];
                        if (resource != null)
                        {
                            AppDatabase.SetValue(resource.Id, value);
                        }
                        else
                        {
                            Console.WriteLine($"Error updating resource value: no resource at index {idx}");
                        }
                    }
                });

                var date = DateTime.Now;
                string logLine = $"{date.ToShortDateString()}, {date.ToString("HH:mm")}: {idx}, {value}";
                _loggerService.LogLine(logLine);

                OnMeasurementProcessed?.Invoke(idx, value);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error processing measurment: {ex.Message}");
            }
        }
    }
}
