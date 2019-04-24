# Application Insights reporter for App.Metrics

## Usage
1. Install nuget package: [App.Metrics.Reporting.ApplicationInsights](https://nuget.com)
2. Obtain Application Insights [instrumentation key](https://docs.microsoft.com/en-us/azure/azure-monitor/app/create-new-resource).
3. Configure App.Metrics like so:
```
var instrumentationKey = "00000000-0000-0000-0000-000000000000";

var metrics = new MetricsBuilder()
    .Configuration.Configure(metricsOptions)
    .Report.ToApplicationInsights(instrumentationKey)
    .Build();
```

## How it works
App.Metrics pre-aggregates metrics and reporters are responsible for publishing such aggregated data.
Application Insights's type `MetricTelemetry` is used to describe pre-aggregated metrics
and method `Track(ITelemetry telemetry)` of `TelemetryClient` publishes it.

It just boils down to translating `MetricsDataValueSource` into `IEnumerable<MetricTelemetry>` and publishing the collection using `TelemetryClient`.

## Links
* [App.Metrics documentation](https://www.app-metrics.io/)
* [Application Insights API for custom events and metrics](https://docs.microsoft.com/en-us/azure/azure-monitor/app/api-custom-events-metrics)

## Known issues / shortcomings

#### 1. Aggregation scope
With regards to pre-aggregated data statistics (min, max, stddev) there are two types of App.Metric value sources:

1. Configurable cumulative - (counter), which are indefinately cummulative by default, but can be configured in their respective options using property `ResetOnReporting` to start new aggregation scope when the last one is reported.
2. Indefinately cumulative - (meter, histogram, apdex, timer), which means that they will start new aggregation scope only when IMetricsRoot is created (in practice this usually means when application restarts).

AI clearly favors the first approach.<br/>
It is what TelemetryClient does when non-pre-aggregation API (`.GetMetric("mycounter").TrackValue(2)`) is used and therefore aggregation strategy is under its control.
I guess mostly because data statistics _min, max, stddev_ are really only useful for describing smaller batches of uploaded data and not the whole "ever recorded" scope.

Bottom line is: when using App.Metric as a facade to Application Insights do not rely on metric properties _min, max, stddev_ as they will contain something else than you would expect (compared to using TelemetryClient alone for example).

#### 2. Metric dimensions in Azure Portal / Applications Insights
"Dimension" is primarily a term used in Application Insights; the App.Metrics synonym is "item" as in `Meter.Mark(MeterOptions options, long amount, string item)`.

`MetricTelemetry` does not have a way how to explicitly report dimensions, so this reporter reports them as new metric records with names derived for the parent one.

For example: if on the App.Metric side you would have a single metric _"fruit_count"_ with two dimensions _"apples"_ and _"pears"_
than three metrics will be reported to Application Insights: _"fruit_count", "fruit_count.apples"_ and _"fruit_count.pears"_.