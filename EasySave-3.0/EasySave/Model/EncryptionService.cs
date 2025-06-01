using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryptoSoft;

namespace EasySave.Model
{
    public sealed class EncryptionService
    {
        private static readonly Lazy<EncryptionService> _instance =
            new Lazy<EncryptionService>(() => new EncryptionService());

        public static EncryptionService Instance => _instance.Value;

        private readonly object _lock = new object();
        private string _encryptionKey = "123";
        private List<string> _encryptionExtensions = new List<string>();

        private EncryptionService() { }

        public void Configure(string key, List<string> extensions)
        {
            lock (_lock)
            {
                _encryptionKey = key;
                _encryptionExtensions = extensions;
            }
        }

        public int EncryptFile(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLower();

            lock (_lock) // Check extensions list thread-safely
            {
                if (!_encryptionExtensions.Contains(extension))
                    return 0;
            }

            // Use file locking for the actual encryption
            try
            {
                lock (string.Intern(filePath)) // File-specific lock
                {
                    var fileManager = new FileManager(filePath, _encryptionKey);
                    return fileManager.TransformFile();
                }
            }
            catch (Exception ex)
            {
                return -1; // Indicate encryption failure
            }
        }
    }
}
