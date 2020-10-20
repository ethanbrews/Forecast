using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Dwrandaz.AutoUpdateComponent;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ForecastUWP.Pages
{

    public class AppIsUpdatingPageParameters
    {
        public NavigationView NavBar { get; set; }
        public Frame ContentFrame { get; set; }
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AppIsUpdatingPage : Page
    {

        private AppIsUpdatingPageParameters Parameters = null;
        
        public AppIsUpdatingPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter != null)
                Parameters = e.Parameter as AppIsUpdatingPageParameters;

            Parameters.NavBar.IsEnabled = false;

        }

        private async void AppIsUpdatingPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            var info = MainPage.Current.ApplicationUpdateInfo;
            var result = await AutoUpdateManager.TryToUpdateAsync(info);
            if (!result.Succeeded)
            {
                Parameters.NavBar.IsEnabled = true;
                FailedMessage.Visibility = Visibility.Visible;
                UpdatingMessage.Visibility = Visibility.Collapsed;
            }
        }

        private async void GoBackButton_Click(object sender, RoutedEventArgs e)
        {
            if (Parameters.ContentFrame.CanGoBack)
                Parameters.ContentFrame.GoBack();
        }
    }
}
