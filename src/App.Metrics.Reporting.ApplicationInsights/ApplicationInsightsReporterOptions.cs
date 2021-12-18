namespace App.Metrics.Reporting.ApplicationInsights
{
    using App.Metrics.Filters;
    using Microsoft.ApplicationInsights.Channel;
    using System;

    /// <summary>
    /// Provides programmatic configuration of Microsoft Application Insights reporting in the App Metrics framework.
    /// </summary>
    public class ApplicationInsightsReporterOptions
    {
        /// <summary>
        ///     Application Insights instrumentation key.
        /// </summary>
        public string InstrumentationKey { get; set; } = string.Empty;

        /// <summary>
        ///     Application Insights telemetry channel to provide with this configuration instance.
        /// </summary>
        public ITelemetryChannel? ITelemetryChannel { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IFilterMetrics" /> to use for just this reporter.
        /// </summary>
        public IFilterMetrics? Filter { get; set; }

        /// <summary>
        /// Gets or sets the interval between flushing metrics.
        /// </summary>
        public TimeSpan FlushInterval { get; set; } = TimeSpan.FromSeconds(60);

        /// <summary>
        /// If <code>true</code> the <see cref="ItemsAsCustomPropertyMetricsTranslator"/> will be used to translate metrics;
        /// otherwise <see cref="DefaultMetricsTranslator"/>.
        /// </summary>
        public bool ItemsAsCustomDimensions { get; set; }

        /// <summary>
        /// Default custom property name for AppMetrics' items in Application Insights; the default is 'item'.
        /// </summary>
        public string DefaultCustomDimensionName { get; set; } = "item";
    }
}
