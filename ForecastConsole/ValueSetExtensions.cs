using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;

namespace ForecastConsole
{
    static class ValueSetExtensions
    {
        public static object GetValueOrNull(this ValueSet set, string key) =>
            set.TryGetValue(key, out var res) ? res : null;
    }
}
