using DropBear.Codex.Core.BaseModels;

namespace DropBear.Codex.Core.Models;

/// <summary>
///     Immutable version of the secure wrapper that does not allow changes to the data after instantiation.
/// </summary>
/// <typeparam name="T">Generic type to be wrapped for security.</typeparam>
public class ImmutableSecureWrapper<T>(T data) : SecureWrapperBase<T>(data)
{
    public T GetData() => Data;
}
