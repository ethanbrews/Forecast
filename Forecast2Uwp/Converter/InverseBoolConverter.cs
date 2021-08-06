using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Forecast2Uwp.Converter
{
    class InverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language) => !(bool)value;

        public object ConvertBack(object value, Type targetType, object parameter, string language) =>
            Convert(value, targetType, parameter, language);
    }
}
