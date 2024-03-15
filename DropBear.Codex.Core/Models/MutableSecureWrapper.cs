using DropBear.Codex.Core.BaseModels;
using MessagePack;

namespace DropBear.Codex.Core.Models;

/// <summary>
/// Mutable version of the secure wrapper that allows changes to the data.
/// It requires re-validation of the hash after any modification to ensure integrity.
/// </summary>
/// <typeparam name="T">Generic type to be wrapped for security.</typeparam>
[MessagePackObject]
public class MutableSecureWrapper<T> : SecureWrapperBase<T>
{
    public MutableSecureWrapper(T data)
    {
        UpdateMetaDataAndHash(data);
    }

    /// <summary>
    /// Updates the data and automatically generates a new hash, along with updating other metadata.
    /// </summary>
    /// <param name="newData">The new data to be wrapped.</param>
    public void SetData(T newData)
    {
        UpdateMetaDataAndHash(newData);
    }
    
    public T GetData()
    {
        return Data;
    }
}