// <auto-generated />
//
// To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
//
//    using ForecastUno.Shared.Data.Thunderstore.Packages;
//
//    var package = Package.FromJson(jsonString);

using Windows.UI.Xaml.Media.Imaging;

namespace ForecastUWP.Data.Thunderstore.Packages
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class Package
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("full_name", NullValueHandling = NullValueHandling.Ignore)]
        public string FullName { get; set; }

        [JsonProperty("owner", NullValueHandling = NullValueHandling.Ignore)]
        public string Owner { get; set; }

        [JsonProperty("package_url", NullValueHandling = NullValueHandling.Ignore)]
        public Uri PackageUrl { get; set; }

        [JsonProperty("date_created", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? DateCreated { get; set; }

        [JsonProperty("date_updated", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? DateUpdated { get; set; }

        [JsonProperty("uuid4", NullValueHandling = NullValueHandling.Ignore)]
        public Guid? Uuid4 { get; set; }

        [JsonProperty("rating_score", NullValueHandling = NullValueHandling.Ignore)]
        public long? RatingScore { get; set; }

        [JsonProperty("is_pinned", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsPinned { get; set; }

        [JsonProperty("is_deprecated", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsDeprecated { get; set; }

        [JsonProperty("versions", NullValueHandling = NullValueHandling.Ignore)]
        public Version[] Versions { get; set; }

        [JsonProperty("forecast_profile_selected_version", NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string ForecastProfileSelectedVersion { get; set; } = null;

        [JsonIgnore]
        public Uri ImageUri { get; set; }
    }

    public partial class Version
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("full_name", NullValueHandling = NullValueHandling.Ignore)]
        public string FullName { get; set; }

        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        [JsonProperty("icon", NullValueHandling = NullValueHandling.Ignore)]
        public Uri Icon { get; set; }

        [JsonProperty("version_number", NullValueHandling = NullValueHandling.Ignore)]
        public string VersionNumber { get; set; }

        [JsonProperty("dependencies", NullValueHandling = NullValueHandling.Ignore)]
        public string[] Dependencies { get; set; }

        [JsonProperty("download_url", NullValueHandling = NullValueHandling.Ignore)]
        public Uri DownloadUrl { get; set; }

        [JsonProperty("downloads", NullValueHandling = NullValueHandling.Ignore)]
        public long? Downloads { get; set; }

        [JsonProperty("date_created", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? DateCreated { get; set; }

        [JsonProperty("website_url", NullValueHandling = NullValueHandling.Ignore)]
        public string WebsiteUrl { get; set; }

        [JsonProperty("is_active", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsActive { get; set; }

        [JsonProperty("uuid4", NullValueHandling = NullValueHandling.Ignore)]
        public Guid? Uuid4 { get; set; }
    }

    public partial class Package
    {
        public static Package[] FromJson(string json) => JsonConvert.DeserializeObject<Package[]>(json, ForecastUWP.Data.Thunderstore.Packages.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this Package[] self) => JsonConvert.SerializeObject(self, ForecastUWP.Data.Thunderstore.Packages.Converter.Settings);
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
