using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace DropBear.Codex.Core.BaseModels;

/// <summary>
///     Base class for wrapping a generic type with security features such as hashing for data integrity checks.
/// </summary>
/// <typeparam name="T">Generic type to be wrapped for security.</typeparam>
public abstract class SecureWrapperBase<T>
{
    protected SecureWrapperBase(T data)
    {
        Data = data;
        Hash = GenerateHash(Data);
    }

    protected T Data { get; set; }
    protected string Hash { get; set; }

    /// <summary>
    ///     Generates a SHA256 hash for the given data.
    /// </summary>
    /// <param name="data">Data to hash.</param>
    /// <returns>A hash string.</returns>
    protected string GenerateHash(T data)
    {
        var serializedData = JsonSerializer.Serialize(data);
        var dataBytes = Encoding.UTF8.GetBytes(serializedData);
        var hashBytes = SHA256.HashData(dataBytes);
        return Convert.ToBase64String(hashBytes);
    }

    /// <summary>
    ///     Validates the data against its stored hash.
    /// </summary>
    /// <returns>True if the data matches the hash, false otherwise.</returns>
    public bool Validate()
    {
        var newHash = GenerateHash(Data);
        return Hash == newHash;
    }
}