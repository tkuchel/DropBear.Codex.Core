using DropBear.Codex.Core.BaseModels;

namespace DropBear.Codex.Core.Models;

/// <summary>
///     Mutable version of the secure wrapper that allows changes to the data.
///     It requires re-validation of the hash after any modification to ensure integrity.
/// </summary>
/// <typeparam name="T">Generic type to be wrapped for security.</typeparam>
public class MutableSecureWrapper<T>(T data) : SecureWrapperBase<T>(data)
{
    /// <summary>
    ///     Updates the data and automatically generates a new hash.
    /// </summary>
    /// <param name="newData">The new data to be wrapped.</param>
    public void SetData(T newData)
    {
        Data = newData;
        Hash = GenerateHash(Data);
    }

    /// <summary>
    ///     Retrieves the current data.
    /// </summary>
    /// <returns>The wrapped data.</returns>
    public T GetData() => Data;
}
