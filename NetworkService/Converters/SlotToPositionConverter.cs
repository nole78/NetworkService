using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace NetworkService.Converters
{
    public class SlotToPositionConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2 && values[0] is int idx && values[1] is Canvas canvas)
            {
                int totalCols = 4;
                int totalRows = 3;

                int row = idx / totalCols;
                int col = idx % totalCols;

                double slotWidth = canvas.ActualWidth / totalCols;
                double slotHeight = canvas.ActualHeight / totalRows;

                var axis = parameter as string;

                if (axis == "X")
                {
                    return (col * slotWidth) + (slotWidth / 2);
                }
                else if(axis == "Y")
                {
                    return (row * slotHeight) + (slotHeight / 2);
                }
            }
            return 0.0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
