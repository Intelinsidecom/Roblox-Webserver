using System;
using System.Collections.Concurrent;

namespace Website.Services;

public record PresenceEntry(DateTime LastRequestTime, bool IsStudio, DateTime LastDbWriteTime);

public class PresenceTracker
{
    private readonly ConcurrentDictionary<long, PresenceEntry> _entries = new();

    public void TrackRequest(long userId, bool isStudio)
    {
        var now = DateTime.UtcNow;
        _entries.AddOrUpdate(userId,
            _ => new PresenceEntry(now, isStudio, DateTime.MinValue),
            (_, existing) => new PresenceEntry(now, isStudio, existing.LastDbWriteTime));
    }

    public void MarkFlushed(long userId, DateTime now)
    {
        _entries.AddOrUpdate(userId,
            _ => new PresenceEntry(now, false, now),
            (_, existing) => new PresenceEntry(existing.LastRequestTime, existing.IsStudio, now));
    }

    public bool TryGetValue(long userId, out PresenceEntry? entry)
    {
        entry = null;
        if (_entries.TryGetValue(userId, out var e))
        {
            entry = e;
            return true;
        }
        return false;
    }

    public IReadOnlyCollection<KeyValuePair<long, PresenceEntry>> GetAll() => _entries;

    public bool TryRemove(long userId) => _entries.TryRemove(userId, out _);
}
