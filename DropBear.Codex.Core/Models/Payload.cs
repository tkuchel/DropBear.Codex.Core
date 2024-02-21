using System.Security.Cryptography;
using MessagePack;

namespace DropBear.Codex.Core.Models;

[MessagePackObject]
public class Payload<T>
{
    [Key(1)] private readonly byte[] _checksum;

    [IgnoreMember] private readonly RSA _cryptoKey;

    [Key(2)] private readonly byte[] _signature;

    [Key(0)] private T _data;

    [Key(3)] private byte[] _exportedPublicKey;

    public Payload(T data)
    {
        _data = data;
        _checksum = ComputeChecksum(data);
        _signature = SignChecksum(_checksum);
        _cryptoKey = RSA.Create();
        _exportedPublicKey = _cryptoKey.ExportRSAPublicKey();
    }

    [SerializationConstructor]
    internal Payload(T data, byte[] checksum, byte[] signature, byte[] exportedPublicKey)
    {
        unsafe
        {
            _exportedPublicKey = exportedPublicKey;

            fixed (byte* ptr = exportedPublicKey)
            {
                _cryptoKey.ImportRSAPublicKey(
                    new ReadOnlySpan<byte>(ptr, exportedPublicKey.Length),
                    out _);
            }
        }
        
        _data = data;
        _checksum = checksum;
        _signature = signature;
    }
    

    [Key(0)]
    public T Data
    {
        get => _data;
        internal set
        {
            var newChecksum = ComputeChecksum(value);
            if (!VerifyChecksum(newChecksum))
                throw new InvalidOperationException("Checksum mismatch");

            _data = value;
        }
    }

    public byte[] GetChecksum()
    {
        return _checksum;
    }

    public byte[] GetSignature()
    {
        return _signature;
    }

    public bool VerifyIntegrity()
    {
        var computedChecksum = ComputeChecksum(_data);
        return VerifyChecksum(computedChecksum) &&
               VerifySignature(computedChecksum);
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
        return _cryptoKey.SignData(checksum,
            HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    }

    private bool VerifySignature(byte[] checksum)
    {
        return _cryptoKey.VerifyData(checksum, _signature,
            HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    }
}