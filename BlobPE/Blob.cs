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
        /// Verifies if the executable is started as an updater and applies the update if so.
        /// Needs to be called at the start of the application. In Program.cs at the beginning of the Main method.
        /// </summary>
        /// <param name="args"></param>
        public static void CheckForUpdates(string[] args)
        {
            BlobUpdater.CheckForUpdates(args);
        }

        /// <summary>
        /// Uses this method to remove the update files after the application has been updated and restarted.
        /// If you do not call this method, the update files will remain in the temporary directory.
        /// </summary>
        public static void RemoveUpdateFiles()
        {
            BlobUpdater.RemoveUpdateFiles();
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string Get(string key)
        {
            _data.TryGetValue(key, out string value);
            return value;
        }

        /// <summary>
        /// Gets the value associated with the specified key as a boolean.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool GetBool(string key)
        {
            _data.TryGetValue(key, out var value);
            bool.TryParse(value, out bool result);
            return result;
        }

        /// <summary>
        /// Gets the value associated with the specified key as an integer.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static int GetInt(string key)
        {
            _data.TryGetValue(key, out var value);
            int.TryParse(value, out int result);
            return result;
        }

        /// <summary>
        /// Used to set a value for the specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void Set(string key, object value)
        {
            _data[key] = value.ToString();
        }

        /// <summary>
        /// Saves the current settings to the executable's BLOB section and restarts the application.
        /// </summary>
        public static void Save()
        {
            BlobUpdater.StartUpdateFile(_data);
        }
    }
}
