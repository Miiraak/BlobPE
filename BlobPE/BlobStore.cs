using System.Text;
using System.Text.Json;

namespace BlobPE
{
    internal class BlobStore
    {
        private static readonly byte[] StartTagBytes = Encoding.UTF8.GetBytes("[BLOB_START]");
        private static readonly byte[] EndTagBytes = Encoding.UTF8.GetBytes("[BLOB_END]");

        internal static Dictionary<string, string> Read()
        {
            string exePath = Environment.ProcessPath;
            byte[] fileBytes = File.ReadAllBytes(exePath);

            int startIndex = FindSequence(fileBytes, StartTagBytes);
            if (startIndex == -1)
            {
                return new Dictionary<string, string>();
            }

            int endIndex = FindSequence(fileBytes, EndTagBytes, startIndex + StartTagBytes.Length);
            if (endIndex == -1)
            {
                return new Dictionary<string, string>();
            }

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

        internal static void Write(string exePath, Dictionary<string, string> data)
        {
            byte[] fileBytes = File.ReadAllBytes(exePath);

            int startIndex = FindSequence(fileBytes, StartTagBytes);
            int endIndex = FindSequence(fileBytes, EndTagBytes, startIndex + StartTagBytes.Length);

            if (startIndex == -1 || endIndex == -1)
                throw new Exception("Zone BLOB non trouvée");

            int jsonStart = startIndex + StartTagBytes.Length;
            int jsonLength = endIndex - jsonStart;

            string newJson = JsonSerializer.Serialize(data);
            byte[] newJsonBytes = Encoding.UTF8.GetBytes(newJson);

            if (newJsonBytes.Length > jsonLength)
                throw new Exception($"Le nouveau blob est trop grand pour la zone existante ({newJsonBytes.Length} > {jsonLength}).");

            // Préparer un buffer final
            byte[] newFileBytes = new byte[fileBytes.Length];
            Array.Copy(fileBytes, newFileBytes, fileBytes.Length);

            // Copier les bytes JSON mis à jour (avec padding espaces pour garder même taille)
            Array.Copy(newJsonBytes, 0, newFileBytes, jsonStart, newJsonBytes.Length);

            // Padding espaces sur le reste
            for (int i = jsonStart + newJsonBytes.Length; i < jsonStart + jsonLength; i++)
                newFileBytes[i] = (byte)' ';

            File.WriteAllBytes(exePath, newFileBytes);
        }

        // Recherche d'une séquence d'octets dans un tableau d'octets, avec offset optionnel
        private static int FindSequence(byte[] buffer, byte[] pattern, int startIndex = 0)
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
