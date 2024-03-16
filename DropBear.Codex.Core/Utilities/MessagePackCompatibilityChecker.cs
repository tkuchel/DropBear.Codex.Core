using System.Collections.Concurrent;
using System.Reflection;
using MessagePack;

namespace DropBear.Codex.Core.Utilities;

/// <summary>
///     Provides utilities for ensuring types are compatible with MessagePack serialization.
/// </summary>
public static class MessagePackCompatibilityChecker
{
    // Cache to store compatibility check results to improve performance.
    private static readonly ConcurrentDictionary<Type, bool> CompatibilityCache = new();

    /// <summary>
    ///     Ensures that a type is compatible with MessagePack serialization.
    ///     This includes being public, not nested, having a MessagePackObject attribute,
    ///     and all serializable members having a Key attribute or being marked to ignore.
    /// </summary>
    /// <typeparam name="T">The type to check for MessagePack compatibility.</typeparam>
    /// <exception cref="InvalidOperationException">Thrown if the type is not compatible with MessagePack serialization.</exception>
    public static void EnsureMessagePackCompatibility<T>()
    {
        var type = typeof(T);

        // Check cache first to avoid repeated reflection.
        if (CompatibilityCache.TryGetValue(type, out var isCompatible) &&
            isCompatible) return; // Type is already verified as compatible, no need to check again.

        // Ensure the type is public and not nested.
        if (!type.IsPublic || type.IsNested)
            throw new InvalidOperationException($"Type {type.Name} must be public and not nested.");

        // Ensure the type has a MessagePackObject attribute.
        var messagePackObjectAttribute = type.GetCustomAttribute<MessagePackObjectAttribute>();
        if (messagePackObjectAttribute is null)
            throw new InvalidOperationException($"Type {type.Name} must have a MessagePackObject attribute.");

        // Collect all properties and fields that should be serialized.
        var members = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Cast<MemberInfo>()
            .Concat(type.GetFields(BindingFlags.Public | BindingFlags.Instance));

        var membersWithoutKey = (from member in members let ignoreAttribute = member.GetCustomAttribute<IgnoreMemberAttribute>() where ignoreAttribute is null let keyAttribute = member.GetCustomAttribute<KeyAttribute>() where keyAttribute is null select member.Name).ToList();

        // If any members are missing a Key attribute and are not ignored, report them in an exception.
        if (membersWithoutKey.Count is not 0)
            throw new InvalidOperationException(
                $"The following members in type {type.Name} must have a Key attribute for MessagePack serialization or be marked with IgnoreMember: {string.Join(", ", membersWithoutKey)}.");

        // Cache the result as the type is compatible.
        CompatibilityCache[type] = true;
    }
}
