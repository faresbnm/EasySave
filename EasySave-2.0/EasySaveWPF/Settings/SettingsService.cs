using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;


namespace EasySaveWPF.Settings
{
    public class SettingsService
    {
        private const string ConfigPath = "Settings.json";

        public void Save(UserSettings settings)
        {
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ConfigPath, json);
        }

        public UserSettings Load()
        {
            if (!File.Exists(ConfigPath))
                return new UserSettings(); // default

            var json = File.ReadAllText(ConfigPath);
            return JsonSerializer.Deserialize<UserSettings>(json) ?? new UserSettings();
        }
    }
}
