using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using ForecastUWP.Data;
using ForecastUWP.Data.Thunderstore.Packages;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace ForecastUWP.Helpers
{
    class ProfileHelper
    {
        private static List<Profile> Profiles = null;

        public static async Task<List<Profile>> GetModdedProfilesAsync()
        {
            if (Profiles == null)
                await LoadProfilesFromFileAsync();
            return Profiles;
        }

        public static async Task<List<Profile>> GetProfilesAsync()
        {
            var profiles = (await GetModdedProfilesAsync()).ToList();  // ToList makes another copy to prevent the orignal list being edited.
            profiles.Add(new Profile { Name = "Vanilla", Packages = new Package[] { new Package {Name = "Alternate Game Modes"} }, CanBeModified = false });
            return profiles;
        }

        public static async Task AddProfileAsync(Profile profile)
        {
            var oldProfileWithSameName = Profiles.Where(x => x.Name == profile.Name).FirstOrDefault();
            if (oldProfileWithSameName != null)
                Profiles.Remove(oldProfileWithSameName);
            Profiles.Add(profile);
            await SaveProfilesToFileAsync();
        }

        public static async Task<bool> DeleteProfileAsync(Profile profile)
        {
            if (Profiles.Remove(profile))
            {
                await SaveProfilesToFileAsync();
                //TODO: Delete mods here if not used by another profile
                return true;
            }

            return false;
        }

        public static Profile ProfileFromBson(string bson)
        {
            byte[] data = Convert.FromBase64String(bson);

            MemoryStream ms = new MemoryStream(data);
            using (BsonReader reader = new BsonReader(ms))
            {
                JsonSerializer serializer = new JsonSerializer();

                return serializer.Deserialize<Profile>(reader);
            }
        }

        public static string ProfileToBson(Profile profile)
        {
            MemoryStream ms = new MemoryStream();
            using (BsonWriter writer = new BsonWriter(ms))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(writer, profile);
            }

            return Convert.ToBase64String(ms.ToArray());
        }

        public static async Task SaveProfilesToFileAsync()
        {
            await FileIO.WriteTextAsync(await StorageHelper.GetLocalFileAsync("profiles.json"),
                JsonConvert.SerializeObject(Profiles));
        }

        public static async Task LoadProfilesFromFileAsync()
        {
            Profiles = (StorageHelper.DoesLocalFileExist("profiles.json")
                    ? JsonConvert.DeserializeObject<List<Profile>>(
                        await FileIO.ReadTextAsync(
                            await StorageHelper.GetLocalFileAsync("profiles.json"))) : new List<Profile>()
                );
        }
    }
}
