using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forecast2Uwp.Thunderstore.Data.Share
{

    public partial class PackSharedResponse
    {
        [JsonProperty("success")]
        public Success Success { get; set; }
    }

    public partial class Success
    {
        [JsonProperty("pack-code")]
        public string PackCode { get; set; }

        [JsonProperty("code")]
        public int Code { get; set; }
    }
}
