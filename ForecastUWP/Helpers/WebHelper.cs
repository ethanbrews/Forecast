using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace ForecastUWP.Helpers
{
    class WebHelper
    {
        public static string GenerateNameFromUrl(string url)
        {
            return url.Replace("://", "_").Replace(".", "_").Replace("/", "_");
        }

        public static string AddHTMLBoilerplate(string htmlContent)
        {
            return
                $"<html><head><link href=\"ms-appx-web:///Assets/Markdown.css\" rel=\"stylesheet\" /><style>html {{color: #{(App.Current.RequestedTheme == ApplicationTheme.Light ? "000" : "fff")}}}</style></head><body>{htmlContent}</body></html>";
        }
    }
}
