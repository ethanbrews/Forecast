using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace ForecastUWP.Converters
{
    class PackageNameToDisplayNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var v = value as string;
            return (v.Contains("_") ? v.Replace("_", " ") : SplitCamelCase(v));
        }

        private static string SplitCamelCase(string input)
        {
            //return System.Text.RegularExpressions.Regex.Replace(input, "([A-Z])", " $1", System.Text.RegularExpressions.RegexOptions.Compiled).Trim();
            return System.Text.RegularExpressions.Regex.Replace(input, "(^[a-z]+|[A-Z]+(?![a-z])|[A-Z][a-z]+)", " $1", System.Text.RegularExpressions.RegexOptions.Compiled).Trim();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
}
