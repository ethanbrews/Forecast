using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Forecast2Uwp.Helpers;
using Windows.Storage;
using Windows.ApplicationModel;

namespace Forecast2Uwp
{
    static class Options
    {
        public static string[] BlockedPackagesFullNames = new string[]
        {
            "ebkr-r2modman"
        };

        static Options()
        {
            foreach(var opt in ApplicationData.Current.LocalSettings.Values)
            {
                Analytics.Log($"[LocalSettingsValue] {opt.Key} = {opt.Value}");
            }
            
        }

        public static LocalSettingsValue<string> ThunderstoreApiEndpointString = new LocalSettingsValue<string>("ThunderstoreApiEndpointString", "https://thunderstore.io/api/v1/");
        public static LocalSettingsValue<string> RemoteConfigUrl = new LocalSettingsValue<string>("RemoteConfigUrl", "https://ethanbrews.me/remotecfg/forecast-KK9szTovR0GYAJ9bifqqwQ.json");

        private static LocalSettingsValue<string> RiskOfRainInstallFolderString = new LocalSettingsValue<string>("RiskOfRainInstallFolderString", null);
        public static LocalSettingsValue<int> FeatureGroup = new LocalSettingsValue<int>("RemoteConfig.FeatureGroup", new Random().Next(100));
        public static LocalSettingsValue<bool> LaunchDirectly = new LocalSettingsValue<bool>("LaunchDirectly", false);

        public static LocalSettingsValue<string> ShareEndpoint = new LocalSettingsValue<string>("ProfileShareEndpoint", "https://forecast.ethanbrews.me/api/v2/");
        //public static LocalSettingsValue<string> ShareEndpoint = new LocalSettingsValue<string>("ProfileShareEndpoint", "http://127.0.0.1:8000/api/v2/");

        public static LocalSettingsValue<bool> TrackErrors = new LocalSettingsValue<bool>("Analytics.TrackErrors", true);
        public static LocalSettingsValue<bool> TrackEvents = new LocalSettingsValue<bool>("Analytics.TrackEvents", true);
        public static LocalSettingsValue<bool> TrackCrashes = new LocalSettingsValue<bool>("Analytics.TrackCrashes", true);
        public static async Task<StorageFolder> GetRiskOfRainFolderAsync()
        {
            try
            {
                return await StorageFolder.GetFolderFromPathAsync(RiskOfRainInstallFolderString?.Value);
            } catch (Exception ex)
            {
                Analytics.TrackError(ex);
                return null;
            }

        }

        public static void SetRiskOfRainFolder(StorageFolder folder) => RiskOfRainInstallFolderString.Value = folder.Path;

        public static string AppVersion
        {
            get
            {
                Package package = Package.Current;
                PackageId packageId = package.Id;
                PackageVersion version = packageId.Version;

                return string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
            }
        }
    }
}
