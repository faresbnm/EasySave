using System.Diagnostics;
using System.Text;

namespace CryptoSoft;

/// <summary>
/// File manager class
/// This class is used to encrypt and decrypt files
/// </summary>
public class FileManager(string path, string key)
{
    private string FilePath { get; } = path;
    private string Key { get; } = key;

    /// <summary>
    /// check if the file exists
    /// </summary>
    private bool CheckFile()
    {
        if (File.Exists(FilePath))
            return true;

        Console.WriteLine("File not found.");
        Thread.Sleep(1000);
        return false;
    }

    /// <summary>
    /// Encrypts the file with xor encryption
    /// </summary>
    public int TransformFile()
    {
        if (!CheckFile()) return -1;

        try
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            // Read file with shared read access
            byte[] fileBytes;
            using (var fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                fileBytes = new byte[fs.Length];
                fs.Read(fileBytes, 0, (int)fs.Length);
            }

            var keyBytes = ConvertToByte(Key);
            fileBytes = XorMethod(fileBytes, keyBytes);

            // Write file with exclusive access
            using (var fs = new FileStream(FilePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                fs.Write(fileBytes, 0, fileBytes.Length);
            }

            stopwatch.Stop();
            return (int)stopwatch.ElapsedMilliseconds;
        }
        catch (Exception ex)
        {
            return -1;
        }
    }

    /// <summary>
    /// Convert a string in byte array
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    private static byte[] ConvertToByte(string text)
    {
        return Encoding.UTF8.GetBytes(text);
    }

    /// <summary>
    /// </summary>
    /// <param name="fileBytes">Bytes of the file to convert</param>
    /// <param name="keyBytes">Key to use</param>
    /// <returns>Bytes of the encrypted file</returns>
    private static byte[] XorMethod(IReadOnlyList<byte> fileBytes, IReadOnlyList<byte> keyBytes)
    {
        var result = new byte[fileBytes.Count];
        for (var i = 0; i < fileBytes.Count; i++)
        {
            result[i] = (byte)(fileBytes[i] ^ keyBytes[i % keyBytes.Count]);
        }

        return result;
    }
}
