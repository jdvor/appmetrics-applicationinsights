namespace SandboxConsoleApp
{
    using App.Metrics;
    using App.Metrics.Reporting;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.Extensions.Configuration;
    using Serilog;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using static SampleMetrics;

    public static class Program
    {
        private static readonly ThreadLocal<Random> Rnd = new ThreadLocal<Random>(() => new Random(Environment.TickCount));
        private static ILogger Logger;
        private static IMetricsRoot metrics;
        private static IRunMetricsReports reporter;
        private static Task[] backgroundTasks;
        private static Stopwatch TotalTime;
        private static int RecordCount;
        private static int FlushCount;

        public static void Main()
        {
            var recordEvery = TimeSpan.FromSeconds(2);
            var reportEvery = TimeSpan.FromSeconds(60);

            Init();

            TotalTime = Stopwatch.StartNew();
            using (var cts = new CancellationTokenSource())
            {
                backgroundTasks = new[]
                {
                    Task.Run(() => Record(recordEvery, cts.Token)),
                    Task.Run(() => Report(reportEvery, cts.Token)),
                };

                PrintHelp(recordEvery, reportEvery);

                ConsoleKey consoleKey;
                do
                {
                    consoleKey = Console.ReadKey().Key;
                    switch (consoleKey)
                    {
                        case ConsoleKey.P:
                            PrintMetricsToConsole();
                            break;

                        case ConsoleKey.R:
                            ReportOnDemand();
                            break;
                    }
                }
                while (consoleKey != ConsoleKey.Escape);

                cts.Cancel();
                Task.WaitAll(backgroundTasks, 5000);
            }

            Console.WriteLine();
            Console.WriteLine($"In {TotalTime.Elapsed} the metrics have been recorded {RecordCount} times and TelemetryClient flushed {FlushCount} times.");
        }

        private static void Init()
        {
            var cfg = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var logFile = ResolveLogFilePath(cfg);
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Async(x => x.File(logFile))
                .CreateLogger();
            Logger = Log.Logger.ForContext(typeof(Program));

            var metricsOptions = cfg.GetSection("metrics").Get<MetricsOptions>();
            var instrumentationKey = ResolveInstrumentationKey(cfg);

            metrics = new MetricsBuilder()
                .Configuration.Configure(metricsOptions)
                .Report.ToApplicationInsights(instrumentationKey)
                .Build();

            reporter = metrics.ReportRunner;
        }

        private static string ResolveLogFilePath(IConfiguration cfg)
        {
            var logFile = cfg.GetValue("logFile", @"%DOTNET_WORKSPACE%\ApplicationInsightsSandbox.log");
            logFile = Environment.ExpandEnvironmentVariables(logFile);
            return logFile.Contains("%")
                ? Path.GetTempFileName()
                : logFile;
        }

        private static string ResolveInstrumentationKey(IConfiguration cfg)
        {
            var instrumentationKey = cfg.GetValue<string>("metrics:instrumentationKey");
            if (!string.IsNullOrEmpty(instrumentationKey))
            {
                var g = Guid.Parse(instrumentationKey);
                if (g != Guid.Empty)
                {
                    return instrumentationKey;
                }
            }

            throw new Exception("You must set non-empty Application Insights instrumentation key in the appsettings.json config as metrics:instrumentationKey.");
        }

        private static void PrintHelp(TimeSpan recordEvery, TimeSpan reportEvery)
        {
            const string sep = "--------------------------------------------------------------------------------";
            Console.WriteLine(sep);
            Console.WriteLine("Use following keys to:");
            Console.WriteLine("P     -> print current metrics to console");
            Console.WriteLine("R     -> immediately report current metrics");
            Console.WriteLine("<Esc> -> exit");
            Console.WriteLine(sep);
            Console.WriteLine($"New metrics are being created every {recordEvery.TotalSeconds}s and reported every {reportEvery.TotalSeconds}s.");
            Console.WriteLine(sep);
        }

        private static async Task Record(TimeSpan every, CancellationToken ct)
        {
            var sw = new Stopwatch();
            while (!ct.IsCancellationRequested)
            {
                sw.Restart();

                metrics.Measure.Counter.Increment(CounterOne);
                metrics.Measure.Counter.Increment(CounterTwo, Rnd.Value.Next(1, 4));
                metrics.Measure.Gauge.SetValue(GaugeOne, Rnd.Value.Next(0, 201));
                metrics.Measure.Histogram.Update(HistogramOne, Rnd.Value.Next(0, 201));

                var dimension1 = Rnd.Value.Next(0, 2) == 0 ? "failures" : "errors";
                metrics.Measure.Meter.Mark(MeterOne, Rnd.Value.Next(0, 6), dimension1);

                try
                {
                    using (metrics.Measure.Timer.Time(TimerOne))
                    {
                        await Task.Delay(Rnd.Value.Next(0, 101), ct).ConfigureAwait(false);
                    }

                    using (metrics.Measure.Apdex.Track(ApdexOne))
                    {
                        await Task.Delay(Rnd.Value.Next(0, 1001), ct).ConfigureAwait(false);
                    }

                    Console.Write('.');
                    Interlocked.Increment(ref RecordCount);

                    sw.Stop();
                    var remaining = every - sw.Elapsed;
                    if (remaining < TimeSpan.Zero)
                    {
                        Logger.Warning(
                            "It is impossible to honour repetition every {0}s because the body of the Record method took {1}.",
                            every.TotalSeconds,
                            sw.Elapsed);
                        continue;
                    }

                    await Task.Delay(remaining, ct).ConfigureAwait(false);
                }
                catch (TaskCanceledException ex) when (ex.CancellationToken == ct)
                {
                    Logger.Verbose("Record task cancelled.");
                }
            }
        }

        private static async Task Report(TimeSpan every, CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(every, ct).ConfigureAwait(false);
                    await Task.WhenAll(reporter.RunAllAsync(ct)).ConfigureAwait(false);
                    Interlocked.Increment(ref FlushCount);
                    Console.Write('*');
                }
                catch (TaskCanceledException ex) when (ex.CancellationToken == ct)
                {
                    Logger.Verbose("Report task cancelled.");
                }
            }
        }

        private static void PrintMetricsToConsole()
        {
            var metricsData = metrics.Snapshot.Get();

            foreach (var fmt in metrics.OutputMetricsFormatters)
            {
                using (var ms = new MemoryStream())
                {
                    fmt.WriteAsync(ms, metricsData).GetAwaiter().GetResult();
                    var txt = Encoding.UTF8.GetString(ms.ToArray());
                    Console.WriteLine(txt);
                }
            }
        }

        private static void ReportOnDemand()
            => Task.WaitAll(reporter.RunAllAsync().ToArray());
    }
}
