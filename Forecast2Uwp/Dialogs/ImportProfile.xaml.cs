using Forecast2Uwp.Download;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Forecast2Uwp.Dialogs
{
    public sealed partial class ImportProfile : ContentDialog
    {
        private int cursorPos = 0;
        private TextBlock[] items;

        private Thunderstore.Data.Profile.Profile _profile;
        public Thunderstore.Data.Profile.Profile Profile
        {
            get => _profile;
            private set
            {
                _profile = value;
                IsSecondaryButtonEnabled = value != null;
            }
        }

        public string Code
        {
            get => string.Join("", items.Select(x => x.Text));
            set
            {
                if (value == null)
                {
                    foreach (TextBlock i in items)
                    {
                        i.Text = "-";
                    }
                }
                if (value.Length > 5)
                {
                    value = value.Substring(0, 5);
                }
                for (var i = 0; i < 5; i++)
                {
                    items[i].Text = i < value.Length ? value[i].ToString() : "-";
                }
            }
        }


        public ImportProfile(string code = null)
        {
            this.InitializeComponent();
            items = new TextBlock[] { TB1, TB2, TB3, TB4, TB5 };
            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
            if (code != null)
            {
                Code = code;
                CheckForProfileWithCode();
            }
        }

        private async void CheckForProfileWithCode()
        {
            try
            {
                Profile = JsonConvert.DeserializeObject<Thunderstore.Data.Profile.Profile>(
                    await DownloadManager.GetStringAsync(
                        new Uri(new Uri(Options.ShareEndpoint.Value), $"find/{Code}/")
                    )
                );
                ProfileName.Text = Profile.Name;
            } catch (Exception ex)
            {
                Analytics.TrackError(ex);
                ProfileName.Text = "No Profile Found";
                Profile = null;
            }
        }

        private new void Hide()
        {
            base.Hide();
            Window.Current.CoreWindow.KeyDown -= CoreWindow_KeyDown;
        }

        private void CoreWindow_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            Analytics.Log(((char)args.VirtualKey).ToString());
            if (args.VirtualKey == Windows.System.VirtualKey.Back)
            {
                ProfileName.Text = "";
                Profile = null;
                items[cursorPos].Text = "-";
                cursorPos = Math.Max(cursorPos - 1, 0);
            }
            if (cursorPos == 4 && items[4].Text != "-") 
                return;

            var keyName = ((char)args.VirtualKey).ToString();
            keyName = keyName.Substring(keyName.Length - 1, 1);

            if ("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".Contains(keyName))
            {
                ProfileName.Text = "";
                Profile = null;
                items[cursorPos].Text = keyName;

                if (cursorPos == 4)
                    CheckForProfileWithCode();

                cursorPos = Math.Min(4, cursorPos + 1);
            }
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) => Hide();

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (Profile != null)
            {
                PackageManager.Profiles.Add(Profile);
                _ = PackageManager.InstallModsForProfile(Profile);
            }
            Hide();
        }
            
    }
}
