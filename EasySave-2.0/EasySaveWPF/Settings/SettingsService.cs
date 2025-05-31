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
            {
                // First install or missing file → return fully initialized default settings
                return new UserSettings
                {
                    SelectedLogFormat = 0,
                    BusinessSoftwareName = "",
                    EncryptionExtensions = new List<string>(),
                    EncryptionKey = "123",
                    PriorityExtensions = new List<string>(),
                    MaxParallelTransferSizeKB = 0,
                    Language = "en"
                };
            }

            var json = File.ReadAllText(ConfigPath);
            return JsonSerializer.Deserialize<UserSettings>(json) ?? new UserSettings();
        }
    }
}
