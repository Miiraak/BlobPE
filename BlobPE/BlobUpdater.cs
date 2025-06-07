using System.Diagnostics;
using System.Text.Json;

namespace BlobPE
{
    /// <summary>
    /// Provides functionality for managing and applying updates to the current executable or related files.
    /// </summary>
    /// <remarks>The <see cref="BlobUpdater"/> class includes methods for checking update commands, applying
    /// updates, initiating update processes, and cleaning up temporary update files. It is designed to handle update
    /// operations in a controlled manner, ensuring that updates are applied safely and efficiently.</remarks>
    internal class BlobUpdater
    {
        /// <summary>
        /// Checks for update commands and applies updates if specified.
        /// </summary>
        /// <remarks>This method processes command-line arguments to determine if an update operation 
        /// should be performed. If the first argument is <c>--update</c> and exactly three  arguments are provided, the
        /// method applies the update using the subsequent two  arguments as parameters.</remarks>
        /// <param name="args">An array of command-line arguments. The first argument must be <c>--update</c>,  followed by two additional
        /// arguments specifying the update parameters.</param>
        /// <param name="defaultData">A dictionary containing default data to be used once during the blob creation process.</param>
        internal static void CheckForUpdates(string[] args, Dictionary<string, int> defaultData)
        {
            if (args.Length == 3 && args[0] == "--update")
            {
                ApplyUpdate(args[1], args[2], defaultData);
                return;
            }
        }

        /// <summary>
        /// Applies an update to the specified target path using the provided JSON data.
        /// </summary>
        /// <remarks>This method waits until the target path is no longer locked before applying the
        /// update.  The update data is deserialized from the provided JSON string and written to the target path. After
        /// the update is applied, the target path is executed as a process, and the application exits.</remarks>
        /// <param name="targetPath">The file path where the update will be applied. This must be a valid, writable path.</param>
        /// <param name="jsonData">A JSON-formatted string containing the update data. The string must represent a dictionary of key-value
        /// pairs.</param>
        /// <param name="defaultData">A dictionary containing default data to be used during the blob creation process.</param>
        private static void ApplyUpdate(string targetPath, string jsonData, Dictionary<string, int> defaultData)
        {
            while (IsLocked(targetPath))
                Thread.Sleep(100);

            var data = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonData);
            BlobStore.Write(targetPath, data, defaultData);
            Process.Start(targetPath);
            Environment.Exit(0);
        }

        /// <summary>
        /// Determines whether the specified file is currently locked by another process or unavailable for access.
        /// </summary>
        /// <remarks>This method attempts to open the file with read/write access and exclusive sharing.
        /// If the operation fails,         it is assumed that the file is locked or in use by another
        /// process.</remarks>
        /// <param name="file">The full path of the file to check.</param>
        /// <returns><see langword="true"/> if the file is locked or unavailable for access; otherwise, <see langword="false"/>.</returns>
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

        /// <summary>
        /// Initiates the process to update the current executable with the provided data.
        /// </summary>
        /// <remarks>This method creates a temporary copy of the current executable, serializes the
        /// provided data into a JSON payload,  and starts the temporary executable with the necessary arguments to
        /// perform the update.  The current process will terminate after invoking the update process.</remarks>
        /// <param name="updatedData">A dictionary containing key-value pairs of data to be used during the update process.  Keys and values must
        /// be serializable to JSON.</param>
        internal static void StartUpdateFile(Dictionary<string, string> updatedData)
        {
            string exePath = Environment.ProcessPath;
            string tempPath = Path.Combine(Path.GetTempPath(), "updateBlobPOC_" + Guid.NewGuid() + ".exe");

            File.Copy(exePath, tempPath, true);
            string payload = JsonSerializer.Serialize(updatedData);
            Process.Start(tempPath, $"--update \"{exePath}\" \"{payload.Replace("\"", "\\\"")}\"");
            Environment.Exit(0);
        }

        /// <summary>
        /// Deletes temporary update files from the system's temporary directory.
        /// </summary>
        /// <remarks>This method searches the system's temporary directory for files matching the pattern
        /// "updateBlobPOC_*.exe" and attempts to delete them. If a file cannot be deleted, the error is logged for
        /// debugging purposes.</remarks>
        internal static void RemoveUpdateFiles()
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
    }
}
