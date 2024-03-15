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
    // Static dictionary for caching integrity check results.
    private readonly Dictionary<string, bool> _integrityCheckCache = new(StringComparer.OrdinalIgnoreCase);
    [Key(1)] private readonly byte[] _checksum;

    [Key(3)] private readonly byte[]? _exportedPublicKey;

    [Key(2)] private readonly byte[]? _signature;

    [IgnoreMember] private RSA? _cryptoKey;

    public Payload(T data)
    {
        Data = data;
        _checksum = ComputeChecksum(data);
        _signature = SignChecksum(_checksum);
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
            _cryptoKey ??= RSA.Create(); // Lazy initialization using null-coalescing assignment operator
            if (_exportedPublicKey is not null && _cryptoKey.KeyExchangeAlgorithm is null)
                _cryptoKey.ImportRSAPublicKey(_exportedPublicKey, out _);
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

        // Check cache first if enabled
        if (useCache && _integrityCheckCache.TryGetValue(checksumString, out var cachedResult)) return cachedResult;

        // Perform signature verification and cache the result
        var result = _signature is not null &&
                     CryptoKey.VerifyData(_checksum, _signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

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
}
