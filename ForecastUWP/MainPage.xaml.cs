using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Dwrandaz.AutoUpdateComponent;
using Microsoft.AppCenter.Crashes;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ForecastUWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        public event EventHandler<UpdateInfo> UpdateStatusChangedEvent;
        public UpdateInfo ApplicationUpdateInfo;
        public static MainPage Current;

        public MainPage()
        {
            Current = this;
            this.InitializeComponent();

            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;

            Package package = Package.Current;
            AppTitleTextBlock.Text = string.Format("{0} {1}.{2}.{3}.{4} (Beta Release)", "Forecast", package.Id.Version.Major, package.Id.Version.Minor, package.Id.Version.Build, package.Id.Version.Revision);
            Window.Current.CoreWindow.SizeChanged += (s, e) => UpdateAppTitle();
            coreTitleBar.LayoutMetricsChanged += (s, e) => UpdateAppTitle();

            Task.Run(async () =>
            {
                var path = "http://storage.ethanbrews.me/Forecast/Release/Forecast.appinstaller";
                var info = await AutoUpdateManager.CheckForUpdatesAsync(path);
                if (!info.Succeeded)
                {
                    if (App.ErrorState == ApplicationErrorState.NONE) 
                        App.SetErrorState(ApplicationErrorState.CANNOT_GET_UPDATE_INFORMATION);
                    return;
                }

                ApplicationUpdateInfo = info;
                UpdateStatusChangedEvent?.Invoke(this, info);

                if (await Crashes.HasCrashedInLastSessionAsync())
                {
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () => {
                        await new Dialogs.AppCrashedApologyDialog().ShowAsync();
                    });
                }
                    
            });

        }

        private void UpdateAppTitle()
        {
            var full = (ApplicationView.GetForCurrentView().IsFullScreenMode);
            var left = 12 + (full ? 0 : CoreApplication.GetCurrentView().TitleBar.SystemOverlayLeftInset);
            AppTitleTextBlock.Margin = new Thickness(left, 8, 0, 0);
        }

        private void MainPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (ApplicationSettings.RequiresFirstTimeSetup.Value)
                ContentFrame.Navigate(typeof(Pages.SetupPage), null, new SuppressNavigationTransitionInfo());
            else
                ContentFrame.Navigate(typeof(Pages.NavigationFrame), null, new SuppressNavigationTransitionInfo());
        }
    }
}
