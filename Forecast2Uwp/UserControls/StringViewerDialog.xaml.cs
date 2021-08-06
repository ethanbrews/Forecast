using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
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

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Forecast2Uwp.UserControls
{
    public sealed partial class StringViewerDialog : ContentDialog, INotifyPropertyChanged
    {

        public bool ShowCopiedText { get; set; } = false;

        public StringViewerDialog(string text, string title, string fontFamily = "Segoe UI")
        {
            this.InitializeComponent();
            DataContext = this;
            Text.FontFamily = new FontFamily(fontFamily);
            Text.Text = text;
            Title.Text = title;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            DataPackage dataPackage = new DataPackage
            {
                RequestedOperation = DataPackageOperation.Copy
            };
            dataPackage.SetText(Text.Text);
            Clipboard.SetContent(dataPackage);
            ShowCopiedText = true;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ShowCopiedText"));
            _ = Helpers.ThreadHelper.RunOnBackgroundThreadAsync(() =>
            {
                Thread.Sleep(3000);
                _ = Helpers.ThreadHelper.RunOnUIThreadAsync(() =>
                {
                    ShowCopiedText = false;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ShowCopiedText"));
                });
            });
        }


        private void HideButton_Click(object sender, RoutedEventArgs e) => Hide();


    }
}
