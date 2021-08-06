using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Forecast2Uwp.Converter
{
    class EmptyListToDisabled : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language) =>
            (value as IEnumerable<object>).Count() == 0 ? false : true;

        public object ConvertBack(object value, Type targetType, object parameter, string language) => (bool)value ? 1 : 0;
    }
}
