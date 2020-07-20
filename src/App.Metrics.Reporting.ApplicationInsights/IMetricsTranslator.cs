namespace App.Metrics.Reporting.ApplicationInsights
{
    using Microsoft.ApplicationInsights.DataContracts;
    using System;
    using System.Collections.Generic;

    public interface IMetricsTranslator
    {
        IEnumerable<MetricTelemetry> Translate(MetricsContextValueSource ctx, DateTimeOffset now);
    }
}
