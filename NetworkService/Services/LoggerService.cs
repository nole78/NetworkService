using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkService.Services
{
    public class LoggerService
    {
        private readonly string _logFilePath;

        public LoggerService(string logFilePath) 
        { 
            _logFilePath = logFilePath; 
        }

        public void LogMeasurment(string measurment)
        {
            try
            {
                using (var writer = new StreamWriter(_logFilePath, true))
                {
                    writer.WriteLine($"{DateTime.Now} [MEASURMENT]: {measurment}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to log message: {ex.Message}");
            }
        }
    }
}
