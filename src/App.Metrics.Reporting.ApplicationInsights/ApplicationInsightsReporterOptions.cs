namespace App.Metrics.Reporting.ApplicationInsights
{
    using App.Metrics.Filters;
    using System;

    /// <summary>
    /// Provides programmatic configuration of Microsoft Application Insights reporting in the App Metrics framework.
    /// </summary>
    public class ApplicationInsightsReporterOptions
    {
        /// <summary>
        ///     .
        /// </summary>
        public string InstrumentationKey { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IFilterMetrics" /> to use for just this reporter.
        /// </summary>
        public IFilterMetrics Filter { get; set; }

        /// <summary>
        /// Gets or sets the interval between flushing metrics.
        /// </summary>
        public TimeSpan FlushInterval { get; set; } = TimeSpan.FromSeconds(60);
    }
}
