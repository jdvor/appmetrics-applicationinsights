namespace App.Metrics.Reporting.ApplicationInsights
{
    using App.Metrics.Apdex;
    using App.Metrics.Counter;
    using App.Metrics.Filters;
    using App.Metrics.Formatters;
    using App.Metrics.Histogram;
    using App.Metrics.Logging;
    using App.Metrics.Meter;
    using App.Metrics.Timer;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Extensibility;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    public sealed class ApplicationInsightsReporter : IReportMetrics
    {
        private const string UnitKey = "unit";
        private static readonly ILog Logger = LogProvider.For<ApplicationInsightsReporter>();
        private readonly TelemetryClient client;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ApplicationInsightsReporter"/> class.
        /// </summary>
        /// <param name="options">
        ///     Configuration for <see cref="ApplicationInsightsReporter"/>.
        /// </param>
        public ApplicationInsightsReporter(ApplicationInsightsReporterOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            client = new TelemetryClient(new TelemetryConfiguration(options.InstrumentationKey));
            FlushInterval = options.FlushInterval > TimeSpan.Zero
                ? options.FlushInterval
                : AppMetricsConstants.Reporting.DefaultFlushInterval;
            Filter = options.Filter;

            Logger.Info($"Using Metrics Reporter {this}. FlushInterval: {FlushInterval}");
        }

        /// <inheritdoc />
        public IFilterMetrics Filter { get; set; }

        /// <inheritdoc />
        public TimeSpan FlushInterval { get; set; }

        /// <inheritdoc />
        public IMetricsOutputFormatter Formatter { get; set; }

        /// <inheritdoc />
        public Task<bool> FlushAsync(MetricsDataValueSource metricsData, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested || metricsData == null)
            {
                return Task.FromResult(false);
            }

            var sw = Stopwatch.StartNew();
            var now = DateTimeOffset.Now;
            var count = 0;
            foreach (var ctx in metricsData.Contexts)
            {
                foreach (var mt in TranslateContext(ctx, now))
                {
                    // Although the method comment suggest it is internal and should not be used,
                    // the documentation here https://docs.microsoft.com/en-us/azure/azure-monitor/app/api-custom-events-metrics
                    // suggest this is exactly the method you should use when metrics pre-aggregation is done by your code.
                    client.Track(mt);
                    ++count;
                }
            }

            if (count > 0)
            {
                client.Flush();
                Logger.Trace($"Flushed TelemetryClient; {count} records; elapsed: {sw.Elapsed}.");
            }

            return Task.FromResult(true);
        }

        private IEnumerable<MetricTelemetry> TranslateContext(MetricsContextValueSource ctx, DateTimeOffset now)
        {
            var context = Filter != null ? ctx.Filter(Filter) : ctx;
            var contextName = context.Context;

            foreach (var source in context.ApdexScores)
            {
                yield return Translate(source, contextName, now);
            }

            foreach (var source in context.Counters)
            {
                foreach (var mt in Translate(source, contextName, now))
                {
                    yield return mt;
                }
            }

            foreach (var source in context.Gauges)
            {
                var mt = MetricFactory.CreateMetric(source, contextName, now);
                mt.Sum = source.Value;
                yield return mt;
            }

            foreach (var source in context.Histograms)
            {
                yield return Translate(source, contextName, now);
            }

            foreach (var source in context.Meters)
            {
                foreach (var mt in Translate(source, contextName, now))
                {
                    yield return mt;
                }
            }

            foreach (var source in context.Timers)
            {
                foreach (var mt in Translate(source, contextName, now))
                {
                    yield return mt;
                }
            }
        }

        private static MetricTelemetry Translate(ApdexValueSource source, string contextName, DateTimeOffset now)
        {
            var mt = MetricFactory.CreateMetric(source, contextName, now);
            source.Value.CopyTo(mt);
            return mt;
        }

        private static IEnumerable<MetricTelemetry> Translate(CounterValueSource source, string contextName, DateTimeOffset now)
        {
            var mt = MetricFactory.CreateMetric(source, contextName, now);
            source.Value.CopyTo(mt);
            yield return mt;

            foreach (var item in source.Value.Items)
            {
                mt = MetricFactory.CreateMetric(source, contextName, now, item.Item);
                item.ForwardTo(mt);
                yield return mt;
            }
        }

        private static MetricTelemetry Translate(HistogramValueSource source, string contextName, DateTimeOffset now)
        {
            var mt = MetricFactory.CreateMetric(source, contextName, now);
            source.Value.CopyTo(mt);
            return mt;
        }

        private static IEnumerable<MetricTelemetry> Translate(MeterValueSource source, string contextName, DateTimeOffset now)
        {
            var unit = source.Value.RateUnit.ToShortString();
            var mt = MetricFactory.CreateMetric(source, contextName, now);
            source.Value.CopyTo(mt, unit);
            yield return mt;

            foreach (var item in source.Value.Items)
            {
                mt = MetricFactory.CreateMetric(source, contextName, now, item.Item);
                item.CopyTo(mt, unit);
                yield return mt;
            }
        }

        private static IEnumerable<MetricTelemetry> Translate(TimerValueSource source, string contextName, DateTimeOffset now)
        {
            var mt = MetricFactory.CreateMetric(source, contextName, now);
            mt.Properties[UnitKey] = source.DurationUnit.ToShortString();
            source.Value.Histogram.CopyTo(mt);
            yield return mt;

            var unit = source.Value.Rate.RateUnit.ToShortString();
            mt = MetricFactory.CreateMetric(source, contextName, now, "rate");
            source.Value.Rate.CopyTo(mt, unit);
            yield return mt;

            foreach (var item in source.Value.Rate.Items)
            {
                mt = MetricFactory.CreateMetric(source, contextName, now, item.Item);
                item.CopyTo(mt, unit);
                yield return mt;
            }
        }
    }
}
