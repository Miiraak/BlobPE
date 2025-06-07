using System.Text;
using System.Text.Json;

namespace BlobPE
{
    /// <summary>
    /// Provides functionality for reading and writing JSON-encoded data blobs embedded within executable files.
    /// </summary>
    /// <remarks>The <see cref="BlobStore"/> class is designed to handle JSON data embedded in executable
    /// files,  delimited by predefined start and end tags. It includes methods for reading and writing such data, 
    /// ensuring that the data is properly serialized and deserialized. This class is intended for internal use  and
    /// assumes that the executable file format supports appending or modifying data.</remarks>
    internal class BlobStore
    {
        /// <summary>
        /// Represents the byte sequence used to identify the start of BLOB data in an executable file.Tags to identify the start and end of the BLOB data in the executable file
        /// </summary>
        /// <remarks>The tag is encoded as UTF-8 and is intended to mark the beginning of a BLOB section.
        /// This field is read-only and cannot be modified.</remarks>
        private static readonly byte[] StartTagBytes = Encoding.UTF8.GetBytes("[BLOB_START]");
        /// <summary>
        /// Represents the UTF-8 encoded byte sequence for the end tag "[BLOB_END]".
        /// </summary>
        /// <remarks>This field is used to signify the end of a data blob in scenarios where a specific
        /// delimiter is required.</remarks>
        private static readonly byte[] EndTagBytes = Encoding.UTF8.GetBytes("[BLOB_END]");

        /// <summary>
        /// Reads embedded JSON data from the current executable file and deserializes it into a dictionary.
        /// </summary>
        /// <remarks>This method searches for a JSON payload embedded within the executable file,
        /// delimited by predefined start and end tags. If the tags are not found, or if the JSON data is invalid, an
        /// empty dictionary is returned.</remarks>
        /// <returns>A dictionary containing the deserialized JSON data. Returns an empty dictionary if no valid JSON data is
        /// found.</returns>
        internal static Dictionary<string, string> Read()
        {
            string exePath = Environment.ProcessPath;
            byte[] fileBytes = File.ReadAllBytes(exePath);

            int startIndex = FindSequence(fileBytes, StartTagBytes);
            if (startIndex == -1)
                return new Dictionary<string, string>();

            int endIndex = FindSequence(fileBytes, EndTagBytes, startIndex + StartTagBytes.Length);
            if (endIndex == -1)
                return new Dictionary<string, string>();

            int jsonStart = startIndex + StartTagBytes.Length;
            int jsonLength = endIndex - jsonStart;

            byte[] jsonBytes = new byte[jsonLength];
            Array.Copy(fileBytes, jsonStart, jsonBytes, 0, jsonLength);

            string json = Encoding.UTF8.GetString(jsonBytes).Trim();

            try
            {
                var data = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                return data ?? new Dictionary<string, string>();
            }
            catch
            {
                return new Dictionary<string, string>();
            }
        }

