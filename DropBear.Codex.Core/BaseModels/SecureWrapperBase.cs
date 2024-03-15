using System.Text;
using System.Text.Json;
using Blake2Fast;
using MessagePack;

// Path/Filename: /SecureWrapper/SecureWrapperBase.cs

namespace DropBear.Codex.Core.BaseModels;

/// <summary>
///     Base class for wrapping a generic type with security features, including BLAKE2 hashing for data integrity checks.
///     This class automatically sets metadata attributes to prevent misuse or tampering.
/// </summary>
/// <typeparam name="T">Generic type to be wrapped for security.</typeparam>
[MessagePackObject]
public abstract class SecureWrapperBase<T>
{
    [Key(0)] protected T Data { get; set; } = default!;

    [Key(1)] protected string Hash { get; set; } = string.Empty;

    [Key(2)] public DateTime Timestamp { get; private set; }

    [Key(3)] public string DataType => typeof(T).Name;
    
    protected void UpdateMetaDataAndHash(T data)
    {
        Data = data;
        Timestamp = DateTime.UtcNow;
        Hash = GenerateHash(data);
    }

    /// <summary>
    ///     Generates a BLAKE2 hash for the given data.
    /// </summary>
    /// <param name="data">Data to hash.</param>
    /// <returns>A hash string.</returns>
    protected string GenerateHash(T data)
    {
        var serializedData = JsonSerializer.Serialize(data);
        var dataBytes = Encoding.UTF8.GetBytes(serializedData);
        var hashBytes = Blake2b.ComputeHash(dataBytes);
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
