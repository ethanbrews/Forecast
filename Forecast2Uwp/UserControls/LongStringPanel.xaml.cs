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

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Forecast2Uwp.UserControls
{
    public sealed partial class LongStringPanel : UserControl
    {

        [Description("Text displayed in the textblock"), Category("Data")]
        public string Text
        { 
            get => (string)GetValue(textProperty);
            set => SetValue(textProperty, value);
        }

        [Description("Text displayed next to line count"), Category("Data")]
        public string Caption
        {
            get => (string)GetValue(captionProperty) ?? "";
            set => SetValue(captionProperty, value);
        }

        public int NLines => (Text ?? "").Split('\n').Length;

        [Description("Title to be shown in content dialog"), Category("Data")]
        public string Title
        {
            get => (string)GetValue(titleProperty);
            set => SetValue(titleProperty, value);
        }

        public static readonly DependencyProperty textProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(LongStringPanel), null);

        public static readonly DependencyProperty titleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(LongStringPanel), null);

        public static readonly DependencyProperty captionProperty =
            DependencyProperty.Register("Caption", typeof(string), typeof(LongStringPanel), null);

        public LongStringPanel()
        {
            InitializeComponent();
            DataContext = this;
            TextPreview.Text = Text ?? "";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _ = new StringViewerDialog(Text, Title, "Consolas").ShowAsync();
        }
    }
}
