using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Forecast2Uwp.Converter
{
    class TenItemLimiterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            int count;
            if (Int32.TryParse((string)parameter, out count))
            {
                return ((IEnumerable<object>)value).Take(count);
            }
            return ((IEnumerable<object>)value).Take(10);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
