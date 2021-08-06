using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Forecast2Uwp.Play
{
    class PackageListLimiterConverter : IValueConverter
    {

        private readonly int size = 14;

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var packages = value as IList<Thunderstore.Data.Package>;

            if (packages.Count() == 0)
                return new Thunderstore.Data.Package[0];

            var total = new Thunderstore.Data.Package[size];
            for (var i = 0; i < size; i++)
            {
                total[i] = packages[i % packages.Count()];
            }

            return total;
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language) => null;
    }
}
