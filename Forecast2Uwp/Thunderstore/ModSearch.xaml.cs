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
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Forecast2Uwp.Thunderstore
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ModSearch : Page
    {

        public ThunderstorePageViewModel ViewModel = new ThunderstorePageViewModel();

        public ModSearch()
        {
            this.InitializeComponent();
            this.DataContext = ViewModel;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ViewModel.FilterPackages((sender as TextBox).Text, CategoryBox.SelectedValue as string);
        }

        private void SelectModButton_Click(object sender, RoutedEventArgs e)
        {
            var pkg = (sender as Button).DataContext as Data.Package;
            ViewModel.SelectPackage(pkg);
            ViewModel.FilterPackages(SearchBox.Text, CategoryBox.SelectedValue as string);
        }

        private void DeselectButton_Click(object sender, RoutedEventArgs e)
        {
            var pkg = (sender as FrameworkElement).DataContext as Data.Package;
            ViewModel.DeselectPackage(pkg);
            if (ViewModel.AllSelectedPackages.Count == 0)
                SelectedPackagesFlyout.Hide();
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(NameProfile), ViewModel, new Windows.UI.Xaml.Media.Animation.SlideNavigationTransitionInfo
            {
                Effect = Windows.UI.Xaml.Media.Animation.SlideNavigationTransitionEffect.FromRight
            });
        }
    }
}
