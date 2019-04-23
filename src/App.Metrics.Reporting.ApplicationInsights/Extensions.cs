namespace App.Metrics.Reporting.ApplicationInsights
{
    using App.Metrics.Apdex;
    using App.Metrics.Counter;
    using App.Metrics.Histogram;
    using App.Metrics.Meter;
    using Microsoft.ApplicationInsights.DataContracts;
    using System;

    internal static class Extensions
    {
        private const string UnitKey = "unit";

        internal static string ToShortString(this TimeUnit unit)
        {
            switch (unit)
            {
                case TimeUnit.Days: return "d";
                case TimeUnit.Hours: return "h";
                case TimeUnit.Minutes: return "m";
                case TimeUnit.Seconds: return "s";
                case TimeUnit.Milliseconds: return "ms";
                case TimeUnit.Microseconds: return "us";
                case TimeUnit.Nanoseconds: return "ns";
                default: return "n/a";
            }
        }

        internal static void CopyTo(this MetricTags tags, MetricTelemetry mt)
        {
            for (int i = 0; i < tags.Count; i++)
            {
                mt.Properties[tags.Keys[i]] = tags.Values[i];
            }
        }

        internal static void CopyTo(this CounterValue value, MetricTelemetry mt)
        {
            mt.Sum = value.Count;
        }

        internal static void ForwardTo(this CounterValue.SetItem value, MetricTelemetry mt)
        {
            mt.Sum = value.Count;
            value.Tags.CopyTo(mt);
        }

        internal static void CopyTo(this HistogramValue value, MetricTelemetry mt)
        {
            mt.Sum = value.Sum;
            mt.Count = Convert.ToInt32(value.Count);
            mt.Min = value.Min;
            mt.Max = value.Max;
            mt.StandardDeviation = value.StdDev;
        }

        internal static void CopyTo(this MeterValue value, MetricTelemetry mt, string unit)
        {
            mt.Sum = value.MeanRate;
            mt.Properties[UnitKey] = unit;
        }

        internal static void CopyTo(this MeterValue.SetItem value, MetricTelemetry mt, string unit)
        {
            mt.Sum = value.Value.MeanRate;
            mt.Properties[UnitKey] = unit;
            value.Tags.CopyTo(mt);
        }

        internal static void CopyTo(this ApdexValue value, MetricTelemetry mt)
        {
            var all = value.Satisfied + value.Tolerating + value.Frustrating;
            mt.Sum = (value.Satisfied + ((double)value.Tolerating / 2)) / all;
        }
    }
}
