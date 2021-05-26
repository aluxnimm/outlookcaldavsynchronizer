using CalDavSynchronizer.OAuth.Daimler;
using CalDavSynchronizer.OAuth.Daimler.Models;
using CalDavSynchronizer.Ui;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CalDavSynchronizer.ProfileTypes.ConcreteTypes.Daimler.Helpers
{
    public class ProfileDataProvider
    {
        private readonly Guid _profileId;
        private readonly string _profileDataPath;

        public ProfileDataProvider(Guid profileId, bool useRoamingFolder = false)
        {
            _profileId = profileId;
            _profileDataPath = Path.Combine(Environment.GetFolderPath(useRoamingFolder ?
                Environment.SpecialFolder.ApplicationData : Environment.SpecialFolder.LocalApplicationData), "CalDavSynchronizer", profileId.ToString());
        }

        public void SaveConfig(DaimlerOptions configuration)
        {
            var json = JsonConvert.SerializeObject(configuration);
            var filePath = Path.Combine(_profileDataPath, ".config");

            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            if (File.Exists(filePath)) File.Delete(filePath);
            File.WriteAllText(filePath, json);
        }

        public DaimlerOptions LoadConfig()
        {
            var filePath = Path.Combine(_profileDataPath, ".config");
            if (!File.Exists(filePath)) return null;

            var json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<DaimlerOptions>(json);
        }

        public ProfileOptions LoadProfileOptions()
        {
            var data = LoadProfileOptionsData();

            return data?.Profiles?.FirstOrDefault(profile => profile.ProfileId == _profileId);
        }

        private ProfileOptionsData LoadProfileOptionsData()
        {
            var filePath = Path.Combine(_profileDataPath, "profile.options");
            if (!File.Exists(filePath)) return null;

            var json = File.ReadAllText(filePath);

            return JsonConvert.DeserializeObject<ProfileOptionsData>(json);
        }

        public void SaveProfileOptions(ProfileOptions options)
        {
            var data = LoadProfileOptionsData() ?? new ProfileOptionsData();
            var existing = data.Profiles?.FirstOrDefault(profile => profile.ProfileId == options.ProfileId);
            if (existing == null)
            {
                data.Profiles = data.Profiles.Append(options).ToArray();
            }
            else
            {
                existing.SelectedEnvironment = options.SelectedEnvironment;
                existing.CalenderUrl = options.CalenderUrl;
            }

            var filePath = Path.Combine(_profileDataPath, "profile.options");
            var json = JsonConvert.SerializeObject(data);

            File.WriteAllText(filePath, json);
        }

        public void SaveToken(TokenData data, string environment)
        {
            if (data == null) return;

            var envDir = Path.Combine(_profileDataPath, environment);

            Directory.CreateDirectory(envDir);

            var tokenFile = Path.Combine(envDir, ".token");
            File.WriteAllText(tokenFile, JsonConvert.SerializeObject(data));
        }

        public async Task<TokenData> LoadToken(string environment, bool tryAuthIfNotExists = false)
        {
            TokenData token = default;
            var tokenFile = Path.Combine(_profileDataPath, environment, ".token");
            if (!File.Exists(tokenFile)) // return null;
            {
                if (!tryAuthIfNotExists) return null;

                var config = LoadConfig();
                var env = config.Environments.First(e => e.Name == environment);

                await UiService.Dispatcher.Invoke<Task>(async () =>
                {
                    token = await new AuthenticationService(env).Authenticate();
                    SaveToken(token, environment);
                });

                return token;
            }

            var json = File.ReadAllText(tokenFile);
            token = JsonConvert.DeserializeObject<TokenData>(json);

            if (!token.IsExpired) return token;

            if (tryAuthIfNotExists)
            {
                var config = LoadConfig();
                var env = config.Environments.First(e => e.Name == environment);
                token = await UiService.Dispatcher.Invoke<Task<TokenData>>(async () =>
                {
                    var authSvc = new AuthenticationService(env);

                    try
                    {
                        var refreshed = await authSvc.RefreshToken(token.RefreshToken);
                        SaveToken(refreshed, environment);

                        return refreshed;
                    }
                    catch { }

                    token = await authSvc.Authenticate();
                    SaveToken(token, environment);

                    return token;
                });
            }

            return token;
        }
    }
}
