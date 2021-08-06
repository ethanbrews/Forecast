using Forecast2Uwp.Download;
using Forecast2Uwp.RemoteConfig;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Forecast2Uwp.Settings
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Debug : Page
    {

        public string RemoteConfigJson => (Application.Current as App).Config.ToJson();
        public string RemoteConfigUrl => Options.RemoteConfigUrl.Value;

        public string LocalSettings => string.Join('\n', ApplicationData.Current.LocalSettings.Values.Select(x => $"{x.Key} = {x.Value}"));


        public Debug()
        {
            this.InitializeComponent();
            DataContext = this;
        }

        private async void ReloadRemoteConfigButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.IsEnabled = false;
            try
            {
                (Application.Current as App).Config = await DownloadManager.GetObjectAsync(
                    new Uri(Options.RemoteConfigUrl.Value),
                    s => Config.FromJson(s),
                    false
                );
            }
            catch (Exception ex)
            {
                Analytics.TrackError(ex);
                (Application.Current as App).Config = null;
            }
            this.Frame.IsEnabled = true;
        }

        private void UpdateRemoteConfigUrlButton_Click(object sender, RoutedEventArgs e) =>
            Options.RemoteConfigUrl.Value = RemoteConfigUrlBox.Text;

        private void ResetRemoteConfigUrlButton_Click(object sender, RoutedEventArgs e) =>
            Options.RemoteConfigUrl.ResetToDefault();

        private void RemoteConfigUrlBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var text = RemoteConfigUrlBox.Text;
            var regex = new Regex(@"^(http|https|ftp|ftps|file)\:\/\/(\/?)[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&amp;%\$#_]*)?$");
            UpdateRemoteConfigUrlButton.IsEnabled = regex.IsMatch(text);
        }
    }
}
