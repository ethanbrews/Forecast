using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ForecastUWP.Data
{
    public partial class Profile : INotifyPropertyChanged
    {

        [field: JsonIgnore] public event PropertyChangedEventHandler PropertyChanged;

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("packages")]
        public Data.Thunderstore.Packages.Package[] Packages { get; set; }

        [JsonIgnore] public bool CanBeModified { get; set; } = true;

        [JsonIgnore] private bool _requiresInstall = false;

        [JsonIgnore]
        public bool RequiresInstall
        {
            get => _requiresInstall;
            set
            {
                _requiresInstall = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("RequiresInstall"));
            }
        }

        [JsonIgnore] public Tuple<int, int, int> _currentInstalledTotalToInstallProgress = null;

        [JsonIgnore]
        public Tuple<int, int, int> CurrentInstalledTotalToInstallProgress
        {
            get => _currentInstalledTotalToInstallProgress;
            set
            {
                _currentInstalledTotalToInstallProgress = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CurrentInstalledTotalToInstallProgress"));
            }
        }


    }

    public partial class Profile
    {
        public static Profile FromJson(string s)
        {
            return JsonConvert.DeserializeObject<Profile>(s);
        }
    }
}
