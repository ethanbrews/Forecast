using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForecastUWP
{
    class Constants
    {
        public static Uri ThunderstoreApiEndpoint => new Uri("https://thunderstore.io/api/v1/");
        public static Uri ThunderstorePackagesApiEndpoint = new Uri(ThunderstoreApiEndpoint, "package");

        public static Uri ForecastProfileStoreApiEndpoint = new Uri("https://forecast.ethanbrews.me/api/v1/");
        public static Uri ForecastProfileStoreFindApiEndpoint = new Uri(ForecastProfileStoreApiEndpoint, "find");
        public static Uri ForecastProfileStoreSubmitApiEndpoint = new Uri(ForecastProfileStoreApiEndpoint, "submit");
    }
}
