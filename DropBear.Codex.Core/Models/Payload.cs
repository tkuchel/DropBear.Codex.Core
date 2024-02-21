using System.Security.Cryptography;
using MessagePack;

namespace DropBear.Codex.Core.Models;

[MessagePackObject]
public class Payload<T>
{
    [Key(1)] private readonly byte[] _checksum;

    [IgnoreMember] private readonly RSA _cryptoKey = RSA.Create();

    [Key(3)] private readonly byte[] _exportedPublicKey;

    [Key(2)] private readonly byte[] _signature;


    public Payload(T data)
    {
        Data = data;
        _checksum = ComputeChecksum(data);
        _signature = SignChecksum(_checksum);
        _exportedPublicKey = _cryptoKey.ExportRSAPublicKey();
    }

    [SerializationConstructor]
    public Payload(T data, byte[] checksum, byte[] signature, byte[] exportedPublicKey, long timestamp)
    {
        Data = data;
        _checksum = checksum;
        _signature = signature;
        _exportedPublicKey = exportedPublicKey;
        Timestamp = timestamp;
        _cryptoKey.ImportRSAPublicKey(exportedPublicKey, out _);
    }

    [Key(0)] [field: Key(0)] public T Data { get; }

    [field: Key(4)] public long Timestamp { get; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    public byte[] GetChecksum()
    {
        return _checksum;
    }

    public byte[] GetSignature()
    {
        return _signature;
    }

    public byte[] GetExportedPublicKey()
    {
        return _exportedPublicKey;
    }

    public bool VerifyIntegrity()
    {
        var computedChecksum = ComputeChecksum(Data);
        return VerifyChecksum(computedChecksum) && VerifySignature(computedChecksum);
    }

    private bool VerifyChecksum(byte[] checksum)
    {
        return _checksum.SequenceEqual(checksum);
    }

    private byte[] ComputeChecksum(T data)
    {
        var serialized = MessagePackSerializer.Serialize(data);
        return SHA256.HashData(serialized);
    }

    private byte[] SignChecksum(byte[] checksum)
    {
        return _cryptoKey.SignData(checksum, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    }

    private bool VerifySignature(byte[] checksum)
    {
        return _cryptoKey.VerifyData(checksum, _signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    }
}