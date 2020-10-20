using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using ForecastUWP.Data;
using ForecastUWP.Helpers;
using Microsoft.AppCenter.Analytics;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using Microsoft.Xaml.Interactions.Media;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ForecastUWP.Dialogs
{
    public sealed partial class ImportProfileDialog : ContentDialog
    {

        static readonly Regex Validator = new Regex(@"^[A-Z0-9]+$");
        private Profile _downloadedProfile = null;
        public Profile DownloadedProfile { get; set; }
        private string CodeText = "";
        private bool IsControlDown = false;
        private Storyboard BlinkingElementStoryboard = null;

        public ImportProfileDialog()
        {
            this.InitializeComponent();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            System.Diagnostics.Debug.Assert(_downloadedProfile != null);
            DownloadedProfile = _downloadedProfile;
            Analytics.TrackEvent("PackImported");
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        { 
            //TODO: Import from file
        }

        private async void SearchForPackByCodeAndInstall()
        {
            void SetFailed(string message)
            {
                ErrorBlock.Text = message;
                SearchingFCHostBar.IsIndeterminate = false;
                SearchingFCHostBar.ShowError = true;
                SearchingFCHostBar.Opacity = 0;
                SuccessBlock.Text = "";
            }

            IsPrimaryButtonEnabled = false;
            SearchingFCHostBar.IsIndeterminate = true;
            SearchingFCHostBar.Opacity = 1;

            var txt = CodeText;
            if (txt.Replace(" ", "").Length < 5)
            {
                SetFailed("Invalid code");
                return;
            }
            var response = await AppWebClient.Client.GetAsync(new Uri($"https://forecast.ethanbrews.me/api/v1/find/{txt}/"));
            if (!response.IsSuccessStatusCode)
            {
                SetFailed("No profile could be found for the given code.");
                return;
            }

            var bson = await response.Content.ReadAsStringAsync();
            _downloadedProfile = ProfileHelper.ProfileFromBson(bson);
            SearchingFCHostBar.IsIndeterminate = false;
            IsPrimaryButtonEnabled = true;
            SearchingFCHostBar.Opacity = 0;
            ErrorBlock.Text = "";
            SuccessBlock.Text = "Found " + _downloadedProfile.Name;
        }

        private void UpdateTextBlocksForCode()
        {
            var blocks = CodeBoxesPanel.FindChildren<TextBlock>().ToList();
            for (var i = 0; i < 5; i++)
            {
                blocks[i].Text = (CodeText.Length > i ? CodeText[i].ToString() : "_");
            }

            if (CodeText.Length == 5)
                return;

            if (BlinkingElementStoryboard != null)
                BlinkingElementStoryboard.Stop();

            var animation1 = new DoubleAnimation
            {
                To = 0.1,
                From = 1.0,
                Duration = TimeSpan.FromSeconds(0.75),
                FillBehavior = FillBehavior.HoldEnd,
                AutoReverse = true
            };

            var elementToBlink = blocks[CodeText.Length];
            Storyboard.SetTarget(animation1, elementToBlink);
            Storyboard.SetTargetProperty(animation1, "Opacity");

            BlinkingElementStoryboard = new Storyboard
            {
                Duration = TimeSpan.FromSeconds(1.6),
                RepeatBehavior = RepeatBehavior.Forever
            };

            BlinkingElementStoryboard.Children.Add(animation1);
            BlinkingElementStoryboard.Begin();

        }

        private void PasteIntoBoxes(string text)
        {
            if (text.Length == 5 && Validator.IsMatch(text))
            {
                CodeText = text;
                SearchForPackByCodeAndInstall();
                UpdateTextBlocksForCode();
                if (BlinkingElementStoryboard?.GetCurrentState() == ClockState.Active)
                    BlinkingElementStoryboard?.Stop();
            }
            
        }

        private async void ImportProfileDialog_OnLoaded(object sender, RoutedEventArgs e)
        {
            DataPackageView dataPackageView = Clipboard.GetContent();
            if (dataPackageView.Contains(StandardDataFormats.Text))
            {
                string text = await dataPackageView.GetTextAsync();
                PasteIntoBoxes(text);
            }
            UpdateTextBlocksForCode();
        }

        private static async Task<string> GetClipboardAsync()
        {
            DataPackageView dataPackageView = Clipboard.GetContent();
            if (dataPackageView.Contains(StandardDataFormats.Text))
            {
                string text = await dataPackageView.GetTextAsync();
                return text;
            }
            return "";
        }

        private async void ImportProfileDialog_OnKeyUp(object sender, KeyRoutedEventArgs e)
        {
            switch (e.Key)
            {
                case VirtualKey.Control:
                    IsControlDown = false;
                    return;
                case VirtualKey.Back:
                    CodeText = CodeText.Substring(0, (CodeText.Length == 0 ? 0 : CodeText.Length - 1));
                    UpdateTextBlocksForCode();
                    return;
            }

            string value;
            try
            {
                value = VirtualKeyHelper.VirtualKeyToAlphanumericCharacter(e.Key);
            }
            catch
            {
                return;
            }

            if (IsControlDown && value == "V")
                PasteIntoBoxes(await GetClipboardAsync());
            else if (!IsControlDown && CodeText.Length < 5)
                CodeText += value;

            UpdateTextBlocksForCode();

            if (CodeText.Length == 5)
                SearchForPackByCodeAndInstall();
        }

        private void ImportProfileDialog_OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Control)
                IsControlDown = true;
        }
    }
}