        /// <summary>
        /// Updates or appends a JSON-encoded data blob within a specified executable file.
        /// </summary>
        /// <remarks>If a JSON blob delimited by the tags <c>[BLOB_START]</c> and <c>[BLOB_END]</c>
        /// already exists in the file,  it will be replaced with the new serialized data, provided the new data fits
        /// within the existing space.  If the new data exceeds the available space, an exception is thrown. <para> If
        /// no existing blob is found, a new JSON blob is appended to the end of the file. </para></remarks>
        /// <param name="exePath">The path to the executable file to be modified. Must not be null or empty.</param>
        /// <param name="data">A dictionary containing the key-value pairs to be serialized into the JSON blob.</param>
        /// <param name="defaultData">A dictionary containing default data to be used when creating a new blob if no existing blob is found.</param>
        /// <exception cref="Exception">Thrown if the new JSON blob exceeds the size of the existing blob in the file.</exception>
        internal static void Write(string exePath, Dictionary<string, string> data, Dictionary<string, int> defaultData)
        {
            byte[] fileBytes = File.ReadAllBytes(exePath);

            int startIndex = FindSequence(fileBytes, StartTagBytes);
            int endIndex = FindSequence(fileBytes, EndTagBytes, startIndex + StartTagBytes.Length);

            if (startIndex == -1 || endIndex == -1)
            {
                // If no existing blob is found, append a new one
                // Create a new Json blob with default sized data
                if (defaultData == null || defaultData.Count == 0)
                    throw new ArgumentException("Default data cannot be null or empty when creating a new blob.");

                Dictionary<string, string> defaultBlobData = new Dictionary<string, string>();
                foreach (var kvp in defaultData)
                {
                    if (kvp.Value <= 0)
                        throw new ArgumentException($"Field size for '{kvp.Key}' must be greater than zero.");

                    defaultBlobData[kvp.Key] = new string(' ', kvp.Value);
                }

                string json = JsonSerializer.Serialize(defaultBlobData);
                string blob = $"[BLOB_START]{json}[BLOB_END]";
                using (var stream = new FileStream(exePath, FileMode.Append, FileAccess.Write, FileShare.None))
                {
                    byte[] blobBytes = Encoding.UTF8.GetBytes(blob);
                    stream.Write(blobBytes, 0, blobBytes.Length);
                }

                fileBytes = File.ReadAllBytes(exePath);
                startIndex = FindSequence(fileBytes, StartTagBytes);
                endIndex = FindSequence(fileBytes, EndTagBytes, startIndex + StartTagBytes.Length);
            }

            int jsonStart = startIndex + StartTagBytes.Length;
            int jsonLength = endIndex - jsonStart;

            string newJson = JsonSerializer.Serialize(data);
            byte[] newJsonBytes = Encoding.UTF8.GetBytes(newJson);

            if (newJsonBytes.Length > jsonLength)
                throw new Exception($"Le nouveau blob est trop grand pour la zone existante ({newJsonBytes.Length} > {jsonLength}).");

            byte[] newFileBytes = new byte[fileBytes.Length];
            Array.Copy(fileBytes, newFileBytes, fileBytes.Length);
            Array.Copy(newJsonBytes, 0, newFileBytes, jsonStart, newJsonBytes.Length);

            for (int i = jsonStart + newJsonBytes.Length; i < jsonStart + jsonLength; i++)
                newFileBytes[i] = (byte)' ';

            File.WriteAllBytes(exePath, newFileBytes);
        }

        /// <summary>
        /// Searches for the first occurrence of a specified byte sequence within a buffer, starting at a given index.
        /// </summary>
        /// <remarks>This method performs a linear search and compares the bytes in <paramref
        /// name="buffer"/> with the bytes in <paramref name="pattern"/>.  The search is case-sensitive and does not
        /// wrap around the buffer.</remarks>
        /// <param name="buffer">The array of bytes to search within. Cannot be <see langword="null"/>.</param>
        /// <param name="pattern">The byte sequence to search for. Cannot be <see langword="null"/> or empty.</param>
        /// <param name="startIndex">The zero-based index in <paramref name="buffer"/> at which to begin the search. Must be non-negative and
        /// less than the length of <paramref name="buffer"/>.</param>
        /// <returns>The zero-based index of the first occurrence of <paramref name="pattern"/> in <paramref name="buffer"/>,  or
        /// -1 if the sequence is not found.</returns>
        internal static int FindSequence(byte[] buffer, byte[] pattern, int startIndex = 0)
        {
            for (int i = startIndex; i <= buffer.Length - pattern.Length; i++)
            {
                bool found = true;
                for (int j = 0; j < pattern.Length; j++)
                {
                    if (buffer[i + j] != pattern[j])
                    {
                        found = false;
                        break;
                    }
                }
                if (found)
                    return i;
            }
            return -1;
        }
    }
}
