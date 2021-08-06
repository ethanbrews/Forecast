using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Forecast2Uwp.Play
{
    class PackageToExImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language) =>
            ((Thunderstore.Data.Package)value).Versions[0].Icon;
        public object ConvertBack(object value, Type targetType, object parameter, string language) => null;
    }
}
