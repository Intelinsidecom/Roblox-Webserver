using System;
using System.Collections.Concurrent;

namespace Website.Services;

public class TypingEntry
{
    public DateTime LastTypingTime { get; set; }
    public bool IsTyping { get; set; }
}

public class TypingTracker
{
    private readonly ConcurrentDictionary<string, TypingEntry> _entries = new();
    private static readonly TimeSpan TypingTimeout = TimeSpan.FromSeconds(5);

    private static string MakeKey(long conversationId, long userId) => $"{conversationId}:{userId}";

    public void UpdateTypingStatus(long conversationId, long userId, bool isTyping)
    {
        var key = MakeKey(conversationId, userId);
        _entries.AddOrUpdate(key,
            _ => new TypingEntry { LastTypingTime = DateTime.UtcNow, IsTyping = isTyping },
            (_, _) => new TypingEntry { LastTypingTime = DateTime.UtcNow, IsTyping = isTyping });
    }

    public bool IsUserTyping(long conversationId, long userId)
    {
        var key = MakeKey(conversationId, userId);
        if (_entries.TryGetValue(key, out var entry))
        {
            if (entry.IsTyping && (DateTime.UtcNow - entry.LastTypingTime) < TypingTimeout)
                return true;
            _entries.TryRemove(key, out _);
        }
        return false;
    }

    public long[] GetTypingUserIds(long conversationId)
    {
        var now = DateTime.UtcNow;
        var result = new System.Collections.Generic.List<long>();

        foreach (var kvp in _entries)
        {
            if (kvp.Key.StartsWith($"{conversationId}:") &&
                kvp.Value.IsTyping &&
                (now - kvp.Value.LastTypingTime) < TypingTimeout)
            {
                var userIdStr = kvp.Key.Substring(kvp.Key.IndexOf(':') + 1);
                if (long.TryParse(userIdStr, out var uid))
                    result.Add(uid);
            }
        }

        return result.ToArray();
    }

    public void CleanupExpired()
    {
        var now = DateTime.UtcNow;
        foreach (var kvp in _entries)
        {
            if (!kvp.Value.IsTyping || (now - kvp.Value.LastTypingTime) > TypingTimeout)
            {
                _entries.TryRemove(kvp.Key, out _);
            }
        }
    }
}
