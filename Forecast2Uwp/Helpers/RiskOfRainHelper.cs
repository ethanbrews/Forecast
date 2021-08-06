using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Forecast2Uwp.Helpers
{
    static class RiskOfRainHelper
    {
        public static async Task GuessOrAskForStorageFolderAsync()
        {
            var steamInstallFolder = await StorageFolder.GetFolderFromPathAsync(await Win32Bridge.GetSteamInstallationPathAsync());
            
            if (StorageHelper.DoesFolderExist(steamInstallFolder, @"steamapps\common\Risk of Rain 2"))
            {
                Options.SetRiskOfRainFolder(await StorageHelper.GetFolderAsync(steamInstallFolder, @"steamapps\common\Risk of Rain 2"));
            } else
            {
                await ThreadHelper.RunOnUIThreadAsync(async () =>
                {
                    var dialog = new Dialogs.SelectRiskOfRainDirectoryDialog();
                    await dialog.ShowAsync();
                    Options.SetRiskOfRainFolder(dialog.SelectedFolder);
                });
            }
        }
    }
}
