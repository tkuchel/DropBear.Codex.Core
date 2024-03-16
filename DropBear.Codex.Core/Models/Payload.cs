using Blake2Fast;
using MessagePack;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;

namespace DropBear.Codex.Core.Models;

/// <summary>
///     Represents a payload encapsulating data of type <typeparamref name="T" /> with cryptographic integrity checks.
/// </summary>
[MessagePackObject]
public class Payload<T>
{
    [Key(1)] internal readonly byte[] _checksum;
    [Key(3)] internal readonly byte[] _publicKey;
    [Key(2)] internal readonly byte[] _signature;
    [Key(7)] private readonly byte[] _storedPublicKey;

    /// <summary>
    ///     Constructor for deserialization.
    /// </summary>
    public Payload(T data, byte[] checksum, byte[] signature, byte[] publicKey, long timestamp, string dataType,
        Guid payloadId)
    {
        Data = data ?? throw new ArgumentNullException(nameof(data));
        _checksum = checksum ?? throw new ArgumentNullException(nameof(checksum));
        _signature = signature ?? throw new ArgumentNullException(nameof(signature));
        _publicKey = publicKey ?? throw new ArgumentNullException(nameof(publicKey));
        _storedPublicKey = publicKey;
        Timestamp = timestamp;
        DataType = dataType ?? throw new ArgumentNullException(nameof(dataType));
        PayloadId = payloadId;
    }

    /// <summary>
    ///     Constructor for creating a new payload with data and a private key.
    /// </summary>
    public Payload(T data, Ed25519PrivateKeyParameters privateKey)
    {
        ArgumentNullException.ThrowIfNull(privateKey);

        Data = data ?? throw new ArgumentNullException(nameof(data));
        _checksum = ComputeChecksum(data);
        _signature = SignData(_checksum, privateKey);
        _publicKey = privateKey.GeneratePublicKey().GetEncoded();
        _storedPublicKey = _publicKey;
        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        DataType = typeof(T).Name;
        PayloadId = Guid.NewGuid();
    }

    [Key(0)] public T Data { get; }
    [Key(4)] public long Timestamp { get; }
    [Key(5)] public string DataType { get; }
    [Key(6)] public Guid PayloadId { get; }

    /// <summary>
    ///     Validates the integrity of the payload using the stored public key.
    /// </summary>
    public bool ValidateIntegrity()
    {
        var publicKeyParam = new Ed25519PublicKeyParameters(_storedPublicKey, 0);
        var verifier = new Ed25519Signer();
        verifier.Init(false, publicKeyParam);
        verifier.BlockUpdate(_checksum, 0, _checksum.Length);
        return verifier.VerifySignature(_signature);
    }

    private static byte[] ComputeChecksum(T data)
    {
        var serializedData = MessagePackSerializer.Serialize(data);
        return Blake2b.ComputeHash(serializedData);
    }

    private static byte[] SignData(byte[] data, ICipherParameters privateKey)
    {
        var signer = new Ed25519Signer();
        signer.Init(true, privateKey ?? throw new ArgumentNullException(nameof(privateKey)));
        signer.BlockUpdate(data, 0, data.Length);
        return signer.GenerateSignature();
    }
}
