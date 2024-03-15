using DropBear.Codex.Core.BaseModels;
using MessagePack;

namespace DropBear.Codex.Core.Models;

/// <summary>
/// Immutable version of the secure wrapper that does not allow changes to the data after instantiation.
/// </summary>
/// <typeparam name="T">Generic type to be wrapped for security.</typeparam>
[MessagePackObject]
public class ImmutableSecureWrapper<T> : SecureWrapperBase<T>
{
    public ImmutableSecureWrapper(T data)
    {
        UpdateMetaDataAndHash(data);
    }
    
    public T GetData()
    {
        return Data;
    }
}