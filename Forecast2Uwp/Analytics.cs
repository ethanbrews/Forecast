using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forecast2Uwp
{
    public static class Events
    {
        public static string TestEvent = "TestEvent";
    }

    public static class Analytics
    {

        public static void Log(string message)
        {
            System.Diagnostics.Debug.WriteLine($"[Forecast (UWP)] {message}");
        }

        public static void TrackEvent(string name, Dictionary<string, string> keyValuePairs = null)
        {
            if (keyValuePairs is null)
            {
                Microsoft.AppCenter.Analytics.Analytics.TrackEvent(name);
                System.Diagnostics.Debug.WriteLine($"Tracked Event: {name}");

            } else
            {
                Microsoft.AppCenter.Analytics.Analytics.TrackEvent(name, keyValuePairs);
                System.Diagnostics.Debug.WriteLine($"Tracked Event with parameters: {name}\n " + string.Join("\n", keyValuePairs.ToList().Select((pair) =>
                    $" - {pair.Key} = {pair.Value}"
                )));
            }
        }

        public static void TrackError(Exception exception, Dictionary<string, string> properties = null)
        {
            if (properties is null)
            {
                System.Diagnostics.Debug.WriteLine($"Tracked Error: {exception.ToString()}");
                Microsoft.AppCenter.Crashes.Crashes.TrackError(exception);
            } else
            {
                Microsoft.AppCenter.Crashes.Crashes.TrackError(exception, properties);
                System.Diagnostics.Debug.WriteLine($"Tracked Error with parameters: {exception.ToString()}\n " + string.Join("\n", properties.ToList().Select((pair) =>
                        $" - {pair.Key} = {pair.Value}"
                    )));
            }
        }
    }
}
