using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;

namespace DropBear.Codex.Core.Resolvers;

public static class DynamicCompositeResolver
{
    private static readonly List<IFormatterResolver> _customResolvers = new();
    private static readonly object _lock = new();

    static DynamicCompositeResolver()
    {
        // Initialize your custom resolvers here
        _customResolvers.Add(ExitCodeResolver.Instance);
        _customResolvers.Add(ResultResolver.Instance);
        // Add additional custom resolvers here
    }

    private static IFormatterResolver _compositeResolver = CompositeResolver.Create(
        _customResolvers.ToArray() as IReadOnlyList<IMessagePackFormatter>,
        new IFormatterResolver[] { StandardResolver.Instance });

    public static IFormatterResolver GetResolver()
    {
        lock (_lock)
        {
            return _compositeResolver;
        }
    }

    public static void RegisterResolver(IFormatterResolver newResolver)
    {
        lock (_lock)
        {
            _customResolvers.Add(newResolver);
            _compositeResolver = CompositeResolver.Create(
                _customResolvers.ToArray() as IReadOnlyList<IMessagePackFormatter>,
                new IFormatterResolver[] { StandardResolver.Instance });
        }
    }
}