using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Nop.Core.Caching;
using Nop.Core.Configuration;

namespace Nop.Services.Caching;

public class MemoryDistributedCacheManager : DistributedCacheManager
{
    private static List<string> _keysList = new List<string>();

    public MemoryDistributedCacheManager(AppSettings appSettings, IDistributedCache distributedCache) : base(appSettings, distributedCache)
    {
    }

    public override async Task RemoveByPrefixAsync(string prefix, params object[] prefixParameters)
    {
        using var _ = _locker.Lock();

        foreach (var key in _keysList.Where(key => key.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase))
                     .ToList())
        {
            await _distributedCache.RemoveAsync(key);
            _keysList.Remove(key);
        }
    }

    public override async Task ClearAsync()
    {
        foreach (var key in _keysList) 
            await _distributedCache.RemoveAsync(key);

        _keysList.Clear();
    }

    protected override void OnUpdateKey(CacheKey key, bool add = true)
    {
        using var _ = _locker.Lock();

        if (add && !_keysList.Contains(key.Key)) 
            _keysList.Add(key.Key);
        else if (!add)
            _keysList.Remove(key.Key);

        base.OnUpdateKey(key, add);
    }
}