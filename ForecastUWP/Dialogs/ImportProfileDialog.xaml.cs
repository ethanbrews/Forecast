using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
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
using ForecastUWP.Data;
using ForecastUWP.Helpers;
using Microsoft.Toolkit.Uwp.UI.Extensions;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ForecastUWP.Dialogs
{
    public sealed partial class ImportProfileDialog : ContentDialog
    {

        static readonly Regex Validator = new Regex(@"^[A-Z0-9]+$");
        public Profile DownloadedProfile = null;

        public ImportProfileDialog()
        {
            this.InitializeComponent();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) => System.Diagnostics.Debug.Assert(DownloadedProfile != null);

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        { 
            //TODO: Import from file
        }

        private string GetTextFromBoxes()
        {
            var s = "";
            foreach (var box in CodeBoxesPanel.FindChildren<TextBox>())
            {
                s += box.Text;
            }
            return s;
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

            var txt = GetTextFromBoxes();
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
            DownloadedProfile = ProfileHelper.ProfileFromBson(bson);
            SearchingFCHostBar.IsIndeterminate = false;
            IsPrimaryButtonEnabled = true;
            SearchingFCHostBar.Opacity = 0;
            ErrorBlock.Text = "";
            SuccessBlock.Text = "Found " + DownloadedProfile.Name;
        }

        private void CodeBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            var box = sender as TextBox;
            var index = Int16.Parse(box.Tag as string);
            var nextBox = (index == 4 ? null : CodeBoxesPanel.FindChildren<TextBox>().ToList()[index + 1]);
            if (box.Text.Length > 0)
                box.Text = box.Text.Substring(box.Text.Length - 1, 1).ToUpper();
            if (!Validator.IsMatch(box.Text)) 
                box.Text = "";
            if (nextBox != null && box.Text != "")
            {
                nextBox.Focus(FocusState.Keyboard);
                nextBox.SelectionStart = nextBox.Text.Length;
            }
            else if (box.Text != "")
                SearchForPackByCodeAndInstall();
        }

        private void PasteIntoBoxes(string text)
        {
            if (text.Length == 5 && Validator.IsMatch(text))
            {
                var boxes = CodeBoxesPanel.FindChildren<TextBox>().ToList();
                for (int i = 0; i < 5; i++)
                {
                    boxes[i].Text = text[i].ToString();
                }
                SearchForPackByCodeAndInstall();
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
        }

        private async void TextBox_OnPaste(object sender, TextControlPasteEventArgs e)
        {
            e.Handled = true;
            DataPackageView dataPackageView = Clipboard.GetContent();
            if (dataPackageView.Contains(StandardDataFormats.Text))
            {
                string text = await dataPackageView.GetTextAsync();
                PasteIntoBoxes(text);
            }
        }
    }
}
