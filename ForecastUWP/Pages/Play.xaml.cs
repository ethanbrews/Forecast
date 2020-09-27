using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using ForecastUWP.Data;
using ForecastUWP.Dialogs;
using ForecastUWP.Helpers;
using Microsoft.AppCenter.Crashes;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ForecastUWP.Pages
{
    public class InstallProfilePlayParameter
    {
        public Profile ProfileToInstall { get; set; }
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Play : Page
    {
        public Play()
        {
            this.InitializeComponent();
        }


        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var profiles = await ProfileHelper.GetProfilesAsync();


            var parameter = e.Parameter as InstallProfilePlayParameter;
            System.Diagnostics.Debug.WriteLine($"Parameter is {parameter}");
            if (parameter != null)
            {
                parameter.ProfileToInstall.RequiresInstall = true;
                profiles.Remove(profiles.Where(x => x.Name == parameter.ProfileToInstall.Name).FirstOrDefault());
                profiles.Insert(0, parameter.ProfileToInstall);
                InstallProfile(parameter.ProfileToInstall);
            }

            ProfilesItemsControl.ItemsSource = new ObservableCollection<Profile>(profiles);
        }

        private void MarqueeStackPanelContainer_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!ApplicationSettings.UseMarqueeEffectForMods.Value)
            {
                (sender as FrameworkElement).Visibility = Visibility.Collapsed;
                return;
            }

            (sender as FrameworkElement).Visibility = Visibility.Visible;

            var width = ((sender as FrameworkElement).FindName("ModsList") as FrameworkElement).ActualWidth;
            var displayWidth = (sender as StackPanel).ActualWidth -
                               ((sender as StackPanel).Padding.Left + (sender as StackPanel).Padding.Right);
            var storyboard = ((sender as FrameworkElement).FindName("MarqueeStoryBoard") as Storyboard);
            var control = ((sender as FrameworkElement).FindName("ModsList") as ItemsControl);
            var animation = new DoubleAnimation();
            animation.From = displayWidth;
            animation.To = -width;
            animation.Duration = new Duration(TimeSpan.FromSeconds(width / 50));
            animation.RepeatBehavior = RepeatBehavior.Forever;


            Storyboard.SetTarget(storyboard, control);
            Storyboard.SetTargetProperty(storyboard, "(Canvas.Left)");

            storyboard?.Children.Add(animation);
            if (control?.ActualWidth > displayWidth)
                storyboard?.Begin();
            else
            {
                var margin = control?.Margin ?? new Thickness(0);
                margin.Left -= 5;
                control.Margin = margin;
            }

        }

        private async Task StartProfile(Profile profile)
        {
            await SetupProfile(profile);
            await Launcher.LaunchUriAsync(new Uri("steam://rungameid/632360"));
        }

        private async Task SetupProfile(Profile profile)
        {

            var riskOfRainFolder = await ApplicationSettings.GetRiskOfRain2Folder();
            var bepInExFolder = await riskOfRainFolder.CreateFolderAsync("BepInEx", CreationCollisionOption.OpenIfExists);
            var pluginsFolder = await bepInExFolder.CreateFolderAsync("plugins", CreationCollisionOption.OpenIfExists);

            if (profile.Name == "Vanilla")
            {
                if (StorageHelper.DoesFileExist(riskOfRainFolder, "winhttp.dll"))
                    await (await riskOfRainFolder.GetFileAsync("winhttp.dll")).DeleteAsync();
                return;
            }

            foreach (var pkg in profile.Packages)
            {
                var targetModVersion = pkg.Versions.Where(x => x.VersionNumber == pkg.ForecastProfileSelectedVersion)
                    .FirstOrDefault();
                var modZipFile = await StorageHelper.GetLocalFileAsync(@$"mods\{targetModVersion.FullName}.zip");
                
                var tempFolder = await StorageHelper.GetTemporaryFolderAsync();
                await StorageHelper.ExtractZipArchiveToDirectoryAsync(modZipFile, tempFolder);
                if (pkg.Name == "BepInExPack")
                {
                    var bepinex = await tempFolder.GetFolderAsync("BepInExPack");
                    await StorageHelper.CopyFolderAsync(bepinex, riskOfRainFolder);
                } else if ((new string[] {"plugins", "monomod", "patchers", "R2API"})
                    .Where(x => StorageHelper.DoesFolderExist(tempFolder, x)).Any())
                {
                    await StorageHelper.CopyFolderAsync(tempFolder, pluginsFolder);
                }
                else
                {
                    await StorageHelper.CopyFolderAsync(tempFolder, await pluginsFolder.CreateFolderAsync(targetModVersion.FullName, CreationCollisionOption.OpenIfExists));
                }
            }
        }

        private async void InstallProfile(Profile profile)
        {
            System.Diagnostics.Debug.WriteLine("Installing profile: " + profile.Name);
            profile.CurrentInstalledTotalToInstallProgress = new Tuple<int, int, int>(0, profile.Packages.Length, 0);
            profile.RequiresInstall = true;
            if (profile.Packages == null)
                return;
            
            int count = 0;
            foreach (var pkg in profile.Packages)
            {
                var latestVersion = pkg.Versions.OrderByDescending(x => x.DateCreated).FirstOrDefault();
                var path = @$"mods\{latestVersion.FullName}.zip";
                if (!StorageHelper.DoesLocalFileExist(path))
                {
                    var downloadUriStr =
                        $"https://thunderstore.io/package/download/{pkg.Owner}/{pkg.Name}/{latestVersion.VersionNumber}/";
                    if (await AppWebClient.GetFileAsync(
                        new Uri(downloadUriStr),
                        await StorageHelper.GetLocalFileAsync(path)) == null)
                    {
                        await ProfileHelper.DeleteProfileAsync(profile);
                        await new Dialogs.ProfileInstallationError(profile.Name, $"Couldn't install {latestVersion.FullName} ({downloadUriStr}). Ensure the package is still available and that you have a connection to https://thunderstore.io").ShowAsync();
                    }
                }

                pkg.ForecastProfileSelectedVersion = latestVersion.VersionNumber;
                profile.CurrentInstalledTotalToInstallProgress = new Tuple<int, int, int>(count, profile.Packages.Length, (int)Math.Floor((double)(100 * (count / profile.Packages.Length))));
            }
            profile.RequiresInstall = false;
            try
            {
                await ProfileHelper.SaveProfilesToFileAsync();
            }
            catch (IOException ex)
            {
                Crashes.TrackError(ex);
            }
            
        }

        private async void DeleteProfileButton_Click(object sender, RoutedEventArgs e)
        {
            var ctx = (sender as FrameworkElement).DataContext as Profile;
            if (await ProfileHelper.DeleteProfileAsync(ctx))
                (ProfilesItemsControl.ItemsSource as ObservableCollection<Profile>).Remove(ctx);
            //TODO: else { Show error dialog }
        }

        private async void ShareProfileButton_Click(object sender, RoutedEventArgs e) => await new Dialogs.ShareProfileDialog((sender as FrameworkElement).DataContext as Profile).ShowAsync();

        private async void ImportProfileButton_Click(object sender, RoutedEventArgs e)
        {
            var importDialog = new ImportProfileDialog();
            await importDialog.ShowAsync();
            if (importDialog.DownloadedProfile != null)
            {
                ProfileHelper.AddProfileAsync(importDialog.DownloadedProfile);
                (ProfilesItemsControl.ItemsSource as ObservableCollection<Profile>).Insert(0, importDialog.DownloadedProfile);
                InstallProfile(importDialog.DownloadedProfile);
            }
        }

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StartProfile((sender as FrameworkElement).DataContext as Profile);
            }
            catch (Exception ex)
            {
                if (ex is FileNotFoundException)
                {
                    //Repair profile
                }
                else
                {
                    Crashes.TrackError(ex);
                    // Unexpected Error
                }

            }
        }
    }
}
