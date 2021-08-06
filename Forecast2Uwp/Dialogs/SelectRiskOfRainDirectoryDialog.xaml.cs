using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
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
    public sealed partial class SelectRiskOfRainDirectoryDialog : ContentDialog, INotifyPropertyChanged
    {

        public StorageFolder SelectedFolder { get; set; } = null;

        public string SelectedFolderPath => SelectedFolder?.Path ?? "No folder selected";
        public bool FolderIsSelected => SelectedFolder != null;
        public bool RiskOfRainExeMissing { get; set; } = false;

        public SelectRiskOfRainDirectoryDialog()
        {
            this.InitializeComponent();
            this.DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            Options.SetRiskOfRainFolder(SelectedFolder);
            this.Hide();
        }

        private async void FolderButton_Click(object sender, RoutedEventArgs e)
        {
            var folderPicker = new Windows.Storage.Pickers.FolderPicker();
            folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.ComputerFolder;
            folderPicker.FileTypeFilter.Add("*");

            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                // Application now has read/write access to all contents in the picked folder
                // (including other sub-folder contents)
                Windows.Storage.AccessCache.StorageApplicationPermissions.
                FutureAccessList.AddOrReplace("PickedFolderToken", folder);
                Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(folder);
                SelectedFolder = folder;

                RiskOfRainExeMissing = !Helpers.StorageHelper.DoesFileExist(folder, @"Risk of Rain 2.exe");
            }

            foreach (var prop in new string[] { "SelectedFolder", "SelectedFolderPath", "FolderIsSelected", "RiskOfRainExeMissing" })
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
            }
            
        }
    }
}
