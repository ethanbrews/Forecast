using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Shell;
using Windows.Web.Http;

namespace ForecastUWP.Helpers
{
    class ThunderstoreImageLoader
    {
        public static async void LoadImagesAsync(Data.Thunderstore.Packages.Package[] packages)
        {
        }

        public static async Task<StorageFile> LoadImageAsync(Data.Thunderstore.Packages.Package package)
        {
            StorageFile responseFile;
            //var pkgName = package.Versions.FirstOrDefault()?.FullName;
            var pkgName = "MagnusMagnuson-BiggerBazaar-1.9.6";
            if (pkgName == null)
                return null;

            responseFile = await AppWebClient.GetCachedFileAsync(
                new Uri($"https://cdn.thunderstore.io/live/repository/icons/{pkgName}.png.128x128_q85.png"),
                TimeSpan.FromDays(5));
            if (responseFile != null)
                return responseFile;

            responseFile = await AppWebClient.GetCachedFileAsync(
                new Uri($"https://cdn.thunderstore.io/live/repository/icons/{pkgName}.png.128x128_q85.jpg"),
                TimeSpan.FromDays(5));

            return responseFile;
        }
    }
}
