using Microsoft.Win32;
using System;
using System.IO;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;

namespace ForecastConsole
{
    class Program
    {

        public static bool ExistsOnPath(string fileName)
        {
            return GetFullPath(fileName) != null;
        }

        public static string GetFullPath(string fileName)
        {
            if (File.Exists(fileName))
                return Path.GetFullPath(fileName);

            var values = Environment.GetEnvironmentVariable("PATH");
            foreach (var path in values.Split(Path.PathSeparator))
            {
                var fullPath = Path.Combine(path, fileName);
                if (File.Exists(fullPath))
                    return fullPath;
            }
            return null;
        }

        static AppServiceConnection Connection;
        static void Main(string[] args)
        {
            try
            {
                Console.Title = "Forecast Console Component";
            } catch (Exception ex) { }
            
            Console.WriteLine("Opening AppServiceConnection");
            InitializeAppServiceConnection();
            try
            {
                Console.ReadLine();
            }
            catch (Exception ex) { }
            
        }

        private static async void InitializeAppServiceConnection()
        {
            Connection = new AppServiceConnection();
            Connection.AppServiceName = "SampleInteropService";
            Connection.PackageFamilyName = Package.Current.Id.FamilyName;
            Connection.RequestReceived += Connection_RequestReceived;
            Connection.ServiceClosed += Connection_ServiceClosed;

            AppServiceConnectionStatus status = await Connection.OpenAsync();
            if (status != AppServiceConnectionStatus.Success)
            {
                //Failed
            }
        }

        private static void Connection_ServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            Environment.Exit(0);
        }

        private static void Connection_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            var request = (string)args.Request.Message["request"];
            object responseObj = null;
            Console.WriteLine(request);
            if (request == "SteamInstallPath")
            {
                try
                {
                    string keyPath = Environment.Is64BitOperatingSystem ? @"SOFTWARE\Valve\Steam" : @"SOFTWARE\Wow6432Node\Valve\Steam";
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath))
                    {
                        if (key != null)
                        {
                            Object o = key.GetValue("InstallPath");
                            if (o != null)
                            {
                                responseObj = o as String;
                            }
                        } else
                        {
                            Console.WriteLine("Key is null");
                        }
                    }
                }
                catch (Exception ex)  //just for demonstration...it's always best to handle specific exceptions
                {
                    Console.WriteLine("Exception reading key, Registry.LocalMachine.OpenSubKey");
                }
            }
            else if (request == "RunProgram")
            {
                bool hidden = args.Request.Message.GetValueOrNull("hidden") as bool? ?? true;
                string path = args.Request.Message.GetValueOrNull("path") as string;
                if (path is null)
                    return;

                bool useWt = ExistsOnPath("wt.exe");

                Console.WriteLine((useWt ? "wt.exe" : "cmd.exe /C") + " " + path);

                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.WindowStyle = hidden ? System.Diagnostics.ProcessWindowStyle.Hidden : System.Diagnostics.ProcessWindowStyle.Normal;
                startInfo.FileName = useWt ? "wt.exe" : "cmd.exe";
                startInfo.Arguments = (useWt ? "" : "/C ") + path;
                process.StartInfo = startInfo;
                process.Start();
            }

                var vs = new ValueSet();
            vs["response"] = responseObj;
            Console.WriteLine(responseObj.ToString());
            _ = args.Request.SendResponseAsync(vs);
        }
    }
}
