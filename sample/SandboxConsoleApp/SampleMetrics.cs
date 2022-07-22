namespace SandboxConsoleApp;

using App.Metrics;
using App.Metrics.Apdex;
using App.Metrics.Counter;
using App.Metrics.Gauge;
using App.Metrics.Histogram;
using App.Metrics.Meter;
using App.Metrics.Timer;

public static class SampleMetrics
{
    public static ApdexOptions ApdexOne => new ApdexOptions
    {
        Name = "apdex_one",
        ApdexTSeconds = 0.15,
    };

    public static CounterOptions CounterOne => new CounterOptions
    {
        Name = "counter_one",
        ResetOnReporting = true,
    };

    public static CounterOptions CounterTwo => new CounterOptions
    {
        Name = "counter_two",
    };

    public static GaugeOptions GaugeOne => new GaugeOptions
    {
        Name = "gauge_one",
        Tags = new MetricTags(new[] { "prop1", "prop2" }, new[] { "alpha", "beta" }),
    };

    public static HistogramOptions HistogramOne => new HistogramOptions
    {
        Name = "histogram_one",
    };

    public static MeterOptions MeterOne => new MeterOptions
    {
        Name = "meter_one",
        Tags = new MetricTags(new[] { "prop3", "prop4", "DimensionName" }, new[] { "gama", "delta", "bucket" }),
    };

    public static TimerOptions TimerOne => new TimerOptions
    {
        Name = "timer_one"
    };
}
