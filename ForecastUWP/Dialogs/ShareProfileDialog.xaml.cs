using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;
using ForecastUWP.Data;
using ForecastUWP.Helpers;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using HttpResponseMessage = Windows.Web.Http.HttpResponseMessage;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ForecastUWP.Dialogs
{
    public sealed partial class ShareProfileDialog : ContentDialog
    {
        private Profile _profile;

        public ShareProfileDialog(Profile profile)
        {
            _profile = profile;
            this.InitializeComponent();
            Analytics.TrackEvent("ProfileShared");
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private async void ShareProfileDialog_OnLoading(FrameworkElement sender, object args)
        {
            var bson = ProfileHelper.ProfileToBson(_profile);

            var formContent = new HttpFormUrlEncodedContent(new[] {
                new KeyValuePair<string, string>("token", Secrets.ForecastProfileHostServiceSecret),
                new KeyValuePair<string, string>("bson", bson)
            });

            HttpResponseMessage response;
            try
            {
                response = await AppWebClient.Client.PostAsync(new Uri("https://forecast.ethanbrews.me/api/v1/submit/"), formContent);
            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex);
                //TODO still export the file
                return;
            }

            if (!response.IsSuccessStatusCode)
            {
                await new ContentDialog { Title = "Error", PrimaryButtonText = "Close", Content = "HTTP Error Code: " + response.StatusCode.ToString() }.ShowAsync();
                System.Diagnostics.Debug.WriteLine("Connection to ForecastProfileHostService returned HTTP status code: " + response.StatusCode);
                return;
            }

            var res = Data.ForecastWebApp.ForecastSuccessResponse.FromJson(await response.Content.ReadAsStringAsync());

            ProgRing.IsActive = false;
            CodeBlock.Text = res.Success.PackCode.ToUpper();
            GettingCodeMessage.Visibility = Visibility.Collapsed;
            CopiedCodeMessage.Visibility = Visibility.Visible;

            var dataPackage = new DataPackage();
            dataPackage.RequestedOperation = DataPackageOperation.Copy;
            dataPackage.SetText(res.Success.PackCode.ToUpper());
            Clipboard.SetContent(dataPackage);

        }
    }
}
