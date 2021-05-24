# Application Insights reporter for App.Metrics

[changelog](changelog.md)

## Usage
1. Install nuget package: [App.Metrics.Reporting.ApplicationInsights](https://www.nuget.org/packages/App.Metrics.Reporting.ApplicationInsights/)
2. Obtain Application Insights [instrumentation key](https://docs.microsoft.com/en-us/azure/azure-monitor/app/create-new-resource).
3. Configure App.Metrics like so:
```
var instrumentationKey = "00000000-0000-0000-0000-000000000000";

var metrics = new MetricsBuilder()
    .Configuration.Configure(metricsOptions)
    .Report.ToApplicationInsights(instrumentationKey)
    .Build();
```

There are two ways how to deal with AppMetrics' items which would be referred to as _dimension 1_ in Application Insights.
1. Report them as part of the metric name. For example: if on the App.Metric side you would have a single metric _"fruit_count"_ with two dimensions _"apples"_ and _"pears"_
than three metrics will be reported to Application Insights: _"fruit_count", "fruit_count.apples"_ and _"fruit_count.pears"_. This is default.
2. Report them under single name and distinguish dimension by using _customDimensions_; see section "AppMetrics item as Application Insights customDimension".

## How it works
App.Metrics pre-aggregates metrics and reporters are responsible for publishing such aggregated data.
Application Insights's type `MetricTelemetry` is used to describe pre-aggregated metrics
and method `Track(ITelemetry telemetry)` of `TelemetryClient` publishes it.

It just boils down to translating `MetricsDataValueSource` into `IEnumerable<MetricTelemetry>` and publishing the collection using `TelemetryClient`.

## AppMetrics item as Application Insights customDimension
(rather than part of the name, which is default behavior)

```
var metrics = new MetricsBuilder()
    .Configuration.Configure(metricsOptions)
    .Report.ToApplicationInsights(opts => {
        opts.InstrumentationKey = "00000000-0000-0000-0000-000000000000";
        opts.ItemsAsCustomDimensions = true;
        opts.DefaultCustomDimensionName = "item";
    })
    .Build();
```

If you would like the dimension to have more meaningful name than the default, you can add MetricTag with a name _DimensionName_ to an AppMetrics metric and it will use it instead of a reporter-level default value.

## Links
* [App.Metrics documentation](https://www.app-metrics.io/)
* [Application Insights API for custom events and metrics](https://docs.microsoft.com/en-us/azure/azure-monitor/app/api-custom-events-metrics)

## Known issues / shortcomings / gotchas

#### 1. Aggregation scope
With regards to pre-aggregated data statistics (min, max, stddev) there are two types of App.Metric value sources:

1. Configurable cumulative - (counter), which are indefinately cummulative by default, but can be configured in their respective options using property `ResetOnReporting` to start new aggregation scope when the last one is reported.
2. Indefinately cumulative - (meter, histogram, apdex, timer), which means that they will start new aggregation scope only when IMetricsRoot is created (in practice this usually means when application restarts).

AI clearly favors the first approach.<br/>
It is what TelemetryClient does when non-pre-aggregation API (`.GetMetric("mycounter").TrackValue(2)`) is used and therefore aggregation strategy is under its control.
I guess mostly because data statistics _min, max, stddev_ are really only useful for describing smaller batches of uploaded data and not the whole "ever recorded" scope.

Bottom line is: when using App.Metric as a facade to Application Insights do not rely on metric properties _min, max, stddev_ as they will contain something else than you would expect (compared to using TelemetryClient alone for example).

#### 2. Reporters need help to actually publish (in some application contexts)
Reporter alone does not actively publishes the metric data. Something must periodically call its `FlushAsync` method.<br/> 
In ASP.NET Core application this is done by [MetricsReporterBackgroundService](https://github.com/AppMetrics/AppMetrics/blob/7f490edb72ac5203ea4b2fa057a187649ae70381/src/Extensions/src/App.Metrics.Extensions.Hosting/MetricsReporterBackgroundService.cs), which is a part of AppMetrics repository and nuget packages.<br/>
See [Bootsrapping Startup.cs](https://www.app-metrics.io/web-monitoring/aspnet-core/reporting/) how it is registered.<br/>
Everywhere else it is up to you to implement this periodic "nudging" of the AppMetrics' reporters.

[Simple example](https://github.com/jdvor/appmetrics-applicationinsights/blob/master/sample/SandboxConsoleApp/Program.cs#L40) in this repository.