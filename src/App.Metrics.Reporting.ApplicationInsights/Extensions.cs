namespace App.Metrics.Reporting.ApplicationInsights
{
    internal static class Extensions
    {
        internal static string ToShortString(this TimeUnit unit)
        {
            return unit switch
            {
                TimeUnit.Days => "d",
                TimeUnit.Hours => "h",
                TimeUnit.Minutes => "m",
                TimeUnit.Seconds => "s",
                TimeUnit.Milliseconds => "ms",
                TimeUnit.Microseconds => "us",
                TimeUnit.Nanoseconds => "ns",
                _ => "n/a",
            };
        }
    }
}
