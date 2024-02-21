using DropBear.Codex.Core.Formatters;
using DropBear.Codex.Core.ReturnTypes;
using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;

namespace DropBear.Codex.Core.Resolvers;

public class ResultResolver<T> : IFormatterResolver where T : notnull
{
    public static readonly ResultResolver<T> Instance = new ResultResolver<T>();

    private ResultResolver() { }

    public IMessagePackFormatter<TFormatter> GetFormatter<TFormatter>()
    {
        if (typeof(TFormatter) == typeof(Result<T>))
        {
            return (IMessagePackFormatter<TFormatter>)(object)new ResultFormatter<T>();
        }

        return StandardResolver.Instance.GetFormatter<TFormatter>();
    }
}