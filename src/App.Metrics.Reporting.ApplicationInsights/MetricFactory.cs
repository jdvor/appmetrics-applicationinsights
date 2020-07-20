namespace App.Metrics.Reporting.ApplicationInsights
{
    using Microsoft.ApplicationInsights.DataContracts;
    using System;

    internal static class MetricFactory
    {
        private const string Delim = ".";
        private const string ContextKey = "context";

        internal static string CreateName<T>(MetricValueSourceBase<T> source)
            => source.IsMultidimensional ? source.MultidimensionalName : source.Name;

        internal static string CreateName<T>(MetricValueSourceBase<T> source, string[] dimensions)
        {
            var name = source.IsMultidimensional ? source.MultidimensionalName : source.Name;
            return $"{name}{Delim}{string.Join(Delim, dimensions)}";
        }

        internal static MetricTelemetry CreateMetric(
            string name,
            string contextName,
            DateTimeOffset now)
        {
            var mt = new MetricTelemetry
            {
                Name = name,
                MetricNamespace = contextName,
                Timestamp = now,
            };
            mt.Properties[ContextKey] = contextName;
            return mt;
        }

        internal static MetricTelemetry CreateMetric<T>(
            MetricValueSourceBase<T> source,
            string contextName,
            DateTimeOffset now,
            params string[] dimensions)
        => CreateMetric(CreateName(source, dimensions), contextName, now);

        internal static MetricTelemetry CreateMetric<T>(
            MetricValueSourceBase<T> source,
            string contextName,
            DateTimeOffset now)
        => CreateMetric(CreateName(source), contextName, now);
    }
}
