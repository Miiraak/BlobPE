namespace BlobPE
{
    public class Blob
    {
        private static readonly Dictionary<string, string> _data;

        static Blob()
        {
            _data = BlobStore.Read();
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
        /// Saves the current settings to the executable's BLOB section.
        /// </summary>
        public static void Save()
        {
            BlobUpdater.UpdateAndRestart(_data);
        }

        /// <summary>
        /// Creates the BLOB section in the executable if it does not exist.
        /// </summary>
        public static void CreateBlob(Dictionary<string, string> dict)
        {
            if (_data == null)
                BlobInjector.InjectAndRestart(dict);
        }
    }
}
