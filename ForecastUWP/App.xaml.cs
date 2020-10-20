using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using ForecastUWP.Helpers;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using ForecastUWP.Data;

namespace ForecastUWP
{

    public enum ApplicationErrorState
    {
        NONE,
        CANNOT_GET_THUNDERSTORE_PACKAGES,
        CANNOT_GET_UPDATE_INFORMATION,
        CANNOT_LOAD_REMOTE_CONFIG
    }

    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {

        public static ForecastUWP.Data.Thunderstore.Packages.Package[] ThunderstorePackages = null;
        public static ApplicationErrorState ErrorState { get; private set; } = ApplicationErrorState.NONE;
        public static event EventHandler<ApplicationErrorState> ApplicationErrorStateChanged;
        public static bool CanLoadRemoteConfig = false;
        public static RemoteConfig RemoteAppConfig = null;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
#if !DEBUG
            AppCenter.Start(Secrets.AppCenterSecret,
                typeof(Analytics), typeof(Crashes));
#endif
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        public static void SetErrorState(ApplicationErrorState state)
        {
            ErrorState = state;
            ApplicationErrorStateChanged?.Invoke(null, state);
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
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
                try
                {
                    var packages = await AppWebClient.GetObjectAsync(Constants.ThunderstorePackagesApiEndpoint,
                            s => Data.Thunderstore.Packages.Package.FromJson(s)) as
                        Data.Thunderstore.Packages.Package
                        [];

                    ThunderstorePackages = packages.Where(x => !(new string[]
                    {
                        "ebkr-r2modman",
                        "HoodedDeath-RiskOfDeathModManager",
                        "scottbot95-RoR2ModManager",
                        "ethanbrews-RiskOfRainModManager",
                        "MythicManiac-MythicModManager",
                        "ethanbrews-Forecast_Mod_Manager"
                    }).Contains(x.FullName)).ToArray();
                }
                catch (Exception ex)
                {
                    ErrorState = ApplicationErrorState.CANNOT_GET_THUNDERSTORE_PACKAGES;
                    ApplicationErrorStateChanged?.Invoke(this, ErrorState);
                    Crashes.TrackError(ex);
                }

                try
                {
                    RemoteAppConfig = await AppWebClient.GetObjectAsync(Constants.ForecastRemoteConfigApiEndpoint, s => RemoteConfig.FromJson(s)) as RemoteConfig;
                } catch (Exception ex)
                {
                    if (ErrorState == ApplicationErrorState.NONE)
                    {
                        ErrorState = ApplicationErrorState.CANNOT_LOAD_REMOTE_CONFIG;
                        ApplicationErrorStateChanged?.Invoke(this, ErrorState);
                    }
                    Crashes.TrackError(ex);
                }

                rootFrame.CacheSize = 2;
                Window.Current.Activate();
                ExtendAcrylicIntoTitleBar();
                SetThemeFromSettings();
            }
        }

        public void SetThemeFromSettings()
        {
            var rootFrame = Window.Current.Content as Frame;
            switch (ApplicationSettings.RequestedThemeName.Value)
            {
                case "Dark":
                    rootFrame.RequestedTheme = ElementTheme.Dark;
                    break;
                case "Light":
                    rootFrame.RequestedTheme = ElementTheme.Light;
                    break;
                default:
                    rootFrame.RequestedTheme = ElementTheme.Default;
                    break;
            }
        }

        private void ExtendAcrylicIntoTitleBar()
        {
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
        }

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
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            await ProfileHelper.SaveProfilesToFileAsync();
            deferral.Complete();
        }
    }
}
