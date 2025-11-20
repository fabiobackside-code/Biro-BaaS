using System.Collections.Concurrent;

namespace Biro.Blocks.Authentication;

public class InMemoryUserStore
{
    public ConcurrentDictionary<string, User> Users { get; } = new();
}
