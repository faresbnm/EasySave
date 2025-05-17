using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Globalization;
using System.Threading;

namespace EasySave.Localization
{
    public interface ILocalizationService
    {
        string this[string key] { get; }
        string CurrentLanguage { get; }
        void SetLanguage(string languageCode);
        string Format(string key, params object[] args);
    }

    public class JsonLocalizationService : ILocalizationService
    {
        private Dictionary<string, string> _currentLanguageStrings;
        private string _currentLanguage = "en";

        public JsonLocalizationService()
        {
            SetLanguage(_currentLanguage);
        }

        public string this[string key] => _currentLanguageStrings.TryGetValue(key, out var value)
            ? value
            : $"[{key}]";

        public string CurrentLanguage => _currentLanguage;

        public void SetLanguage(string languageCode)
        {
            _currentLanguage = languageCode.ToLower() == "fr" ? "fr" : "en";

            try
            {
                // Get the path to the Languages folder in your project
                string languagesPath = Path.Combine(Directory.GetCurrentDirectory(), "Languages");
                string filePath = Path.Combine(languagesPath, $"{_currentLanguage}.json");
                if (!File.Exists(filePath))
                {
                    // Try alternative path for when running from bin folder
                    filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Languages", $"{_currentLanguage}.json");
                }

                if (File.Exists(filePath))
                {
                    string jsonContent = File.ReadAllText(filePath);
                    _currentLanguageStrings = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent)
                        ?? new Dictionary<string, string>();
                }
                else
                {
                    _currentLanguageStrings = new Dictionary<string, string>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading language file: {ex.Message}");
                _currentLanguageStrings = new Dictionary<string, string>();
            }
        }

        public string Format(string key, params object[] args)
        {
            var format = this[key];
            return string.Format(format, args);
        }
    }
}