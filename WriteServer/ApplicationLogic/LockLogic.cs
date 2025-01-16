using System.Collections.Concurrent;
using ApplicationLogic.Exceptions;
using RedLockNet;

namespace ApplicationLogic;

public class LockLogic {
    private readonly RedLockManager _redLockManager;
    private static readonly ConcurrentDictionary<string, (IRedLock redLock, DateTime expiration)> _locks = new();

    public LockLogic(RedLockManager redLockManager) {
        _redLockManager = redLockManager;

    }

    static LockLogic() {
        _ = CleanupExpiredLocks();

    }

    public async Task LockResource(string resourceKey, int duration = 10) {
        var redLock = await _redLockManager.GetFactory().CreateLockAsync(
            resourceKey,
            TimeSpan.FromSeconds(duration));
        if (!redLock.IsAcquired) {
            throw new LockTakenException($"Couldn't lock resource {resourceKey} as it is already locked");
        }

        var expirationTime = DateTime.UtcNow.AddSeconds(duration);
        _locks[resourceKey] = (redLock, expirationTime);
    }

    public static async Task ReleaseResource(string resourceKey) {
        if (_locks.TryRemove(resourceKey, out var lockInfo)) {
            await lockInfo.redLock.DisposeAsync();
        }
    }

    private static async Task CleanupExpiredLocks() {
        while (true) {
            await Task.Delay(1000);

            var now = DateTime.UtcNow;
            foreach (var key in _locks.Keys) {
                if (_locks.TryGetValue(key, out var lockInfo)) {
                    if (lockInfo.expiration <= now) {
                        await lockInfo.redLock.DisposeAsync();
                        _locks.TryRemove(key, out _);
                    }
                }
            }
        }
    }

    public static bool IsResourceLocked(string resourceKey) {
        return _locks.ContainsKey(resourceKey);
    }
}
