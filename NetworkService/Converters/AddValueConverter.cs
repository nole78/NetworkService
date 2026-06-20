using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace NetworkService.Converters
{
    public class AddValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is double currentPosition && parameter != null)
            {
                if(double.TryParse(parameter.ToString(),out double offset))
                {
                    return currentPosition + offset;
                }
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double currentPosition && parameter != null)
            {
                if (double.TryParse(parameter.ToString(), out double offset))
                {
                    return currentPosition - offset;
                }
            }

            return value;
        }
    }
}
