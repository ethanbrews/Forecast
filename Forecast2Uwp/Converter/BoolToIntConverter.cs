using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Forecast2Uwp.Converter
{
    class BoolToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language) => (bool)value ? 1 : 0;
        public object ConvertBack(object value, Type targetType, object parameter, string language) => (int)value == 1;
    }
}
