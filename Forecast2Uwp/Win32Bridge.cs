using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;

namespace Forecast2Uwp
{
    static class Win32Bridge
    {
        private static async Task<T> RequestDataAsync<T>(string request, ValueSet data = null)
        {
            var values = data ?? new ValueSet();
            values.Add("request", request);
            var response = await (App.Current as App).Connection.SendMessageAsync(values);
            if (response.Status != Windows.ApplicationModel.AppService.AppServiceResponseStatus.Success)
                throw new Exception($"Got {response.Status} from ForecastConsole (Full Trust Win32 Process)");
            return (T)response.Message["response"];
        }

        public static async Task<string> GetSteamInstallationPathAsync() => await RequestDataAsync<string>("SteamInstallPath");
        public static async Task<bool> RunProgramAsync(string path, bool hidden = true) =>
            await RequestDataAsync<bool>("RunProgram", new ValueSet
            {
                ["path"] = path,
                ["hidden"] = hidden
            });
    }
}
