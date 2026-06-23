using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace NetworkService.Services
{
    public class LoggerService
    {
        private readonly string _logFilePath;

        public LoggerService(string logFilePath) 
        { 
            _logFilePath = logFilePath; 
            if(File.Exists(_logFilePath))
            {
                File.Delete(_logFilePath);
            }
        }

        public void LogLine(string message)
        {
            try
            {
                using (var writer = new StreamWriter(_logFilePath, true))
                {
                    writer.WriteLine(message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to log message: {ex.Message}");
            }
        }
    }
}
