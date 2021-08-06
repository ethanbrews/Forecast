using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Forecast2Uwp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {

        public static MainPage Current;

        private HashSet<VirtualKey> CurrentlyPressedKeys;

        public event PropertyChangedEventHandler PropertyChanged;

        public MainPage()
        {
            Current = this;
            InitializeComponent();
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            CurrentlyPressedKeys = new HashSet<VirtualKey>();

            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

            // Hide default title bar.
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            UpdateTitleBarLayout(coreTitleBar);

            // Set XAML element as a draggable region.
            //Window.Current.SetTitleBar(AppTitleBar);

            // Register a handler for when the size of the overlaid caption control changes.
            // For example, when the app moves to a screen with a different DPI.
            coreTitleBar.LayoutMetricsChanged += CoreTitleBar_LayoutMetricsChanged;

            // Register a handler for when the title bar visibility changes.
            // For example, when the title bar is invoked in full screen mode.
            coreTitleBar.IsVisibleChanged += CoreTitleBar_IsVisibleChanged;

            //Register a handler for when the window changes focus
            Window.Current.Activated += Current_Activated;

            (Application.Current as App).AppServiceConnected += MainPage_AppServiceConnected;
            (Application.Current as App).ThunderstoreDataStateChanged += MainPage_ThunderstoreDataStateChanged;

            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
            Window.Current.CoreWindow.KeyUp += CoreWindow_KeyUp;

            PackageManager.DownloadingFilesEvent += PackageManager_DownloadingFilesEvent;
        }

        private void PackageManager_DownloadingFilesEvent(int queueSize)
        {
            DownloadIndicator.Visibility = queueSize == 0 ? Visibility.Collapsed : Visibility.Visible;
            DownloadCount.Text = queueSize.ToString();
        }

        private void CoreWindow_KeyDown(CoreWindow sender, KeyEventArgs e)
        {
            CurrentlyPressedKeys.Add(e.VirtualKey);
            System.Diagnostics.Debug.WriteLine("Currently pressed keys: " + string.Join(", ", from k in CurrentlyPressedKeys select k.ToString()));
            if (new VirtualKey[] { VirtualKey.P, VirtualKey.Control, VirtualKey.Shift }.All(k => CurrentlyPressedKeys.Contains(k)))
            {
                //All keys are pressed
                DebugFlyout.ShowAt(AppTitleBar);
            }
        }

        private void CoreWindow_KeyUp(CoreWindow sender, KeyEventArgs e) =>
            CurrentlyPressedKeys.Remove(e.VirtualKey);

        private void MainPage_ThunderstoreDataStateChanged(object o, ThunderstoreDataState state)
        {
            ThunderstoreConnectingIndicator.Visibility = Visibility.Collapsed;
            ThunderstoreFailedIndicator.Visibility = Visibility.Collapsed;
            DownloadIndicator.Visibility = Visibility.Collapsed;
            if (state == ThunderstoreDataState.DOWNLOADED)
            {
                NavigationThunderstoreItem.IsEnabled = true;
                _ = PackageManager.CheckForDownloadsAsync();
            }
            else if (state == ThunderstoreDataState.FAILED)
                ThunderstoreFailedIndicator.Visibility = Visibility.Visible;
            else if (state == ThunderstoreDataState.WAITING)
                ThunderstoreConnectingIndicator.Visibility = Visibility.Visible;

        }
            


        private async void MainPage_AppServiceConnected(object o, Windows.ApplicationModel.AppService.AppServiceTriggerDetails details)
        {
            if (await Options.GetRiskOfRainFolderAsync() == null)
                await Helpers.RiskOfRainHelper.GuessOrAskForStorageFolderAsync();
            await PackageManager.LoadProfilesFromFileAsync();
        }

        private void CoreTitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args) => UpdateTitleBarLayout(sender);

        private void UpdateTitleBarLayout(CoreApplicationViewTitleBar coreTitleBar)
        {
            // Update title bar control size as needed to account for system size changes.
            AppTitleBar.Height = coreTitleBar.Height;

            // Ensure the custom title bar does not overlap window caption controls
            Thickness currMargin = AppTitleBar.Margin;
            AppTitleBar.Margin = new Thickness(currMargin.Left, currMargin.Top, coreTitleBar.SystemOverlayRightInset, currMargin.Bottom);
        }

        private void CoreTitleBar_IsVisibleChanged(CoreApplicationViewTitleBar sender, object args) => AppTitleBar.Visibility = sender.IsVisible ? Visibility.Visible : Visibility.Collapsed;

        // Update the TitleBar based on the inactive/active state of the app
        private void Current_Activated(object sender, Windows.UI.Core.WindowActivatedEventArgs e)
        {
            SolidColorBrush defaultForegroundBrush = (SolidColorBrush)Application.Current.Resources["TextFillColorPrimaryBrush"];
            SolidColorBrush inactiveForegroundBrush = (SolidColorBrush)Application.Current.Resources["TextFillColorDisabledBrush"];

            AppTitle.Foreground = e.WindowActivationState == Windows.UI.Core.CoreWindowActivationState.Deactivated
                ? inactiveForegroundBrush
                : defaultForegroundBrush;
        }

        // Update the TitleBar content layout depending on NavigationView DisplayMode
        private void NavigationViewControl_DisplayModeChanged(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewDisplayModeChangedEventArgs args)
        {
            const int topIndent = 16;
            const int expandedIndent = 48;
            int minimalIndent = 104;

            // If the back button is not visible, reduce the TitleBar content indent.
            if (NavigationViewControl.IsBackButtonVisible.Equals(Microsoft.UI.Xaml.Controls.NavigationViewBackButtonVisible.Collapsed))
            {
                minimalIndent = 48;
            }

            Thickness currMargin = AppTitleBar.Margin;

            // Set the TitleBar margin dependent on NavigationView display mode
            AppTitleBar.Margin = sender.PaneDisplayMode == Microsoft.UI.Xaml.Controls.NavigationViewPaneDisplayMode.Top
                ? new Thickness(topIndent, currMargin.Top, currMargin.Right, currMargin.Bottom)
                : sender.DisplayMode == Microsoft.UI.Xaml.Controls.NavigationViewDisplayMode.Minimal
                    ? new Thickness(minimalIndent, currMargin.Top, currMargin.Right, currMargin.Bottom)
                    : new Thickness(expandedIndent, currMargin.Top, currMargin.Right, currMargin.Bottom);
        }

        private void OpenPageDebugFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            Type page;
            switch ((sender as FrameworkElement).Tag as string)
            {
                case "Play":
                    page = typeof(Play.Play);
                    break;
                case "Thunderstore.Search":
                    page = typeof(Thunderstore.ModSearch);
                    break;
                case "Thunderstore.Name":
                    page = typeof(Thunderstore.NameProfile);
                    break;
                case "ConfigEdit":
                    page = typeof(ConfigEditor.ConfigEdit);
                    break;
                case "Downloads":
                    page = typeof(ConfigEditor.ConfigEdit);
                    break;
                case "Settings":
                    page = typeof(Settings.SettingsPage);
                    break;
                case "Debug":
                    page = typeof(Settings.Debug);
                    break;
                default:
                    return;
            }

            _ = ContentFrame.Navigate(page);
        }

        private void NavigationViewControl_SelectionChanged(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewSelectionChangedEventArgs args)
        {
            Type pageType;
            var tag = (string)((FrameworkElement)args.SelectedItem).Tag;
            if (tag == null || tag == "")
                return;
            switch (tag)
            {
                case "NavigationThunderstoreItem":
                    pageType = typeof(Thunderstore.ModSearch);
                    break;
                case "NavigationCfgEditItem":
                    pageType = typeof(Play.Play);
                    break;
                default:
                    pageType = typeof(Play.Play);
                    break;
            }
            if (args.IsSettingsSelected)
            {
                pageType = typeof(Settings.SettingsPage);
            }

            _ = ContentFrame.Navigate(pageType, null, args.RecommendedNavigationTransitionInfo);
        }

        private async void OpenFolderDebugFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            StorageFolder folder = null;
            switch ((sender as FrameworkElement).Tag as string)
            {
                case "Local":
                    folder = ApplicationData.Current.LocalFolder;
                    break;
                case "Roaming":
                    folder = ApplicationData.Current.RoamingFolder;
                    break;
                case "LocalCache":
                    folder = ApplicationData.Current.LocalCacheFolder;
                    break;
                case "SharedLocal":
                    folder = ApplicationData.Current.SharedLocalFolder;
                    break;
                case "RoR2":
                    folder = await Options.GetRiskOfRainFolderAsync();
                    break;
                case "Temporary":
                    folder = ApplicationData.Current.TemporaryFolder;
                    break;
                default:
                    break;
            }
            if (folder is null)
                return;

            _ = Launcher.LaunchFolderAsync(folder);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Visibility vis(bool b) => b ? Visibility.Visible : Visibility.Collapsed;

            base.OnNavigatedTo(e);
            var cfg = (Application.Current as App).Config;

            NavigationPlayItem.Visibility = vis(cfg?.Features?.PlayPage?.IsEnabled ?? true);
            NavigationThunderstoreItem.Visibility = vis(cfg?.Features?.ThunderstorePage?.IsEnabled ?? true);
            NavigationCfgEditItem.Visibility = vis(cfg?.Features?.ConfigEditorPage?.IsEnabled ?? true);
            NavigationDownloadsItem.Visibility = vis(cfg?.Features?.DownloadPage?.IsEnabled ?? true);
            NavigationViewControl.IsSettingsVisible = cfg?.Features?.SettingsPage?.IsEnabled ?? true;
        }

        private void RunUpdateInstallRoutineDebugFlyoutItem_Click(object sender, RoutedEventArgs e) =>
            _ = PackageManager.CheckForDownloadsAsync();

        private async void ImportPackRoutineDebugFlyoutItem_Click(object sender, RoutedEventArgs e) =>
            await new Dialogs.ImportProfile().ShowAsync();
    }
}
