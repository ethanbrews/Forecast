using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Forecast2Uwp.Play
{
    class ProfileNameToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language) => (value as string) == "Vanilla" ? Visibility.Collapsed : Visibility.Visible;
        public object ConvertBack(object value, Type targetType, object parameter, string language) => null;
    }
}
