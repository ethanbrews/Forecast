using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
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
    struct NotificationData
    {
        public string Title;
        public string Message;
        public string Icon;
        public int Timeout;
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        public event EventHandler<UpdateInfo> UpdateStatusChangedEvent;
        public UpdateInfo ApplicationUpdateInfo;
        public static MainPage Current;
        Queue<NotificationData> NotificationQueue;
        private Guid? CurrentNotificationToken;

        public MainPage()
        {
            Current = this;
            NotificationQueue = new Queue<NotificationData>();
            CurrentNotificationToken = null;
            this.InitializeComponent();

            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;

            Package package = Package.Current;
#if DEBUG
            var appName = "Forecast [DEBUG BUILD]";
#else
            var appName = "Forecast";
#endif
            AppTitleTextBlock.Text = string.Format("{0} {1}.{2}.{3}.{4}", appName, package.Id.Version.Major, package.Id.Version.Minor, package.Id.Version.Build, package.Id.Version.Revision);
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

        public void EnqueueToastNotification(string title, string message, string icon = null, int timeoutMs = 0)
        {
            NotificationQueue.Enqueue(new NotificationData
            {
                Title = title,
                Message = message,
                Icon = icon ?? "\uE7BA",
                Timeout = timeoutMs
            });

            if (!CurrentNotificationToken.HasValue)
                ShowNextToastNotification();
        }

        private void ShowNextToastNotification()
        {
            var guid = Guid.NewGuid();
            CurrentNotificationToken = guid;
            var next = NotificationQueue.Dequeue();
            Notification_Icon.Glyph = next.Icon;
            Notification_Text.Text = next.Message;
            Notification_Title.Text = next.Title;

            if (next.Timeout > 0)
            {
                _ = Task.Run(() =>
                {
                    Thread.Sleep(next.Timeout);
                    _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                    {
                        if (guid == CurrentNotificationToken)
                        {
                            ExitStoryBoard.Begin();

                            if (NotificationQueue.Count > 0)
                                ShowNextToastNotification();
                            else
                                CurrentNotificationToken = null;
                        }
                            
                    });
                });
            }

            ToastNotification.Visibility = Visibility.Visible;
            if (ApplicationSettings.NotificationSoundsEnabled.Value)
            {
                NotificationArrivedMediaElement.Position = TimeSpan.Zero;
                NotificationArrivedMediaElement.Play();
            }
            EnterStoryBoard.Begin();
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

        private void CloseInAppToastButton_Click(object sender, RoutedEventArgs e)
        {
            ExitStoryBoard.Begin();
            _ = Task.Run(() =>
            {
                Thread.Sleep(300);
                _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                {
                    if (NotificationQueue.Count > 0)
                        ShowNextToastNotification();
                    else
                        CurrentNotificationToken = null;
                });
            });
        }
    }
}
