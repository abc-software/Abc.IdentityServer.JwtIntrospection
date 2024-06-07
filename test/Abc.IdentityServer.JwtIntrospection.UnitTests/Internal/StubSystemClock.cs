namespace Microsoft.AspNetCore.Authentication
{
    internal class StubSystemClock : ISystemClock
    {
        public Func<DateTime> UtcNowFunc = () => DateTime.UtcNow;
        public DateTimeOffset UtcNow => new DateTimeOffset(UtcNowFunc());
    }
}