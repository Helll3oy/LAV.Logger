using System;

namespace LAV.Logger
{
    public static class UnixTimestampExtensions
    {
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static DateTime ToDateTime(this long unixTimestamp)
        {
            return UnixEpoch.AddSeconds(unixTimestamp);
        }

        public static long ToUnixTimestamp(this DateTime dateTime)
        {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
            return new DateTimeOffset(dateTime).ToUnixTimeSeconds();
#else
            return (long)(dateTime.ToUniversalTime() - UnixEpoch).TotalSeconds;
#endif
        }

    }
}
