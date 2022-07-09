using Discord;
using Microsoft.Extensions.Caching.Memory;

namespace TenberBot.Extensions
{
    public static class CacheExtensions
    {
        public static TItem Get<TItem>(this IMemoryCache cache, object id, object key) => cache.Get<TItem>($"{id}, {key}");
        public static TItem Get<TItem>(this IMemoryCache cache, IEntity<ulong> entity, object key) => cache.Get<TItem>(entity.Id, key);

        public static bool TryGetValue<TItem>(this IMemoryCache cache, object id, object key, out TItem value) => cache.TryGetValue($"{id}, {key}", out value);
        public static bool TryGetValue<TItem>(this IMemoryCache cache, IEntity<ulong> entity, object key, out TItem value) => cache.TryGetValue(entity.Id, key, out value);

        public static TItem Set<TItem>(this IMemoryCache cache, object id, object key, TItem value) => cache.Set($"{id}, {key}", value);
        public static TItem Set<TItem>(this IMemoryCache cache, IEntity<ulong> entity, object key, TItem value) => cache.Set(entity.Id, key, value);
    }
}
