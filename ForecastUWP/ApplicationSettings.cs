using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using ForecastUWP.Helpers;
using Microsoft.AppCenter.Crashes;

namespace ForecastUWP
{
    class ApplicationSettings
    {
        public static LocalSettingsValue<int> CreatePageMaxItemsOnPage = new LocalSettingsValue<int>("CreatePageMaxItemsOnPage", 100);
        public static LocalSettingsValue<string> RiskOfRain2Path = new LocalSettingsValue<string>("RiskOfRain2Path", null);
        public static LocalSettingsValue<bool> RequiresFirstTimeSetup = new LocalSettingsValue<bool>("RequiresFirstTimeSetup", true);
        public static LocalSettingsValue<bool> UseMarqueeEffectForMods = new LocalSettingsValue<bool>("UseMarqueeEffectForMods", true);
        public static LocalSettingsValue<DateTime> LastUpdatedTime = new LocalSettingsValue<DateTime>("LastUpdatedTime", DateTime.Now);
        public static LocalSettingsValue<string> RequestedThemeName = new LocalSettingsValue<string>("RequestedThemeName", "System");

        public static async Task<StorageFolder> GetRiskOfRain2Folder()
        {
            try
            {
                return await StorageFolder.GetFolderFromPathAsync(RiskOfRain2Path.Value);
            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex);
                RequiresFirstTimeSetup.Value = true;
                return null;
            }
        }
    }
}