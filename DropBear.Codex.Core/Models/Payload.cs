using System.Security.Cryptography;
using MessagePack;

namespace DropBear.Codex.Core.Models;

/// <summary>
///     Represents the payload containing the data and its cryptographic verification elements.
/// </summary>
/// <typeparam name="T">The type of the data.</typeparam>
[MessagePackObject]
public class Payload<T>
{
    [Key(1)] private readonly byte[] _checksum;

    [Key(3)] private readonly byte[]? _exportedPublicKey;
    private readonly Dictionary<string, bool> _integrityCheckCache = new(StringComparer.OrdinalIgnoreCase);

    [Key(2)] private readonly byte[]? _signature;

    [IgnoreMember] private RSA? _cryptoKey;

    public Payload(T data)
    {
        Data = data;
        _checksum = ComputeChecksum(data);
        _signature = SignChecksum(_checksum);
        // Ensure CryptoKey is accessed to force initialization before exporting the public key.
        _exportedPublicKey = CryptoKey.ExportRSAPublicKey();
    }

    [SerializationConstructor]
    public Payload(T data, byte[] checksum, byte[]? signature, byte[]? exportedPublicKey, long timestamp)
    {
        Data = data;
        _checksum = checksum;
        _signature = signature;
        _exportedPublicKey = exportedPublicKey;
        Timestamp = timestamp;
    }

    private RSA CryptoKey
    {
        get
        {
            if (_cryptoKey is not null) return _cryptoKey;
            _cryptoKey = RSA.Create(); // Lazy initialization
            if (_exportedPublicKey is not null) _cryptoKey.ImportRSAPublicKey(_exportedPublicKey, out _);
            return _cryptoKey;
        }
    }

    [Key(0)] public T Data { get; }

    [field: Key(4)] public long Timestamp { get; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    public byte[] GetChecksum() => _checksum;

    public byte[]? GetSignature() => _signature;

    public byte[]? GetExportedPublicKey() => _exportedPublicKey;

    public bool VerifyIntegrity(bool useCache = true)
    {
        var checksumString = Convert.ToBase64String(_checksum);

        if (useCache && _integrityCheckCache.TryGetValue(checksumString, out var cachedResult)) return cachedResult;

        var result = VerifySignature(_checksum);
        if (useCache) _integrityCheckCache[checksumString] = result;

        return result;
    }

    private static byte[] ComputeChecksum(T data)
    {
        var serialized = MessagePackSerializer.Serialize(data);
        return SHA256.HashData(serialized);
    }

    private byte[]? SignChecksum(byte[] checksum) =>
        CryptoKey.SignData(checksum, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

    private bool VerifySignature(byte[] checksum) => _signature is not null &&
                                                     CryptoKey.VerifyData(checksum, _signature,
                                                         HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
}
