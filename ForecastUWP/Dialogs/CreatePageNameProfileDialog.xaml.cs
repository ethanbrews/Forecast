using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
using ForecastUWP.Data;
using ForecastUWP.Data.Thunderstore.Packages;
using ForecastUWP.Helpers;
using ForecastUWP.Pages;
using Microsoft.AppCenter.Analytics;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ForecastUWP.Dialogs
{
    public sealed partial class CreatePageNameProfileDialog : ContentDialog
    {
        private List<Package> _packagesToInstall { get; set; }
        private Frame _frame;

        public CreatePageNameProfileDialog(List<Package> packagesToInstall, Frame frame, string suggestedName = null)
        {
            _packagesToInstall = packagesToInstall;
            _frame = frame;
            this.InitializeComponent();
            ModCountRun.Text = _packagesToInstall.Count.ToString();
            if (suggestedName != null)
            {
                ProfileNameBox.Text = suggestedName;
                NameBox_KeyUp(null, null);
            }
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            ProfileNameTextBlock.Text = ProfileNameBox.Text;
            await ProfileHelper.AddProfileAsync(new Profile
                {Name = ProfileNameBox.Text, Packages = _packagesToInstall.ToArray()});
            ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("ForwardConnectedAnimation", ProfileNameTextBlock);
            _frame.Navigate(typeof(Pages.Play), new InstallProfilePlayParameter
            {
                ProfileToInstall = new Profile
                {
                    CurrentInstalledTotalToInstallProgress = null,
                    Name = ProfileNameBox.Text,
                    Packages = _packagesToInstall.ToArray()
                }
            }, new SuppressNavigationTransitionInfo());
            Analytics.TrackEvent("NewPackCreated");
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private async void NameBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            OverwriteWarning.Visibility = ((await Helpers.ProfileHelper.GetProfilesAsync()).Where(x => x.Name == ProfileNameBox.Text).FirstOrDefault() == null)
                ? Visibility.Collapsed
                : Visibility.Visible;

            IsPrimaryButtonEnabled = ProfileNameBox.Text.Length > 2 && ProfileNameBox.Text != "Vanilla";
        }
    }
}
