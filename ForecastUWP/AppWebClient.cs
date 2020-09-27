using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Web.Http;
using ForecastUWP.Helpers;
using Newtonsoft.Json;

namespace ForecastUWP
{
    class AppWebClient
    {
        public static HttpClient Client;

        static AppWebClient()
        {
            Client = new HttpClient();
        }

        public static async Task<string> GetStringAsync(Uri uri) => await Client.GetStringAsync(uri);
        public static async Task<string> GetStringAsync(string uri) => await Client.GetStringAsync(new Uri(uri));

        public static async Task<object> GetObjectAsync(Uri uri, Func<string, object> converter = null)
        {
            if (converter == null)
                converter = (string text) => JsonConvert.DeserializeObject(text);

            return converter.Invoke(await GetStringAsync(uri));
        }

        public static async Task<object> GetObjectAsync(string uri, Func<string, object> converter = null) =>
            await GetObjectAsync(new Uri(uri), converter);

        public static async Task<StorageFile> GetFileAsync(Uri uri, StorageFile target)
        {
            var res = await Client.GetAsync(uri);
            if (!res.IsSuccessStatusCode)
                return null;
            await FileIO.WriteBufferAsync(target, await res.Content.ReadAsBufferAsync());
            return target;
        }

        public static async Task<StorageFile> GetCachedFileAsync(Uri uri, TimeSpan allowedOffset)
        {
            string name = WebHelper.GenerateNameFromUrl(uri.AbsoluteUri);
            StorageFile file;
            if (StorageHelper.IsWebFileCached(name))
            {
                file = await StorageHelper.GetOrCreateCachedWebFileAsync(name);
                var props = await file.GetBasicPropertiesAsync();
                if (DateTime.Now < props.DateModified.Add(allowedOffset))
                    return file;
            }

            var response = await Client.GetAsync(uri);
            if (!response.IsSuccessStatusCode)
                return null;
            file = await StorageHelper.GetOrCreateCachedWebFileAsync(name);
            var stream = await file.OpenStreamForWriteAsync();
            await response.Content.WriteToStreamAsync(stream.AsOutputStream());
            return file;
        }
    }
}
