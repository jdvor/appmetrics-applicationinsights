// ReSharper disable once CheckNamespace
namespace App.Metrics;

using App.Metrics.Builder;
using App.Metrics.Reporting.ApplicationInsights;
using System;

/// <summary>
///     Builder for configuring Azure Application Insights reporting using an <see cref="IMetricsReportingBuilder" />.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Reliability",
    "CA2000:Dispose objects before losing scope",
    Justification = "AppMetrics is responsible for disposing reporters; this class is just factory.")]
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
        var translator = options.ItemsAsCustomDimensions
            ? (IMetricsTranslator)new ItemsAsCustomPropertyMetricsTranslator(options.DefaultCustomDimensionName)
            : new DefaultMetricsTranslator();
        var reporter = new ApplicationInsightsReporter(options, translator);

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
        var options = new ApplicationInsightsReporterOptions();
        setupAction.Invoke(options);
        return ToApplicationInsights(reportingBuilder, options);
    }

    /// <summary>
    ///     Add the <see cref="ApplicationInsightsReporter" /> allowing metrics to be reported to Azure Application Insights.
    /// </summary>
    /// <param name="reportingBuilder">
    ///     The <see cref="IMetricsReportingBuilder" /> used to configure metrics reporters.
    /// </param>
    /// <param name="connectionString">Application Insights connection string.</param>
    /// <returns>
    ///     An <see cref="IMetricsBuilder" /> that can be used to further configure App Metrics.
    /// </returns>
    public static IMetricsBuilder ToApplicationInsights(
        this IMetricsReportingBuilder reportingBuilder,
        string connectionString)
    {
        var options = new ApplicationInsightsReporterOptions { ConnectionString = connectionString };
        return ToApplicationInsights(reportingBuilder, options);
    }
}
