using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace BlobPE
{
    public class BlobInjector
    {
        private static void InjectBlob(Dictionary<string, string> data)
        {
            string filePath = Environment.ProcessPath;
            string json = JsonSerializer.Serialize(data);
            string blob = "[BLOB_START]" + json + "[BLOB_END]";
            byte[] blobBytes = Encoding.UTF8.GetBytes(blob);

            byte[] originalBytes = File.ReadAllBytes(filePath);

            using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                stream.Write(originalBytes, 0, originalBytes.Length);
                stream.Write(blobBytes, 0, blobBytes.Length);
            }
        }

        public static void InjectAndRestart(Dictionary<string, string> data)
        {
            string exePath = Environment.ProcessPath;
            string tempPath = Path.Combine(Path.GetTempPath(), "updateBlobPOC_" + Guid.NewGuid() + ".exe");
            File.Copy(exePath, tempPath, true);
            InjectBlob(data);
            Process.Start(tempPath, "--injectblob");
            Environment.Exit(0);
        }
    }
}
