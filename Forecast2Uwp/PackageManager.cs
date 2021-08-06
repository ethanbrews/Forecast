using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Forecast2Uwp.Helpers;
using Forecast2Uwp.Thunderstore.Data.Profile;
using Forecast2Uwp.Thunderstore.Data;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.System;
using Forecast2Uwp.Download;

namespace Forecast2Uwp
{

    static class PackageManager
    {
        private static bool _auto_update_file = true;
        private static readonly string FileName = "profiles.json";
        private static readonly StorageFolder RootFolder = ApplicationData.Current.LocalFolder;
        private static readonly string[] BannedFiles = (Application.Current as App).Config?.ProfileInstaller?.IgnoreFiles ?? new string[] { "icon.png", "manifest.json", "README.md" };
        private static readonly string[] IncludeInRorRoot = (Application.Current as App).Config?.ProfileInstaller?.FileSpecifiers?.Ror2 ?? new string[] { "winhttp.dll", "BepInEx" };
        private static readonly string[] IncludeInBepisRoot = (Application.Current as App).Config?.ProfileInstaller?.FileSpecifiers?.Bepis ?? new string[] { "plugins", "config" };
        public static ObservableCollection<Profile> Profiles { get; set; } = new ObservableCollection<Profile>();

        public delegate void DownloadingFilesEventArgs(int queueSize);
        public static event DownloadingFilesEventArgs DownloadingFilesEvent;

        static PackageManager()
        {
            Profiles.CollectionChanged += Profiles_CollectionChanged;
        }

        // Save profiles to file on collection changed, except when _auto_update_file is false (to prevent Profiles.Clear() from deleting file) 
        private static void Profiles_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (_auto_update_file) {
                _ = SaveProfilesToFileAsync();
                _ = WriteInstalledPackageManifestToDiskAsync();
            }
        }

        // Get packages used in profiles installed on the machine. This allows the app to run offline.
        public static Thunderstore.Data.Package[] GetInstalledPackages() =>
            Profiles.Where(x => x != null).SelectMany(x => x.Packages).ToArray();
        

        // Write Package[] of installed packages to disk
        public async static Task WriteInstalledPackageManifestToDiskAsync()
        {
            try
            {
                var localPackagesFile = await StorageHelper.GetLocalFileAsync("packages.local.json");
                await FileIO.WriteTextAsync(localPackagesFile, GetInstalledPackages().ToJson());
            } catch(Exception ex)
            {
                Analytics.TrackError(ex);
            }
            
        }

        // Read Package[] of installed packages from disk. Return empty array on error.
        public async static Task<Package[]> GetInstalledPackagesManifestAsync()
        {
            try
            {
                var localPackagesFile = await StorageHelper.GetLocalFileAsync("packages.local.json");
                return Package.FromJson(await FileIO.ReadTextAsync(localPackagesFile));
            }
            catch (Exception ex)
            {
                Analytics.TrackError(ex);
                return new Package[0];
            }
        }
            
        // Install mods for a given profile. Requires an internet connection
        public static async Task InstallModsForProfile(Profile profile)
        {
            var modsRoot = await StorageHelper.GetLocalFolderAsync("mods");
            for (var i = 0; i < profile.Packages.Length; i++)
            {
                DownloadingFilesEvent?.Invoke(profile.Packages.Length - (i+1));
                var pkg = profile.Packages[i];
                try {
                    var version = pkg.Versions[0];
                    var folder = await StorageHelper.GetFolderAsync(modsRoot, $"{pkg.Owner}/{pkg.Name}/{version.VersionNumber}");
                    if ((await folder.GetItemsAsync()).Count > 0)
                        continue;
                    var tempZip = await StorageHelper.GetTemporaryFileAsync("zip");
                    await DownloadManager.GetFileAsync(version.DownloadUrl, tempZip);
                    StorageHelper.ExtractZipArchiveToDirectory(tempZip, folder);
                } catch (Exception ex)
                {
                    Analytics.TrackError(ex, properties: new Dictionary<string, string> {
                        { "ModName", pkg.FullName }
                    });
                }
            }
        }

