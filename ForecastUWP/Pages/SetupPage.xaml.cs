using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using ForecastUWP.Helpers;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ForecastUWP.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SetupPage : Page
    {

        private Control[][] ButtonTree;
        private StorageFolder ror2Folder = null;
        private bool _askForRiskOfRainFolder = true;

        public SetupPage()
        {
            this.InitializeComponent();
            ButtonTree = new Control[][]
            {
                new Control[]
                {
                    Pivot1,
                    Button0
                }, 
                new Control[]
                {
                    Pivot2,
                    Button1
                }, 
                new Control[]
                {
                    Pivot3,
                    Button2,
                }, 
                new Control[]
                {
                    Button3
                }, 
            };
        }

        private void EnableButtons()
        {
            void EnableSet(int index)
            {
                foreach (var b in ButtonTree[index])
                {
                    if (b is Button)
                        b.IsEnabled = true;
                }
            }

            foreach (var s in ButtonTree)
            {
                foreach (var b in s)
                {
                    if (b is Button)
                        b.IsEnabled = false;
                }
            }
            
            EnableSet(0);

            if (ror2Folder == null)
                return;
            EnableSet(1);

            if(!TOSCheckbox.IsChecked.HasValue || !TOSCheckbox.IsChecked.Value) 
                return;
            EnableSet(2);
            EnableSet(3);



        }

        private void TOSCheckbox_OnChecked(object sender, RoutedEventArgs e) => EnableButtons();

        private void TOSCheckbox_OnUnchecked(object sender, RoutedEventArgs e) => EnableButtons();

        private void PreviousPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainPivot.SelectedIndex == 0)
                return;
            if (!_askForRiskOfRainFolder && MainPivot.SelectedIndex == 2)
                MainPivot.SelectedIndex -= 2;
            else
                MainPivot.SelectedIndex -= 1;
        }

        private void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainPivot.SelectedIndex == MainPivot.Items.Count - 1)
            {
                ApplicationSettings.RiskOfRain2Path.Value = ror2Folder.Path;
                ApplicationSettings.RequiresFirstTimeSetup.Value = false;
                Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.AddOrReplace("ror2folder", ror2Folder);
                Crashes.SetEnabledAsync(CrashalyticsSwitch.IsOn);
                Analytics.SetEnabledAsync(AnalyticsSwitch.IsOn);
                Frame.Navigate(typeof(Pages.NavigationFrame), null, new EntranceNavigationTransitionInfo());
            }
            else
            {
                if (!_askForRiskOfRainFolder && MainPivot.SelectedIndex == 0)
                    MainPivot.SelectedIndex += 2;
                else
                    MainPivot.SelectedIndex += 1;
            }
        }

        private async void SelectRor2FolderButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FolderPicker();
            picker.FileTypeFilter.Add("*");
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.ComputerFolder;

            var folder = await picker.PickSingleFolderAsync();
            if (folder != null)
            {
                // Application now has read/write access to the picked file
                RiskOfRainFolderIndicatorTextBlock.Text = folder.Path;
                ror2Folder = folder;
                if (StorageHelper.DoesFileExist(folder, "Risk of Rain 2.exe"))
                    CantFindExeWarning.Visibility = Visibility.Collapsed;
                else
                    CantFindExeWarning.Visibility = Visibility.Visible;
            }
            EnableButtons();
        }

        private void MainPivot_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }

        private async void SetupPage_OnLoading(FrameworkElement sender, object args)
        {
            try
            {
                ror2Folder =
                    await StorageFolder.GetFolderFromPathAsync(@"C:\Program Files (x86)\Steam\steamapps\common\Risk of Rain 2");
                _askForRiskOfRainFolder = false;
            }
            catch (Exception ex)
            {
                // BroadFileSystemAccess can be disabled in settings. This will cause an UnauthorisedAccessException
                if (!(ex is UnauthorizedAccessException))
                    Crashes.TrackError(ex);
                else
                    MainPage.Current.EnqueueToastNotification("File System Access Disabled", "File system access is disabled for this app in Windows settings. You will be prompted to select the Risk of Rain 2 folder manually instead.", timeoutMs: 30000);
            }

            EnableButtons();
        }
    }
}
