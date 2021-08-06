using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forecast2Uwp
{
    public class AbstractViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(object sender, params string[] names)
        {
            foreach(var name in names)
            {
                PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(name));
            }
        }
    }
}
