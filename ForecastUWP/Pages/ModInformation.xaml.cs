using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Connectivity;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using ForecastUWP.Data.Thunderstore.Packages;
using ForecastUWP.Helpers;
using HtmlAgilityPack;
using Microsoft.AppCenter.Crashes;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ForecastUWP.Pages
{

    public class ModInformationParameters
    {
        public CreatePreSelectedPackagesParameters CreateParameters { get; set; }
        public Package PackageToShow { get; set; }
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ModInformation : Page
    {

        public ModInformationParameters Parameters { get; set; }
        
        public ModInformation()
        {
            this.InitializeComponent();
        }

        private async void LoadThunderstoreWebPageForPackageAsync()
        {

            string html;
            try
            {
                html = await AppWebClient.GetStringAsync(
                    $"https://thunderstore.io/package/{Parameters.PackageToShow.Owner}/{Parameters.PackageToShow.Name}");
            }
            catch (Exception ex)
            {

                // Track error if internet connection is available, otherwise ignore it and return early.
                if (NetworkInformation.GetInternetConnectionProfile() != null)
                    Crashes.TrackError(ex);

                return;
            }
            

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var node = doc.DocumentNode.SelectNodes("//div[contains(@class, 'card') and contains(@class, 'mb-3') and contains(@class, 'mt-2')]")[0];
            var mdNode = node.SelectNodes("//div[contains(@class, 'markdown-body')]")[0];
            

            DescriptionWebView.NavigateToString(WebHelper.AddHTMLBoilerplate(mdNode.WriteTo()));
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

            if (e.Parameter == null)
                return;

            Parameters = e.Parameter as ModInformationParameters;
            Parameters.CreateParameters.SupressModsPrefilledMessage = true;
            ModImage.Source = new BitmapImage(Parameters.PackageToShow.ImageUri);

            base.OnNavigatedTo(e);
            var anim = ConnectedAnimationService.GetForCurrentView().GetAnimation("ForwardConnectedAnimation");
            if (anim != null)
            {
                anim.Configuration = new BasicConnectedAnimationConfiguration();
                anim.TryStart(ModImage);
            }

            try
            {
                LoadThunderstoreWebPageForPackageAsync();
            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex);

            }


        }

        private void GoBackToListView()
        {
            Frame.Navigate(typeof(Create), Parameters.CreateParameters, new DrillInNavigationTransitionInfo());
        }

        private void ReturnToListButton_Click(object sender, RoutedEventArgs e)
        {
            GoBackToListView();
        }

        private void InstallButton_Click(object sender, RoutedEventArgs e)
        {
            Parameters.CreateParameters.PreSelectedPackages.Add(Parameters.PackageToShow);
            GoBackToListView();
        }

        private async void ShowInBrowserButton_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(
                new Uri(
                    $"https://thunderstore.io/package/{Parameters.PackageToShow.Owner}/{Parameters.PackageToShow.Name}"));
        }
    }
}
