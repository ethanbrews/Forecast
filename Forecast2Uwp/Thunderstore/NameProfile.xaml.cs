using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Forecast2Uwp.Thunderstore
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NameProfile : Page, INotifyPropertyChanged
    {

        public ThunderstorePageViewModel ViewModel { get; set; }
        public bool Error_TooShort { get; set; } = false;
        public bool Error_Unique { get; set; } = false;
        public bool Error_SpecialChars { get; set; } = false;
        public bool IsSuccess => !(Error_TooShort || Error_Unique || Error_SpecialChars);
        public bool IsError => Error_TooShort || Error_Unique || Error_SpecialChars;

        public NameProfile()
        {
            InitializeComponent();
            DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ViewModel = e.Parameter as ThunderstorePageViewModel;
            TextBox_TextChanged(null, null);
        }

        public static bool IsProfileNameValid(string text) =>
            text.Length > 3 && 
            text.All(c => "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789_-. ".Contains(c)) && 
            text != "Vanilla" &&
            !PackageManager.Profiles.Any(x => x.Name == text);

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Error_TooShort = false;
            Error_Unique = false;
            Error_SpecialChars = false;
            if (NameTextBox.Text.Length < 4)
                Error_TooShort = true;
            else if (!NameTextBox.Text.All(c => "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789_-. ".Contains(c)))
                Error_SpecialChars = true;
            else if (PackageManager.Profiles.Any(x => x.Name == NameTextBox.Text) || NameTextBox.Text == "Vanilla")
                Error_Unique = true;

            foreach (var prop in new string[] { "Error_TooShort", "Error_Unique", "Error_SpecialChars", "IsError", "IsSuccess" })
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var profile = new Data.Profile.Profile
            {
                AutoUpdate = AutoUpdateCheckbox.IsChecked.GetValueOrDefault(true),
                Name = NameTextBox.Text,
                PackageFullNames = (from pkg in ViewModel.AllSelectedPackages select pkg.Versions[0].FullName).ToArray()
            };
            PackageManager.Profiles.Add(profile);
            _ = PackageManager.InstallModsForProfile(profile);
            _ = MainPage.Current.ContentFrame.Navigate(typeof(Play.Play), null, new Windows.UI.Xaml.Media.Animation.SlideNavigationTransitionInfo { Effect = Windows.UI.Xaml.Media.Animation.SlideNavigationTransitionEffect.FromLeft });
        }
    }
}
