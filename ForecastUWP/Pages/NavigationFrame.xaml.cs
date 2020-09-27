using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.System;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Microsoft.AppCenter.Crashes;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ForecastUWP.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NavigationFrame : Page
    {
        public NavigationFrame()
        {
            this.InitializeComponent();

            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;

            SetTitleBarErrorStringAndColour();
            Window.Current.CoreWindow.SizeChanged += (s, e) => UpdateAppTitle();
            coreTitleBar.LayoutMetricsChanged += (s, e) => UpdateAppTitle();

            App.ApplicationErrorStateChanged += (sender, state) => SetTitleBarErrorStringAndColour();

            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            NavigationViewControl.SelectedItem = PlayNavItem;
            
            MainPage.Current.UpdateStatusChangedEvent += (sender, info) => SetUpdateButtonVisibility();
            SetUpdateButtonVisibility();
        }

        private void SetUpdateButtonVisibility()
        {
            UpdateButton.Visibility =
                (MainPage.Current.ApplicationUpdateInfo != null && MainPage.Current.ApplicationUpdateInfo.ShouldUpdate
                    ? Visibility.Visible
                    : Visibility.Collapsed);
        }

        private void SetTitleBarErrorStringAndColour()
        {
            switch (App.ErrorState)
            {
                case ApplicationErrorState.CANNOT_GET_THUNDERSTORE_PACKAGES:
                    NavIndicatorRect.Fill = new SolidColorBrush(Colors.Orange);
                    ErrorMessageTextBlock.Text = "Could not connect to thunderstore.io";
                    ErrorMessageTextBlockWrapper.Visibility = Visibility.Visible;
                    CreateNavItem.IsEnabled = false;
                    break;

                case ApplicationErrorState.CANNOT_GET_UPDATE_INFORMATION:
                    NavIndicatorRect.Fill = new SolidColorBrush(Colors.Red);
                    ErrorMessageTextBlock.Text = "Could not check for app updates";
                    ErrorMessageTextBlockWrapper.Visibility = Visibility.Visible;
                    break;

                default:
                    //NavIndicatorRect.Fill = Resources["SystemControlHighlightAccentBrush"] as Brush;
                    NavIndicatorRect.Fill = new SolidColorBrush(Colors.Transparent);
                    ErrorMessageTextBlock.Text = "";
                    CreateNavItem.IsEnabled = true;
                    ErrorMessageTextBlockWrapper.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private void UpdateAppTitle()
        {
            var full = (ApplicationView.GetForCurrentView().IsFullScreenMode);
            var left = 12 + (full ? 0 : CoreApplication.GetCurrentView().TitleBar.SystemOverlayLeftInset);
            ErrorMessageTextBlock.Margin = new Thickness(left, 8, 0, 0);
        }

        private void SettingsButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            if (ContentFrame.CurrentSourcePageType == typeof(Pages.SettingsPage))
                return;
            ContentFrame.Navigate(typeof(Pages.SettingsPage), null,
                new EntranceNavigationTransitionInfo());
            NavigationViewControl.SelectedItem = null;
        }

        private void UpdateButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            if (ContentFrame.CurrentSourcePageType == typeof(Pages.AppIsUpdatingPage))
                return;
            ContentFrame.Navigate(typeof(Pages.AppIsUpdatingPage), null,
                new EntranceNavigationTransitionInfo());
        }

        private void NavigationViewControl_OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem == PlayNavItem)
                ContentFrame.Navigate(typeof(Pages.Play), null, args.RecommendedNavigationTransitionInfo);
            else if (args.SelectedItem == CreateNavItem)
                ContentFrame.Navigate(typeof(Pages.Create), null, args.RecommendedNavigationTransitionInfo);
        }
    }
}
