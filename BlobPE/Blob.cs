namespace BlobPE
{
    /// <summary>
    /// Provides methods for managing key-value data stored in a blob, as well as handling application updates.
    /// </summary>
    /// <remarks>The <see cref="Blob"/> class offers functionality to retrieve, modify, and persist key-value
    /// pairs stored in a blob. It also includes methods for managing application updates, such as checking for updates,
    /// removing update files, and saving updated settings. This class is designed to be used as a static utility and is
    /// initialized automatically when accessed.</remarks>
    public class Blob
    {
        private static readonly Dictionary<string, string> _data;

        /// <summary>
        /// Initializes static members of the <see cref="Blob"/> class by loading data from the blob store.
        /// </summary>
        /// <remarks>This static constructor is invoked automatically before any static members of the
        /// <see cref="Blob"/> class are accessed. It ensures that the blob data is loaded from the underlying storage
        /// at the time of class initialization.</remarks>
        static Blob()
        {
            _data = BlobStore.Read();
        }

        /// <summary>
        /// Checks for updates using the specified arguments.
        /// </summary>
        /// <remarks>This method delegates the update-checking process to the underlying <see
        /// cref="BlobUpdater.CheckForUpdates"/> method. Ensure that the <paramref name="args"/> parameter contains
        /// valid input as expected by the update mechanism.</remarks>
        /// <param name="args">An array of strings representing the arguments used to check for updates. The specific arguments required
        /// depend on the update mechanism.</param>
        /// <param name="defaultData">A dictionary<string, int> containing default data to be used only once during the blob creation process.</param>
        public static void CheckForUpdates(string[] args, Dictionary<string, int> defaultData)
        {
            BlobUpdater.CheckForUpdates(args, defaultData);
        }

        /// <summary>
        /// Removes all update files from the storage location.
        /// </summary>
        /// <remarks>This method deletes any files related to updates that are stored in the system.  It
        /// is typically used to clean up after an update process has completed.</remarks>
        public static void RemoveUpdateFiles()
        {
            BlobUpdater.RemoveUpdateFiles();
        }

        /// <summary>
        /// Retrieves the value associated with the specified key from the data store.
        /// </summary>
        /// <param name="key">The key whose associated value is to be retrieved. Cannot be <see langword="null"/> or empty.</param>
        /// <returns>The value associated with the specified key, or <see langword="null"/> if the key does not exist in the data
        /// store.</returns>
        public static string Get(string key)
        {
            _data.TryGetValue(key, out string value);
            return value;
        }

        /// <summary>
        /// Retrieves a boolean value associated with the specified key.
        /// </summary>
        /// <param name="key">The key used to look up the boolean value. Cannot be null.</param>
        /// <returns><see langword="true"/> if the value associated with the specified key can be parsed as <see
        /// langword="true"/>;  otherwise, <see langword="false"/>. Returns <see langword="false"/> if the key does not
        /// exist or the value cannot be parsed. </returns>
        public static bool GetBool(string key)
        {
            _data.TryGetValue(key, out var value);
            bool.TryParse(value, out bool result);
            return result;
        }

        /// <summary>
        /// Retrieves an integer value associated with the specified key from the internal data store.
        /// </summary>
        /// <param name="key">The key used to look up the value in the data store. Cannot be null.</param>
        /// <returns>The integer value associated with the specified key if it exists and can be parsed as an integer; 
        /// otherwise, 0.</returns>
        public static int GetInt(string key)
        {
            _data.TryGetValue(key, out var value);
            int.TryParse(value, out int result);
            return result;
        }

        /// <summary>
        /// Associates the specified value to his key in the internal data store.
        /// </summary>
        /// <param name="key">The key used to identify the value. Cannot be null or empty.</param>
        /// <param name="value">The value to associate with the specified key. Cannot be null.</param>
        public static void Set(string key, object value)
        {
            _data[key] = value.ToString();
        }

        /// <summary>
        /// Saves the current data by initiating the update process.
        /// </summary>
        /// <remarks>This method triggers the update of the data file using the <see cref="BlobUpdater"/>
        /// class. Ensure that the data to be saved is properly prepared before calling this method.</remarks>
        public static void Save()
        {
            BlobUpdater.UpdateRestart(_data);
        }

        /// <summary>
        /// Resets the contents of all blobs in the data store to empty strings of the same length.
        /// </summary>
        /// <remarks>This method iterates through all keys in the data store and replaces the content of
        /// each blob  with a string of spaces, preserving the original length of the blob. After resetting the blobs, 
        /// the updated data is saved using the <see cref="BlobUpdater.UpdateFile"/> method.</remarks>
        public static void Reset()
        {
            foreach (var key in _data.Keys.ToList())
            {
                int lenght = _data[key].Length;
                _data[key] = new string(' ', lenght);
            }

            BlobUpdater.UpdateRestart(_data);
        }

        /// <summary>
        /// Deletes the current blob from the blob store.
        /// </summary>
        /// <remarks>This method removes the blob associated with the current context from the underlying
        /// blob store. Ensure that the blob exists before calling this method to avoid unexpected behavior.</remarks>
        public static void Delete()
        {
            BlobUpdater.DeleteRestart();
        }
    }
}
