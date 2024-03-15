using Blake2Fast;
using MessagePack;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;

namespace DropBear.Codex.Core.Models;

[MessagePackObject]
public class Payload<T>
{
    [Key(1)] internal readonly byte[] _checksum;
    [Key(3)] internal readonly byte[] _publicKey;
    [Key(2)] internal readonly byte[] _signature;
    [Key(7)] private readonly byte[] _storedPublicKey;
    
    // Constructor for message pack deserialization
    public Payload(T data, byte[] checksum, byte[] signature, byte[] publicKey, long timestamp, string? dataType, Guid payloadId)
    {
        Data = data;
        _checksum = checksum;
        _signature = signature;
        _publicKey = publicKey;
        _storedPublicKey = publicKey; // Assuming storedPublicKey is always the same as publicKey upon deserialization
        Timestamp = timestamp;
        DataType = dataType;
        PayloadId = payloadId;
    }

    public Payload(T data, Ed25519PrivateKeyParameters privateKey)
    {
        Data = data;
        _checksum = ComputeChecksum(data);
        _signature = SignData(_checksum, privateKey);
        _publicKey = privateKey.GeneratePublicKey().GetEncoded();
        _storedPublicKey = _publicKey; // Store the public key
        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        DataType = typeof(T).Name;
        PayloadId = Guid.NewGuid();
    }

    [field: Key(0)] public T Data { get; }

    [field: Key(4)] public long Timestamp { get; }

    [field: Key(5)] public string? DataType { get; }

    [field: Key(6)] public Guid PayloadId { get; }

    public bool ValidateIntegrity()
    {
        var publicKeyParam = new Ed25519PublicKeyParameters(_storedPublicKey, 0);
        var verifier = new Ed25519Signer();
        verifier.Init(forSigning: false, publicKeyParam);
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
        signer.Init(forSigning: true, privateKey);
        signer.BlockUpdate(data, 0, data.Length);
        return signer.GenerateSignature();
    }
}
