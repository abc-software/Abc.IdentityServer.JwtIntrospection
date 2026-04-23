#if NET8_0_OR_GREATER && DUENDE

namespace System
{
    public static class DateTimeExtensions
    {
        //
        // Summary:
        //     Converts the given date value to epoch time.
        public static long ToEpochTime(this DateTime dateTime)
        {
            return (dateTime.ToUniversalTime().Ticks - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).Ticks) / 10000000;
        }

        //
        // Summary:
        //     Converts the given epoch time to a System.DateTime with System.DateTimeKind.Utc
        //     kind.
        public static DateTime ToDateTimeFromEpoch(this long date)
        {
            long value = date * 10000000;
            return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddTicks(value);
        }
    }
}

#endif