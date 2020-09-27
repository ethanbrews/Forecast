using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace ForecastUWP.Helpers
{
    class LocalSettingsValue<T>
    {
        private readonly T _defaultValue;
        public string Key { get; }

        public LocalSettingsValue(string key, T defaultValue)
        {
            Key = key;
            _defaultValue = defaultValue;
        }

        public T Value
        {
            set => ApplicationData.Current.LocalSettings.Values[Key] = value;
            get => (T)Convert.ChangeType(ApplicationData.Current.LocalSettings.Values[Key] ?? _defaultValue, typeof(T));
        }
    }
}
