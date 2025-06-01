using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace EasySave.Model
{
    [Serializable]
    public class Backup : INotifyPropertyChanged
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private double? _progressPercentage;
        public double? ProgressPercentage
        {
            get => _progressPercentage;
            set
            {
                _progressPercentage = value;
                OnPropertyChanged();
            }
        }

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
