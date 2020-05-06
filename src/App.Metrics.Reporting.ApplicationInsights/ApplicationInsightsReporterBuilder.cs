namespace App.Metrics
{
    using App.Metrics.Builder;
    using App.Metrics.Reporting.ApplicationInsights;
    using System;

    /// <summary>
    ///     Builder for configuring Azure Application Insights reporting using an <see cref="IMetricsReportingBuilder" />.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "ApplicationInsightsReporter")]
    public static class ApplicationInsightsReporterBuilder
    {
        /// <summary>
        ///     Add the <see cref="ApplicationInsightsReporter" /> allowing metrics to be reported to Azure Application Insights.
        /// </summary>
        /// <param name="reportingBuilder">
        ///     The <see cref="IMetricsReportingBuilder" /> used to configure metrics reporters.
        /// </param>
        /// <param name="options">The reporting options to use.</param>
        /// <returns>
        ///     An <see cref="IMetricsBuilder" /> that can be used to further configure App Metrics.
        /// </returns>
        public static IMetricsBuilder ToApplicationInsights(
            this IMetricsReportingBuilder reportingBuilder,
            ApplicationInsightsReporterOptions options)
        {
            if (reportingBuilder == null)
            {
                throw new ArgumentNullException(nameof(reportingBuilder));
            }

            var reporter = new ApplicationInsightsReporter(options);

            return reportingBuilder.Using(reporter);
        }

        /// <summary>
        ///     Add the <see cref="ApplicationInsightsReporter" /> allowing metrics to be reported to Azure Application Insights.
        /// </summary>
        /// <param name="reportingBuilder">
        ///     The <see cref="IMetricsReportingBuilder" /> used to configure metrics reporters.
        /// </param>
        /// <param name="setupAction">The reporting options to use.</param>
        /// <returns>
        ///     An <see cref="IMetricsBuilder" /> that can be used to further configure App Metrics.
        /// </returns>
        public static IMetricsBuilder ToApplicationInsights(
            this IMetricsReportingBuilder reportingBuilder,
            Action<ApplicationInsightsReporterOptions> setupAction)
        {
            if (reportingBuilder == null)
            {
                throw new ArgumentNullException(nameof(reportingBuilder));
            }

            var options = new ApplicationInsightsReporterOptions();
            setupAction?.Invoke(options);
            var reporter = new ApplicationInsightsReporter(options);

            return reportingBuilder.Using(reporter);
        }

        /// <summary>
        ///     Add the <see cref="ApplicationInsightsReporter" /> allowing metrics to be reported to Azure Application Insights.
        /// </summary>
        /// <param name="reportingBuilder">
        ///     The <see cref="IMetricsReportingBuilder" /> used to configure metrics reporters.
        /// </param>
        /// <param name="instrumentationKey">Application Insights instrumentation key.</param>
        /// <returns>
        ///     An <see cref="IMetricsBuilder" /> that can be used to further configure App Metrics.
        /// </returns>
        public static IMetricsBuilder ToApplicationInsights(this IMetricsReportingBuilder reportingBuilder, string instrumentationKey)
        {
            if (reportingBuilder == null)
            {
                throw new ArgumentNullException(nameof(reportingBuilder));
            }

            var options = new ApplicationInsightsReporterOptions
            {
                InstrumentationKey = instrumentationKey,
            };

            var reporter = new ApplicationInsightsReporter(options);

            return reportingBuilder.Using(reporter);
        }
    }
}
