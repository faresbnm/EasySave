using System;
using System.Text.Json.Serialization;

namespace EasySave.Model
{
    [Serializable]
    public class Backup
    {
        [JsonPropertyName("backupName")]
        public string BackupName { get; set; }

        [JsonPropertyName("source")]
        public string Source { get; set; }

        [JsonPropertyName("target")]
        public string Target { get; set; }

        [JsonPropertyName("type")]
        public int Type { get; set; }

        [JsonIgnore]
        public string TypeDisplay { get; set; }

        // Add a parameterless constructor for deserialization
        public Backup()
        {
        }

        public Backup(string name, string source, string target, int type)
        {
            BackupName = name;
            Source = source;
            Target = target;
            Type = type;
        }
    }
}
