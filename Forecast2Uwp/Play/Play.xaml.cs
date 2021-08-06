using Forecast2Uwp.Download;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Forecast2Uwp.Play
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Play : Page, INotifyPropertyChanged
    {

        public ObservableCollection<Thunderstore.Data.Profile.Profile> Profiles { get; set; }

        public bool ShareButtonEnabled => (Application.Current as App).Config.Features.ShareProfilesEnabled.IsEnabled;
        public bool EditButtonEnabled => (Application.Current as App).Config.Features.EditProfilesEnabled.IsEnabled;
        public bool UpdateButtonEnabled => (Application.Current as App).Config.Features.UpdateProfilesEnabled.IsEnabled;

        public static ObservableCollection<Thunderstore.Data.Share.PackRecentsResponse> RecentlyShared { get; set; } = new ObservableCollection<Thunderstore.Data.Share.PackRecentsResponse>();


        public Play()
        {
            InitializeComponent();
            DataContext = this;
            Profiles = new ObservableCollection<Thunderstore.Data.Profile.Profile>(PackageManager.Profiles);
            Profiles.Insert(0, new Thunderstore.Data.Profile.Profile
            {
                AutoUpdate = false,
                PackageFullNames = new string[0],
                Name = "Vanilla"
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Profiles"));
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            btn.IsEnabled = false;
            _ = PackageManager.LaunchProfileAsync(btn.DataContext as Thunderstore.Data.Profile.Profile);
            _ = Helpers.ThreadHelper.RunOnBackgroundThreadAsync(() =>
            {
                Thread.Sleep(5000);
                _ = Helpers.ThreadHelper.RunOnUIThreadAsync(() => btn.IsEnabled = true);
                
            });
        }
            

        private void EditModsButton_Click(object sender, RoutedEventArgs e)
        {
            //profile.ShareCode = null;
        }

        private void EditNameButton_Click(object sender, RoutedEventArgs e)
        {
            var profile = (sender as FrameworkElement).DataContext as Thunderstore.Data.Profile.Profile;
            var textbox = ((sender as FrameworkElement).Parent as Panel).Children.First(x => (x as FrameworkElement).Name == "ProfileNameBox") as TextBox;
            if (Thunderstore.NameProfile.IsProfileNameValid(textbox.Name))
            {
                profile.ShareCode = null;
                profile.Name = textbox.Text;
                profile.ChangedProperty("Name");
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var profile = (sender as FrameworkElement).DataContext as Thunderstore.Data.Profile.Profile;
            Profiles.Remove(profile);
            PackageManager.Profiles.Remove(profile);
        }

        private async void ShareButton_Click(object sender, RoutedEventArgs e)
        {
            var profile = (sender as FrameworkElement).DataContext as Thunderstore.Data.Profile.Profile;

            if (profile.ShareCode != null)
            {
                await new Dialogs.PackSharedDialog(profile.ShareCode, profile.Name).ShowAsync();
                return;
            }

            try
            {
                var client = DownloadManager.CachingClient;

                var content = new Windows.Web.Http.HttpFormUrlEncodedContent(new Dictionary<string, string>
            {
                { "name", profile.Name },
                { "profile", JsonConvert.SerializeObject(profile) }
            });

                var res = await client.PostAsync(new Uri(new Uri(Options.ShareEndpoint.Value), "submit/"), content);
                Analytics.Log(res.ToString());
                if (res.IsSuccessStatusCode)
                {
                    var shareResponse = JsonConvert.DeserializeObject<Thunderstore.Data.Share.PackSharedResponse>(await res.Content.ReadAsStringAsync());
                    var p2 = PackageManager.Profiles.First(x => x == profile);
                    profile.ShareCode = shareResponse.Success.PackCode;
                    p2.ShareCode = shareResponse.Success.PackCode;
                    await new Dialogs.PackSharedDialog(shareResponse.Success.PackCode, profile.Name).ShowAsync();
                }
                else
                {
                    Analytics.TrackError(new Exception($"Failed to share pack Response = {{ Code = {res.StatusCode}, Reason = {{ {res.ReasonPhrase} }}, Request = {{ {res.RequestMessage} }}, Source = {{ {res.Source} }} }}"));
                    await new Dialogs.GenericMessage("Error sharing profile", $"An error occurred when sharing the pack (HTTP response {res.IsSuccessStatusCode}). If error logging is enabled, this error will be automatically reported.").ShowAsync();
                }
            } catch (Exception ex)
            {
                Analytics.TrackError(ex);
            }
            
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var profile = btn.DataContext as Thunderstore.Data.Profile.Profile;
            //TODO: Automatically update version of mods in the profile
            btn.IsEnabled = false;
            PackageManager.DownloadingFilesEvent += (int queueSize) =>
                btn.IsEnabled = queueSize == 0;
            _ = PackageManager.InstallModsForProfile(profile);
        }

        private async void Page_Loading(FrameworkElement sender, object args)
        {
            if (RecentlyShared.Count != 0)
                return;
            Thunderstore.Data.Share.PackRecentsResponse[] recents;
            try
            {
                recents = await DownloadManager.GetObjectAsync<Thunderstore.Data.Share.PackRecentsResponse[]>(
                    new Uri(new Uri(Options.ShareEndpoint.Value), "recents/"),
                    x => Thunderstore.Data.Share.PackRecentsResponse.FromJson(x)
                );
            } catch (Exception ex)
            {
                Analytics.TrackError(ex);
                return;
            }
            foreach(var p in recents)
            {
                RecentlyShared.Add(p);
            }
            
        }

        private async void ImportProfileButton_Click(object sender, RoutedEventArgs e)
        {
            //DataContext may be null!
            var context = (sender as FrameworkElement).DataContext as Thunderstore.Data.Share.PackRecentsResponse;
            string code = null;
            if (context != null)
            {
                code = context.Code;
            }
            await new Dialogs.ImportProfile(code.ToUpper()).ShowAsync();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Profiles"));
        }
    }
}
