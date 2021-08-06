using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Forecast2Uwp.Download;

namespace Forecast2Uwp
{

    public enum ThunderstoreDataState { DOWNLOADED, FAILED, WAITING }

    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {

        private BackgroundTaskDeferral AppServiceDeferral;
        public AppServiceConnection Connection;
        public delegate void AppServiceConnectedEventArgs(object o, AppServiceTriggerDetails details);
        public event AppServiceConnectedEventArgs AppServiceConnected;

        public RemoteConfig.Config Config = null;

        public delegate void ThunderstoreDataStateChangedArgs(object o, ThunderstoreDataState state);
        public event ThunderstoreDataStateChangedArgs ThunderstoreDataStateChanged;
        public Thunderstore.Data.Package[] ThunderstorePackages;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
            AppCenter.Start("ec494238-33a4-4254-873e-990815dbb62d",
                   typeof(Analytics), typeof(Crashes));

        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            // Launch the desktop bridge. Once loaded it will trigger the App.BackgroundActivated event.
            if (ApiInformation.IsApiContractPresent("Windows.ApplicationModel.FullTrustAppContract", 1, 0))
            {
                await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync();
            }

            // Get remote config from the URL specified in options.
            // TODO: Do this in the background
            try
            {
                Config = await DownloadManager.GetObjectAsync(
                    new Uri(Options.RemoteConfigUrl.Value),
                    s => RemoteConfig.Config.FromJson(s),
                    false
                );
            }
            catch (Exception ex)
            {
                Analytics.TrackError(ex);
            }

            // Load from file so that the app is responsive straight away and will function offline.
            // This is immediately replaced by the full list from thunderstore once downloaded.
            ThunderstorePackages = await PackageManager.GetInstalledPackagesManifestAsync();
            _ = PackageManager.LoadProfilesFromFileAsync();
            
            // Get thunderstore packages in a seperate thread. Update ThunderstorePackages and fire event when completed
            _ = Task.Run(async () =>
            {
                try
                {
                    var ds = await DownloadManager.GetStringAsync(new Uri(new Uri(Options.ThunderstoreApiEndpointString.Value), "package/"));
                    var pkgs = Thunderstore.Data.Package.FromJson(ds);
                    ThunderstorePackages = pkgs.Where(x => !Options.BlockedPackagesFullNames.Contains(x.FullName)).ToArray();
                    await Helpers.ThreadHelper.RunOnUIThreadAsync(() => ThunderstoreDataStateChanged?.Invoke(this, ThunderstoreDataState.DOWNLOADED));
                } catch (Exception ex)
                {
                    Forecast2Uwp.Analytics.TrackError(ex);
                    await Helpers.ThreadHelper.RunOnUIThreadAsync(() => ThunderstoreDataStateChanged?.Invoke(this, ThunderstoreDataState.FAILED));
                }
                
            });

            //this.Suspending += App_Suspending;
            //this.Resuming += App_Resuming;

            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        /*private void App_Resuming(object sender, object e) =>
            _ = PackageManager.LoadProfilesFromFileAsync();

        private void App_Suspending(object sender, SuspendingEventArgs e) =>
            _ = PackageManager.SaveProfilesToFileAsync();*/

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }

        protected override void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
            base.OnBackgroundActivated(args);
            if (args.TaskInstance.TriggerDetails is AppServiceTriggerDetails details)
            {
                // only accept connections from callers in the same package
                if (details.CallerPackageFamilyName == Package.Current.Id.FamilyName)
                {
                    // connection established from the fulltrust process
                    AppServiceDeferral = args.TaskInstance.GetDeferral();
                    args.TaskInstance.Canceled += TaskInstance_Canceled;

                    Connection = details.AppServiceConnection;
                    AppServiceConnected?.Invoke(this, args.TaskInstance.TriggerDetails as AppServiceTriggerDetails);
                }
            }
        }

        private void TaskInstance_Canceled(Windows.ApplicationModel.Background.IBackgroundTaskInstance sender, Windows.ApplicationModel.Background.BackgroundTaskCancellationReason reason)
        {
            
        }
    }
}
