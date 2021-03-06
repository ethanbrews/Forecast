// <auto-generated />
//
// To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
//
//    using ForecastUWP.Data.ForecastWebApp;
//
//    var forecastSuccessResponse = ForecastSuccessResponse.FromJson(jsonString);

namespace ForecastUWP.Data.ForecastWebApp
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class ForecastSuccessResponse
    {
        [JsonProperty("success", NullValueHandling = NullValueHandling.Ignore)]
        public Success Success { get; set; }
    }

    public partial class Success
    {
        [JsonProperty("pack-code", NullValueHandling = NullValueHandling.Ignore)]
        public string PackCode { get; set; }

        [JsonProperty("code", NullValueHandling = NullValueHandling.Ignore)]
        public long? Code { get; set; }
    }

    public partial class ForecastSuccessResponse
    {
        public static ForecastSuccessResponse FromJson(string json) => JsonConvert.DeserializeObject<ForecastSuccessResponse>(json, ForecastUWP.Data.ForecastWebApp.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this ForecastSuccessResponse self) => JsonConvert.SerializeObject(self, ForecastUWP.Data.ForecastWebApp.Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}