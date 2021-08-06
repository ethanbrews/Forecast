using System;
using Windows.Storage;

namespace Forecast2Uwp.Helpers
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

        public void ResetToDefault() => ApplicationData.Current.LocalSettings.Values[Key] = null;
    }
}
