using DropBear.Codex.Core.Models;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace DropBear.Codex.Core.Factories;

/// <summary>
///     Provides functionalities to create instances of Payload with a globally set or auto-generated Ed25519 private key.
/// </summary>
public static class PayloadFactory
{
    private static readonly object Lock = new();
    private static Ed25519PrivateKeyParameters? s_privateKey;

    /// <summary>
    ///     Generates an Ed25519 private key using a secure random source.
    /// </summary>
    /// <returns>The generated Ed25519 private key.</returns>
    private static Ed25519PrivateKeyParameters GenerateEd25519PrivateKey()
    {
        var keyGenerationParameters = new Ed25519KeyGenerationParameters(new SecureRandom());
        var keyPairGenerator = new Ed25519KeyPairGenerator();
        keyPairGenerator.Init(keyGenerationParameters);
        var keyPair = keyPairGenerator.GenerateKeyPair();
        return (Ed25519PrivateKeyParameters)keyPair.Private;
    }

    /// <summary>
    ///     Initializes the factory with a specific Ed25519 private key or generates one if null is provided.
    /// </summary>
    /// <param name="privateKey">The Ed25519 private key to use for payload creation. If null, a new key will be generated.</param>
    public static void Initialize(Ed25519PrivateKeyParameters? privateKey = null)
    {
        if (privateKey is null)
            lock (Lock)
            {
                s_privateKey ??= GenerateEd25519PrivateKey();
            }
        else
            s_privateKey = privateKey;
    }

    /// <summary>
    ///     Creates a new instance of Payload with the given data, using the initialized or auto-generated private key.
    /// </summary>
    /// <typeparam name="T">The type of the data to be encapsulated in the payload.</typeparam>
    /// <param name="data">The data to encapsulate in the payload.</param>
    /// <returns>A new instance of Payload encapsulating the specified data.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the private key has not been initialized and cannot be generated.</exception>
    public static Payload<T> Create<T>(T data) where T : notnull
    {
        // If the private key is already set, use it to create the payload.
        if (s_privateKey is not null)
            return new Payload<T>(data, s_privateKey);
        
        // If the private key is not set, generate a new one and use it to create the payload.
        lock (Lock)
        {
            s_privateKey = GenerateEd25519PrivateKey();
        }

        // Create a new payload using the generated private key.
        return new Payload<T>(data, s_privateKey);
    }
}