        // Save all profiles to file
        public static async Task SaveProfilesToFileAsync()
        {
            try { await FileIO.WriteTextAsync(await StorageHelper.GetFileAsync(RootFolder, FileName), Profiles.ToArray().ToJson()); }
            catch (Exception ex) { Analytics.TrackError(ex); }
        }

        // Read all profiles from file
        public static async Task LoadProfilesFromFileAsync()
        {
            _auto_update_file = false;
            try
            {
                Profiles.Clear();
                if (StorageHelper.DoesFileExist(RootFolder, FileName))
                {
                    var file = await StorageHelper.GetFileAsync(RootFolder, FileName);
                    var text = await FileIO.ReadTextAsync(file);
                    foreach(var p in Profile.FromJson(text))
                    {
                        Profiles.Add(p);
                    }
                }
            }
            catch (Exception ex)
            {
                Analytics.TrackError(ex);
            }
            _auto_update_file = true;
        }

        // Launch a profile. May use steam or Full Trust app depending on user configuration.
        public static async Task LaunchProfileAsync(Profile profile)
        {
            var modsRoot = await StorageHelper.GetLocalFolderAsync("mods");
            var rorFolder = await Options.GetRiskOfRainFolderAsync();
            var bepInEx = await StorageHelper.GetFolderAsync(rorFolder, "BepInEx");
            var pluginsFolder = await StorageHelper.GetFolderAsync(bepInEx, "plugins");

            //Clear plugins...
            foreach (var item in (await pluginsFolder.GetItemsAsync()))
            {
                await item.DeleteAsync();
            }

            if (StorageHelper.DoesFileExist(rorFolder, "winhttp.dll"))
                await (await StorageHelper.GetFileAsync(rorFolder, "winhttp.dll")).DeleteAsync();

            //Loop through all packages and copy them over.
            foreach (var pkg in profile.Packages)
            {
                var version = pkg.Versions[0]; //TODO: This may load the wrong version!
                var folder = await StorageHelper.GetFolderAsync(modsRoot, $"{pkg.Owner}/{pkg.Name}/{version.VersionNumber}");

                //Packages may be contained within another folder
                if (StorageHelper.DoesFolderExist(folder, pkg.Name))
                    folder = await StorageHelper.GetFolderAsync(folder, pkg.Name);

                if ((await folder.GetItemsAsync()).Any(x => IncludeInBepisRoot.Contains(x.Name)))
                {
                    await StorageHelper.CopyFolderAsync(folder, bepInEx, StorageHelper.CopyCollisionOptions.SkipExisting, (IStorageItem item) => BannedFiles.Contains(item.Name));
                } else if ((await folder.GetItemsAsync()).Any(x => IncludeInRorRoot.Contains(x.Name)))
                {
                    await StorageHelper.CopyFolderAsync(folder, rorFolder, StorageHelper.CopyCollisionOptions.SkipExisting, (IStorageItem item) => BannedFiles.Contains(item.Name));
                } else
                {
                    await StorageHelper.CopyFolderAsync(folder, pluginsFolder, StorageHelper.CopyCollisionOptions.SkipExisting, (IStorageItem item) => BannedFiles.Contains(item.Name));
                }
            }

            if (Options.LaunchDirectly.Value)
                _ = Win32Bridge.RunProgramAsync((await StorageHelper.GetFileAsync(rorFolder, "Risk of Rain 2.exe")).Path);
            else
                _ = Launcher.LaunchUriAsync(new Uri("steam://run/632360"));

        }

        // Check all profiles for updates. If AutoUpdate is selected, automatically find the latest version of the given package on thunderstore.
        public static async Task CheckForDownloadsAsync()
        {
            foreach (var pro in Profiles)
            {
                if (pro is null)
                    continue;
                if (pro.AutoUpdate)
                {
                    //TODO: Automatically update mods to the latest version from thunderstore;
                }
                await InstallModsForProfile(pro);
            }
        }
    }
}
