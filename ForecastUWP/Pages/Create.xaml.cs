using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using ForecastUWP.Data.Thunderstore.Packages;
using Microsoft.Toolkit.Uwp.UI.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ForecastUWP.Pages
{

    public class CreatePreSelectedPackagesParameters
    {
        public List<Package> PreSelectedPackages = null;
        public List<Package> PreSelectedDependencies = null;
        public string SuggestedName = null;
        public bool SupressModsPrefilledMessage = false;
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Create : Page
    {
        public Create()
        {
            this.InitializeComponent();
            SelectedPackages = new ObservableCollection<Package>();
            SelectedDependencies = new ObservableCollection<Package>();
        }

        public ObservableCollection<Package> SelectedPackages;
        public ObservableCollection<Package> SelectedDependencies;
        public ObservableCollection<Package> AllPackages;

        public CreatePreSelectedPackagesParameters Parameter = null;

        private void Create_OnLoaded(object sender, RoutedEventArgs e)
        {
            foreach (var p in App.ThunderstorePackages)
            {
                p.ImageUri = new Uri($"https://cdn.thunderstore.io/live/repository/icons/{p.Versions.FirstOrDefault().FullName}.png.128x128_q85.png");
            }

            AllPackages = new ObservableCollection<Package>(App.ThunderstorePackages.Take(ApplicationSettings.CreatePageMaxItemsOnPage.Value));
            ModsListPanel.ItemsSource = AllPackages;
            SelectedModsControl.ItemsSource = SelectedPackages;
            SelectedDependenciesControl.ItemsSource = SelectedDependencies;
            if (Parameter != null)
            {
                if (Parameter.PreSelectedPackages != null)
                {
                        foreach (var pkg in Parameter.PreSelectedPackages)
                        {
                            SelectPackage(pkg);
                        }
                }
                if (Parameter.PreSelectedDependencies != null)
                {
                    foreach (var pkg in Parameter.PreSelectedDependencies)
                    {
                        SelectPackage(pkg, true);
                    }
                }

                for (var i = SelectedPackages.Count - 1; i >= 0; i--)
                {
                    if (SelectedDependencies.Where(x => x.Name == SelectedPackages[i].Name).FirstOrDefault() != null)
                        SelectedPackages.RemoveAt(i);
                }

                if (!Parameter.SupressModsPrefilledMessage)
                    MainPage.Current.EnqueueToastNotification("Mods pre-selected", "Some mods have been pre-selected to install.", "\uE946", 16000);
            }

            SetInstallButtonText();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Parameter = e.Parameter as CreatePreSelectedPackagesParameters;
        }

        private void ImageExBase_OnImageExFailed(object sender, ImageExFailedEventArgs e)
        {
            var i = sender as ImageEx;
            if (i.Source == null)
                return;

            var sourceUri = i.Source as Uri;
            var sourceString = sourceUri.OriginalString;
            if (sourceString.EndsWith(".png"))
                i.Source = new Uri(sourceString.Substring(0, sourceString.Length - 3) + "jpg");
            else
                i.Source = null;

            try
            {
                var index = Array.IndexOf(App.ThunderstorePackages, App.ThunderstorePackages.Single(x => x.ImageUri == sourceUri));
                var item = App.ThunderstorePackages[index];
                item.ImageUri = i.Source as Uri;
                App.ThunderstorePackages[index] = item;
            }
            catch {}
        }

        private void InstallButton_OnClick(object sender, RoutedEventArgs e)
        {
            InstallButton.Flyout.ShowAt(InstallButton);
        }

        private List<Package> GetNonSelectedPackages()
        {
            List<Package> Temp = App.ThunderstorePackages.ToList();
            List<Package> AllSelected = new List<Package>(SelectedPackages);
            AllSelected.AddRange(SelectedDependencies);
            foreach (var pk in AllSelected)
            {
                try
                {
                    Temp.Remove(pk);
                } catch {  }
            }

            return Temp;
        }

        private void FilterMods(string searchQuery)
        {
            List<Package> TempFiltered;

            TempFiltered = GetNonSelectedPackages().Where(x =>
                x.FullName.Contains(searchQuery, StringComparison.InvariantCultureIgnoreCase) ||
                x.Owner.Contains(searchQuery, StringComparison.InvariantCultureIgnoreCase)).ToList();

            for (int i = AllPackages.Count - 1; i >= 0; i--)
            {
                var item = AllPackages[i];
                if (!TempFiltered.Contains(item))
                    AllPackages.Remove(item);
            }

            foreach (var item in TempFiltered)
            {
                if (!AllPackages.Contains(item) && AllPackages.Count < ApplicationSettings.CreatePageMaxItemsOnPage.Value)
                    AllPackages.Add(item);
            }

            SetInstallButtonText();
        }

        private void SelectButton_OnClick(object sender, RoutedEventArgs e)
        {
            SelectPackage((sender as Button).DataContext as Package);
        }

        private void SelectPackage(Package ctx, bool SelectAsDependency = false)
        {
            AllPackages.Remove(ctx);
            if (SelectedDependencies.Contains(ctx) || SelectedPackages.Contains(ctx))
                return;
            if (SelectAsDependency)
                SelectedDependencies.Add(ctx);
            else
                SelectedPackages.Add(ctx);
            var latestVersion = ctx.Versions.OrderByDescending(x => x.DateCreated).FirstOrDefault();
            foreach (var dependencyName in latestVersion.Dependencies)
            foreach (var pkg in AllPackages.ToList())
            {
                var ver = pkg.Versions.Where(x => x.FullName == dependencyName).FirstOrDefault();
                if (ver != null && !SelectedDependencies.Contains(pkg) && !SelectedPackages.Contains(pkg))
                    SelectPackage(pkg, true);
            }

            SetInstallButtonText();
        }

        private void SetInstallButtonText()
        {
            ModCountRun.Text = (SelectedDependencies.Count + SelectedPackages.Count).ToString();
            InstallButton.IsEnabled = (SelectedDependencies.Count + SelectedPackages.Count) > 0;
        }

        private void SearchBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            FilterMods((sender as TextBox).Text);
        }

        private void RemoveFromListButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SelectedPackages.Remove((sender as FrameworkElement).DataContext as Package);
            var ToKeep = new List<Package>();
            foreach (var pkg in SelectedPackages)
            {
                foreach (var depName in pkg.Versions.OrderByDescending(x => x.DateCreated).FirstOrDefault()
                    ?.Dependencies)
                {
                    var d = SelectedDependencies
                        .Where(x => x.Versions.Where(y => y.FullName == depName).FirstOrDefault() != null).FirstOrDefault();
                    if (d != null)
                        ToKeep.Add(d);
                }
            }

            for (int i = SelectedDependencies.Count - 1; i >= 0; i--)
            {
                if (!ToKeep.Contains(SelectedDependencies[i]))
                    SelectedDependencies.RemoveAt(i);
            }

            SetInstallButtonText();

            if ((SelectedDependencies.Count + SelectedPackages.Count) == 0)
            {
                InstallButton.IsEnabled = false;
                if (InstallButtonFlyout.IsOpen)
                    InstallButtonFlyout.Hide();
            }
                
        }

        private async void ConfirmInstall_Click(object sender, RoutedEventArgs e)
        {
            var allPkg = new List<Package>(SelectedPackages);
            allPkg.AddRange(SelectedDependencies);
            var dialog = new Dialogs.CreatePageNameProfileDialog(allPkg, Frame, Parameter?.SuggestedName);
            await dialog.ShowAsync();
        }

        private void ModPanel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ConnectedAnimationService
                .GetForCurrentView().PrepareToAnimate("ForwardConnectedAnimation",
                    (sender as FrameworkElement)?.FindName("ModImage") as FrameworkElement);
            
            Frame.Navigate(typeof(ModInformation),
                new ModInformationParameters
                {
                    CreateParameters = new CreatePreSelectedPackagesParameters
                    {
                        PreSelectedDependencies = SelectedDependencies.ToList(),
                        PreSelectedPackages = SelectedPackages.ToList()
                    },
                    PackageToShow = (sender as FrameworkElement).DataContext as Package
                }, new SuppressNavigationTransitionInfo());
        }
        

    }
}
