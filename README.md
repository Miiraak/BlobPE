# BlobPE [![CodeQL](https://github.com/Miiraak/BlobPE/actions/workflows/github-code-scanning/codeql/badge.svg)](https://github.com/Miiraak/BlobPE/actions/workflows/github-code-scanning/codeql) [![Published](https://github.com/Miiraak/BlobPE/actions/workflows/publish-package.yml/badge.svg)](https://github.com/Miiraak/BlobPE/actions/workflows/publish-package.yml) [![NugetUpToDate](https://github.com/Miiraak/BlobPE/actions/workflows/nuget-is-up-to-date.yml/badge.svg)](https://github.com/Miiraak/BlobPE/actions/workflows/nuget-is-up-to-date.yml)

## Description
**BlobPE** is a lightweight and experimental .NET (C#) library that enables standalone applications to persist structured data directly within their own executable files. By injecting a compact blob (typically JSON) into the binary, BlobPE allows applications to store and modify internal key/value data without relying on external files or configurations.

The library is intended for advanced use cases such as proof-of-concept tools, binary experiments, or environments where minimizing external traces is desired. Data is stored within the executable itself, delimited by clear markers (e.g., `[BLOB_START]... [BLOB_END]`), and can be read, updated, and re-injected at runtime.

> Warning ⚠️: BlobPE uses a non-standard approach to binary self-modification. It is not recommended for production environments, but rather for educational, exploratory, or controlled scenarios.

## Builds :
- .NET 8.0
- .NET Standard 2.0
- .NET 40

## Features
- Inject structured data (JSON format) directly into the application's binary.
- Read and update key/value pairs embedded in the executable.
- Rewrite the binary to persist updated data.
- Self-relaunch after patching.
- Automatic padding and blob size enforcement to ensure binary integrity.
- No need for external configuration or storage files.
- Automatic injection of blob headers.
- Simple reset of the blob values to their dafault size (only space padding).
- Supports basic data types: string, int, bool, and object serialization.
- Blob removal and clearing functionality.

### Features in Development
|||
|---|---|
| **Binary offset alignment** | More robust detection and modification of embedded blobs |
| **Obfuscation** | Support for basic encoding or encryption of stored data |
| **Custom section placement** | Ability to store the blob in custom PE sections instead of only at the end of the file |

## Prerequisites
- .NET 6.0 SDK or later
- Windows operating system
- Write permissions on the application’s executable file

## Dependencies
- [Newtonsoft.Json](https://www.nuget.org/packages/Newtonsoft.Json/) - For JSON serialization and deserialization.

## User Usages ([Blob.cs](https://github.com/Miiraak/BlobPE/blob/master/BlobPE/Blob.cs))
| Method | Description |
|--- | --- |
| [**CheckForUpdates(string[] args, Dictionary<string, int> defaultData)**](https://github.com/Miiraak/BlobPE/blob/93ca37b4ddd975f8e3af7fc840a491fb35f2035b/BlobPE/Blob.cs#L37) | Checks for update args and applies updates if specified. defaultData is used when no blob is found in the binaries, using the string for key name and int for maximalData size. |
| [**RemoveUpdateFiles()**](https://github.com/Miiraak/BlobPE/blob/93ca37b4ddd975f8e3af7fc840a491fb35f2035b/BlobPE/Blob.cs#L47) | Removes any update files created during the update process. |
| [**Get(string key)**](https://github.com/Miiraak/BlobPE/blob/93ca37b4ddd975f8e3af7fc840a491fb35f2035b/BlobPE/Blob.cs#L58) | Retrieves a value as a string from the blob data by its key. |
| [**GetBool(string key)**](https://github.com/Miiraak/BlobPE/blob/93ca37b4ddd975f8e3af7fc840a491fb35f2035b/BlobPE/Blob.cs#L71) | Retrieves a value as boolean from the blob data by its key. |
| [**GetInt(string key)**](https://github.com/Miiraak/BlobPE/blob/93ca37b4ddd975f8e3af7fc840a491fb35f2035b/BlobPE/Blob.cs#L84) | Retrieves a value as integer from the blob data by its key. |
| [**Set(string key, object value)**](https://github.com/Miiraak/BlobPE/blob/93ca37b4ddd975f8e3af7fc840a491fb35f2035b/BlobPE/Blob.cs#L96) | Sets the value for a specified key in the blob data. |
| [**Save()**](https://github.com/Miiraak/BlobPE/blob/93ca37b4ddd975f8e3af7fc840a491fb35f2035b/BlobPE/Blob.cs#L106) | Saves the current state of the app data back into his own executable file. |
| [**Reset()**](https://github.com/Miiraak/BlobPE/blob/93ca37b4ddd975f8e3af7fc840a491fb35f2035b/BlobPE/Blob.cs#L117) | Clears the blob data, removing all value, keeping the key and size value. |
| [**Delete()**](https://github.com/Miiraak/BlobPE/blob/93ca37b4ddd975f8e3af7fc840a491fb35f2035b/BlobPE/Blob.cs#L133) | Deletes the blob data from the executable file, removing all traces of the stored data. |
> You can find a simple options setting save application example at [BlobPOC](https://github.com/Miiraak/BlobPOC).

## Internal mechanics
![img](https://github.com/Miiraak/BlobPE/blob/master/.github/img/Internal_Interaction.png)
> Can probably be clearer or better, but give you a little idea of the workflow. 

## Contributing
Contributions are welcome! To contribute to this project:

1. Fork the repository.
2. Create a new branch (`git checkout -b my-feature`).
3. Make your changes.
4. Commit (`git commit -m 'Add my feature'`).
5. Push to your branch (`git push origin my-feature`).
6. Open a Pull Request.

## Issues and Suggestions
If you encounter issues or have suggestions for improvements, please open a ticket via the [GitHub issue tracker](https://github.com/Miiraak/BlobPE/issues).

## License
This project is licensed under the MIT License. See the [LICENSE](./LICENSE) file for details.

## Authors
- [**Miiraak**](https://github.com/miiraak) – *Lead Developer*
