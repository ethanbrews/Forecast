using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
using static System.Net.Mime.MediaTypeNames;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Forecast2Uwp.Dialogs
{
    public sealed partial class PackSharedDialog : ContentDialog
    {

        public string ShareCode { get; set; }
        public string PackTitle { get; set; }

        public PackSharedDialog(string code, string packName)
        {
            InitializeComponent();

            ShareCode = code.ToUpper();
            PackTitle = packName;

            DataContext = this;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Hide();

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            DataPackage dataPackage = new DataPackage
            {
                RequestedOperation = DataPackageOperation.Copy
            };
            dataPackage.SetText(ShareCode);
            Clipboard.SetContent(dataPackage);
            Hide();
        }
    }
}
