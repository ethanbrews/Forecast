using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Microsoft.AppCenter.Ingestion.Models;

namespace ForecastUWP.Converters
{
    class InverseBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language) => ((bool) value ? Visibility.Collapsed : Visibility.Visible);

        public object ConvertBack(object value, Type targetType, object parameter, string language) => ((Visibility) value) != Visibility.Visible;
    }
}
