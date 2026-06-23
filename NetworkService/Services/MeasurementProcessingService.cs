using NetworkService.Model;
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
                DistributedEnergyResource resource = null;

                if (AppDatabase.Instance.Resources.Count > idx)
                {
                    resource = AppDatabase.Instance.Resources[idx];
                }

                if (resource == null)
                {
                    Console.WriteLine($"Error updating resource value: no resource at index {idx}");
                    return;
                }

                Application.Current.Dispatcher.Invoke(() => {
                    
                    AppDatabase.Instance.SetValue(resource.Id, value);
                });

                var date = DateTime.Now;
                string logLine = $"{date.ToShortDateString()}, {date:HH:mm}: {resource.Id}, {value}";
                _loggerService.LogLine(logLine);

                OnMeasurementProcessed?.Invoke(resource.Id, value);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error processing measurment: {ex.Message}");
            }
        }
    }
}
