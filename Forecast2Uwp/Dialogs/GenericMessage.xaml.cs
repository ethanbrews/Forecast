using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
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
    public sealed partial class GenericMessage : ContentDialog
    {

        public new string Title { get; set; }
        public string Message { get; set; }
        public string ActionMessage { get; set; }
        public Uri ActionUrl { get; set; }

        public GenericMessage(string title, string message, string actionMessage = null, string actionUrl = null)
        {
            InitializeComponent();
            Title = title;
            Message = message;
            ActionMessage = actionMessage;
            ActionUrl = actionUrl is null ? null : new Uri(actionUrl);
            DataContext = this;

            if (actionMessage is null || actionUrl is null)
            {
                CloseButton.SetValue(Grid.ColumnSpanProperty, 2);
                ActionButton.Visibility = Visibility.Collapsed;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Hide();

        private void ActionButton_Click(object sender, RoutedEventArgs e) => _ = Launcher.LaunchUriAsync(ActionUrl);
    }
}
