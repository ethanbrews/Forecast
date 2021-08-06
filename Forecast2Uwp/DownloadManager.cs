using Forecast2Uwp.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

namespace Forecast2Uwp.Download
{
    static class DownloadManager
    {
        public static HttpClient CachingClient { get; private set; }
        public static HttpClient NonCachingClient { get; private set; }

        static DownloadManager()
        {
            var rootNonCachingFilter = new HttpBaseProtocolFilter();
            rootNonCachingFilter.CacheControl.ReadBehavior = HttpCacheReadBehavior.MostRecent;
            rootNonCachingFilter.CacheControl.WriteBehavior = HttpCacheWriteBehavior.NoCache;
            NonCachingClient = new HttpClient(rootNonCachingFilter);

            var rootCachingFilter = new HttpBaseProtocolFilter();
            rootCachingFilter.CacheControl.ReadBehavior = HttpCacheReadBehavior.Default;
            rootCachingFilter.CacheControl.WriteBehavior = HttpCacheWriteBehavior.Default;
            CachingClient = new HttpClient(rootCachingFilter);
        }

        private static HttpClient GetAppropriateClient(bool cached) => cached ? CachingClient : NonCachingClient;

        public static async Task<T> GetObjectAsync<T>(Uri uri, Func<string, T> decoder, bool useCache) =>
            decoder.Invoke(await GetStringAsync(uri, useCache));

        public static async Task<T> GetObjectAsync<T>(Uri uri) => await GetObjectAsync(uri, x => JsonConvert.DeserializeObject<T>(x), true);
        public static async Task<T> GetObjectAsync<T>(Uri uri, bool useCache) => await GetObjectAsync(uri, x => JsonConvert.DeserializeObject<T>(x), useCache);
        public static async Task<T> GetObjectAsync<T>(Uri uri, Func<string, T> decoder) => await GetObjectAsync(uri, decoder, true);

        public static async Task<string> GetStringAsync(Uri uri, bool useCache) =>
            await GetAppropriateClient(useCache).GetStringAsync(uri);

        public static async Task<string> GetStringAsync(Uri uri) => await GetStringAsync(uri, false);

        public static async Task<StorageFile> GetFileAsync(Uri uri, StorageFile target, bool useCache)
        {
            var res = await GetAppropriateClient(useCache).GetAsync(uri);
            if (!res.IsSuccessStatusCode)
                return null;
            await FileIO.WriteBufferAsync(target, await res.Content.ReadAsBufferAsync());
            return target;
        }

        public static async Task<StorageFile> GetFileAsync(Uri uri, StorageFile target) => await GetFileAsync(uri, target, true);
    }
}