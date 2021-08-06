using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
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

namespace Forecast2Uwp.Settings
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {

        public string PackageFamily => Package.Current.Id.FamilyName;

        public string PackageArchitecture => Package.Current.Id.Architecture.ToString().ToLower();

        public string PackageName => Package.Current.DisplayName;

        public string PackageEffectivePath => Package.Current.EffectivePath;

        public bool LaunchMethod {
            get => Options.LaunchDirectly.Value;
            set => Options.LaunchDirectly.Value = value;
        }

        public bool TrackCrashes
        {
            get => Options.TrackCrashes.Value;
            set => Options.TrackCrashes.Value = value;
        }

        public bool TrackEvents
        {
            get => Options.TrackEvents.Value;
            set => Options.TrackEvents.Value = value;
        }

        public bool TrackErrors
        {
            get => Options.TrackErrors.Value;
            set => Options.TrackErrors.Value = value;
        }


        public SettingsPage()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
            LaunchMethod = !(e.AddedItems.Any(x => x.ToString().Contains("Steam")));
    }
}
