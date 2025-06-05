using System.Diagnostics;
using System.Text.Json;

namespace BlobPE
{
    internal class BlobUpdater
    {
        /// <summary>
        /// This should be called after the application has been updated and restarted.
        /// </summary>
        public static void RemoveUpdateFiles()
        {
            string tempPath = Path.GetTempPath();
            var files = Directory.GetFiles(tempPath, "updateBlobPOC_*.exe");
            foreach (var file in files)
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Erreur lors de la suppression du fichier {file}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Needs to be called at the start of the application.
        /// </summary>
        /// <param name="args"></param>
        public static void CheckForUpdates(string[] args)
        {
            if (args.Length == 3 && args[0] == "--update")
            {
                ApplyUpdate(args[1], args[2]);
                return;
            }
            if (args.Length == 1 && args[0] == "--injectblob")
            {
                RemoveUpdateFiles();
                return;
            }
        }

        internal static void UpdateAndRestart(Dictionary<string, string> updatedData)
        {
            string exePath = Environment.ProcessPath;
            string tempPath = Path.Combine(Path.GetTempPath(), "updateBlobPOC_" + Guid.NewGuid() + ".exe");

            File.Copy(exePath, tempPath, true);
            string payload = JsonSerializer.Serialize(updatedData);
            Process.Start(tempPath, $"--update \"{exePath}\" \"{payload.Replace("\"", "\\\"")}\"");
            Environment.Exit(0);
        }

        internal static void ApplyUpdate(string targetPath, string jsonData)
        {
            while (IsLocked(targetPath))
                Thread.Sleep(100);

            var data = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonData);
            BlobStore.Write(targetPath, data);
            Process.Start(targetPath);
            Environment.Exit(0);
        }

        private static bool IsLocked(string file)
        {
            try
            {
                using FileStream stream = File.Open(file, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                return false;
            }
            catch
            {
                return true;
            }
        }
    }
}
