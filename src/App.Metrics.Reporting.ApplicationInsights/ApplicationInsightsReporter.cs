namespace App.Metrics.Reporting.ApplicationInsights
{
    using App.Metrics.Filters;
    using App.Metrics.Formatters;
    using App.Metrics.Logging;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Extensibility;
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    public sealed class ApplicationInsightsReporter : IReportMetrics, IDisposable
    {
        private static readonly ILog Logger = LogProvider.For<ApplicationInsightsReporter>();

        /// <summary>
        /// Suprisingly <see cref="TelemetryConfiguration"/> implements <see cref="IDisposable"/> unlike <see cref="TelemetryClient"/>.
        /// https://github.com/Microsoft/ApplicationInsights-dotnet/blob/develop/src/Microsoft.ApplicationInsights/Extensibility/TelemetryConfiguration.cs#L340
        /// </summary>
        private readonly TelemetryConfiguration clientCfg;
        private readonly TelemetryClient client;
        private readonly IMetricsTranslator translator;
        private bool disposed;

        /// <inheritdoc />
        public IFilterMetrics Filter { get; set; }

        /// <inheritdoc />
        public TimeSpan FlushInterval { get; set; }

        /// <inheritdoc />
        public IMetricsOutputFormatter Formatter { get; set; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ApplicationInsightsReporter"/> class.
        /// </summary>
        /// <param name="options">
        ///     Configuration for <see cref="ApplicationInsightsReporter"/>.
        /// </param>
        /// <param name="translator"></param>
        public ApplicationInsightsReporter(ApplicationInsightsReporterOptions options, IMetricsTranslator translator)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            this.translator = translator ?? throw new ArgumentNullException(nameof(translator));
            clientCfg = new TelemetryConfiguration(options.InstrumentationKey);
            client = new TelemetryClient(clientCfg);
            FlushInterval = options.FlushInterval > TimeSpan.Zero
                ? options.FlushInterval
                : AppMetricsConstants.Reporting.DefaultFlushInterval;
            Filter = options.Filter;

            Logger.Info($"Using metrics reporter {nameof(ApplicationInsightsReporter)}. FlushInterval: {FlushInterval}");
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            clientCfg.Dispose();

            disposed = true;
        }

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
                var context = Filter != null ? ctx.Filter(Filter) : ctx;
                foreach (var mt in translator.Translate(context, now))
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
    }
}
