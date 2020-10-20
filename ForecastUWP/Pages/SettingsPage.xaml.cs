using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using ForecastUWP.Helpers;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.Toolkit.Uwp.UI.Animations.Behaviors;
using Microsoft.Toolkit.Uwp.UI.Extensions;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ForecastUWP.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {

        public SettingsPage()
        {
            this.InitializeComponent();
        }

        private async void SettingsPage_OnLoading(FrameworkElement sender, object args)
        {
            RorInstallLocationRun.Text = (await ApplicationSettings.GetRiskOfRain2Folder()).Path;
            PackageInstallLocationRun.Text = ApplicationData.Current.LocalFolder.Path;

            //LastUpdatedRun.Text = ApplicationSettings.LastUpdatedTime.Value.ToString("dddd, dd MMMM yyyy HH:mm");
            var version = Package.Current.Id.Version;
            PackageFamilyRun.Text = Package.Current.Id.FamilyName;
            PackageVersionRun.Text = string.Format("{0}.{1}.{2}.{3} {4}", version.Major, version.Minor, version.Build, version.Revision, Package.Current.Id.Architecture.ToString().ToLower());

            CrashesToggle.IsOn = await Crashes.IsEnabledAsync();
            AnalyticsToggle.IsOn = await Analytics.IsEnabledAsync();

            NotificationSoundsToggle.IsOn = ApplicationSettings.NotificationSoundsEnabled.Value;
            ApplicationSoundsToggle.IsOn = ElementSoundPlayer.State == ElementSoundPlayerState.On;

            switch (ApplicationSettings.RequestedThemeName.Value)
            {
                case "System":
                    SystemThemeRadioButton.IsChecked = true;
                    break;
                case "Dark":
                    DarkThemeRadioButton.IsChecked = true;
                    break;
                case "Light":
                    LightThemeRadioButton.IsChecked = true;
                    break;
            }
        }

        private async void ResetAppSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var k in ApplicationData.Current.LocalSettings.Values.Keys)
            {
                ApplicationData.Current.LocalSettings.Values.Remove(k);
            }

            await ApplicationData.Current.LocalFolder.DeleteAsync();
            await ApplicationData.Current.LocalCacheFolder.DeleteAsync();
            await ApplicationData.Current.TemporaryFolder.DeleteAsync();

            Analytics.TrackEvent("AppDataReset");

            // Attempt restart, with arguments.
            AppRestartFailureReason result =
                await CoreApplication.RequestRestartAsync("");

            // Restart request denied, send a toast to tell the user to restart manually.
            if (result == AppRestartFailureReason.NotInForeground
                || result == AppRestartFailureReason.Other)
            {
                await new Dialogs.PleaseManuallyRestartDialog().ShowAsync();
            }
        }

        private async void UriButton_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as Button).Tag as string)
            {
                case "IssueTracker":
                    await Launcher.LaunchUriAsync(new Uri("https://github.com/ethanbrews/Forecast/issues"));
                    break;
                case "Terms":
                    await Launcher.LaunchUriAsync(new Uri("https://ethanbrews.me/terms/forecast.html"));
                    break;
                case "Privacy":
                    await Launcher.LaunchUriAsync(new Uri("https://ethanbrews.me/privacy/forecast.html"));
                    break;
            }
        }

        private async void ChangeRorFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FolderPicker();
            picker.FileTypeFilter.Add("*");
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.ComputerFolder;

            var folder = await picker.PickSingleFolderAsync();

            if (folder == null)
                return;
            
            CantFindExeWarningMessage.Visibility = (StorageHelper.DoesFileExist(folder, "Risk of Rain 2.exe")
                ? Visibility.Collapsed
                : Visibility.Visible);

            ApplicationSettings.RiskOfRain2Path.Value = folder.Path;
            RorInstallLocationRun.Text = folder.Path;
            Analytics.TrackEvent("RiskOfRainDirectoryChanged");
        }

        private async void OpenRorFolderButton_Click(object sender, RoutedEventArgs e) =>
            await Launcher.LaunchFolderAsync(await ApplicationSettings.GetRiskOfRain2Folder());

        private async void OpenAppFolderButton_Click(object sender, RoutedEventArgs e) =>
            await Launcher.LaunchFolderAsync(ApplicationData.Current.LocalFolder);

        private void ThemeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            ApplicationSettings.RequestedThemeName.Value = (sender as RadioButton).Tag as string;
            (App.Current as App).SetThemeFromSettings();
            Analytics.TrackEvent("ThemeChanged", new Dictionary<string, string> { {"Value", ApplicationSettings.RequestedThemeName.Value } });
        }

        private void AnalyticsToggle_OnToggled(object sender, RoutedEventArgs e) =>
            Analytics.SetEnabledAsync(AnalyticsToggle.IsOn);

        private async void CrashesToggle_OnToggled(object sender, RoutedEventArgs e)
        {
            Crashes.SetEnabledAsync(CrashesToggle.IsOn);
        }
            

        private async void ManageSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("ms-settings:appsfeatures-app"));
            Analytics.TrackEvent("ManageSettingsOpened");
        }

        private void NotificationSoundsToggle_OnToggled(object sender, RoutedEventArgs e) =>
            ApplicationSettings.NotificationSoundsEnabled.Value = NotificationSoundsToggle.IsOn;

        private void ApplicationSoundsToggle_OnToggled(object sender, RoutedEventArgs e) => ElementSoundPlayer.State =
            ApplicationSoundsToggle.IsOn ? ElementSoundPlayerState.On : ElementSoundPlayerState.Off;

        private void CleanUpModsFolder_Click(object sender, RoutedEventArgs e)
        {
            Play.CleanUpUnusedModsAsync();
            ModsFolderCleanedIndicator.Visibility = Visibility.Visible;
        }
    }
}
