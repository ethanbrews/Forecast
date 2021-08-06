using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forecast2Uwp.Helpers
{
    static class ThreadHelper
    {
        public static async Task RunOnUIThreadAsync(Windows.UI.Core.DispatchedHandler handler) =>
         await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, handler);

        public static async Task RunOnBackgroundThreadAsync(Action action) =>
            await Task.Run(action);
    }
}
