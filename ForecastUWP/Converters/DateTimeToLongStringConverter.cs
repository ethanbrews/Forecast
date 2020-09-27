using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace ForecastUWP.Converters
{
    class DateTimeToLongStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language) =>
            ((DateTime) value).ToString("dddd, dd MMMM yyyy HH:mm");

        public object ConvertBack(object value, Type targetType, object parameter, string language) => null;
    }
}
