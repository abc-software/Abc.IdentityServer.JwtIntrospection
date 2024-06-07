#if DUENDE && NET8_0_OR_GREATER

namespace Duende.IdentityServer
{
    internal class StubClock : IClock
    {
        public Func<DateTime> UtcNowFunc = () => DateTime.UtcNow;
        public DateTimeOffset UtcNow => new DateTimeOffset(UtcNowFunc());
    }
}

#endif