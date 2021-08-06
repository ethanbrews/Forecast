using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Forecast2Uwp.Converter
{
    class ToUppercaseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language) => ((string)value).ToUpper();
        public object ConvertBack(object value, Type targetType, object parameter, string language) => ((string)value).ToLower();
    }
}
