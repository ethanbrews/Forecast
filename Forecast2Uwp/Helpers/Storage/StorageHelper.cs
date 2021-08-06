using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;

namespace Forecast2Uwp.Helpers
{
    class StorageHelper
    {
        public static async Task<StorageFile> GetOrCreateCachedWebFileAsync(string fileName)
        {
            var folder = await ApplicationData.Current.LocalCacheFolder.CreateFolderAsync("Web", CreationCollisionOption.OpenIfExists);
            return await folder.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);
        }

        public static bool IsWebFileCached(string fileName)
        {
            return File.Exists(Path.Combine(ApplicationData.Current.LocalCacheFolder.Path, "Web", fileName));
        }

        public static bool DoesFileExist(StorageFolder folder, string path)
        {
            var fPath = Path.Combine(folder.Path, path);
            return File.Exists(fPath);
        }

        public static bool DoesFolderExist(StorageFolder folder, string path)
        {
            var fPath = Path.Combine(folder.Path, path);
            return Directory.Exists(fPath);
        }

        public static async Task<StorageFile> GetFileAsync(StorageFolder folder, string path) => await folder.CreateFileAsync(path.Replace("/", "\\"), CreationCollisionOption.OpenIfExists);
        public static async Task<StorageFolder> GetFolderAsync(StorageFolder folder, string path) => await folder.CreateFolderAsync(path.Replace("/", "\\"), CreationCollisionOption.OpenIfExists);

        public static bool DoesLocalFileExist(string path) => DoesFileExist(ApplicationData.Current.LocalFolder, path);
        public static bool DoesCacheFileExist(string path) => DoesFileExist(ApplicationData.Current.LocalCacheFolder, path);

        public static async Task<StorageFile> GetLocalFileAsync(string path) =>
            await GetFileAsync(ApplicationData.Current.LocalFolder, path);
        public static async Task<StorageFolder> GetLocalFolderAsync(string path) =>
            await GetFolderAsync(ApplicationData.Current.LocalFolder, path);

        public static async Task<StorageFile> GetCacheFileAsync(string path) =>
            await GetFileAsync(ApplicationData.Current.LocalCacheFolder, path);
        public static async Task<StorageFile> GetCacheFolderAsync(string path) =>
            await GetFileAsync(ApplicationData.Current.LocalCacheFolder, path);

        public static async Task<StorageFile> GetTemporaryFileAsync(string fileExtension = null) => await GetFileAsync(
            ApplicationData.Current.TemporaryFolder,
            Guid.NewGuid().ToString() + (fileExtension == null ? "" : $".{fileExtension}"));
        public static async Task<StorageFolder> GetTemporaryFolderAsync() =>
            await ApplicationData.Current.TemporaryFolder.CreateFolderAsync(Guid.NewGuid().ToString());

        public static void ExtractZipArchiveToDirectory(StorageFile zipFile,
            StorageFolder destinationFolder) =>
            ZipFile.ExtractToDirectory(zipFile.Path, destinationFolder.Path, true);

        public enum CopyCollisionOptions { FailIfExists, ReplaceExisting, SkipExisting }

        public static async Task CopyFolderAsync(StorageFolder source, StorageFolder destination, CopyCollisionOptions collisionOptions = CopyCollisionOptions.ReplaceExisting, Func<IStorageItem, bool> include = null)
        {
            foreach (var item in await source.GetItemsAsync())
            {
                if (include?.Invoke(item) ?? false)
                    continue;

                if (item.IsOfType(StorageItemTypes.File))
                {
                    if (!StorageHelper.DoesFileExist(destination, item.Name) ||
                        collisionOptions != CopyCollisionOptions.SkipExisting)
                        await (item as StorageFile).CopyAsync(destination, item.Name,
                            (collisionOptions == CopyCollisionOptions.ReplaceExisting
                                ? NameCollisionOption.ReplaceExisting
                                : NameCollisionOption.FailIfExists));
                }
                else if (item.IsOfType(StorageItemTypes.Folder))
                {
                    var sf = item as StorageFolder;
                    var df = await destination.CreateFolderAsync(sf.Name, CreationCollisionOption.OpenIfExists);
                    await CopyFolderAsync(sf, df);
                }
            }
        }

    }
}