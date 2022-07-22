# Changelog

## v3.0.0
_0f82c02_ removed configuration option by instrumentation key; now it only can be configured by full connection string [#10](https://github.com/jdvor/appmetrics-applicationinsights/issues/10); updated to ApplicationInsights to 2.21.0; started publishing source as snupkg packages.

## v2.2.0
_e36d902_ updated AppMetrics to 4.3.0 and ApplicationInsights to 2.20.0; changes in code analysis; nullable enabled; updated SanboxConsoleApp to net6.0

## v2.1.0
_8afea1e_ [#7](https://github.com/jdvor/appmetrics-applicationinsights/pull/7) PR Added the ability to configure application insights with a ITelemetryChannel

## v2.0.0
_1fd915b_ added option to report AppMetrics items as customDimension in Application Insights rather than items being part of the metric name (which is default) [#6](https://github.com/jdvor/appmetrics-applicationinsights/issues/6)

## v1.0.3
_df1d213_ updated AppMetrics to 4.1.0 [#5](https://github.com/jdvor/appmetrics-applicationinsights/issues/5)

## v1.0.2
_21fb05f_ target AnyCPU [#3](https://github.com/jdvor/appmetrics-applicationinsights/issues/3)

## v1.0.1
_2a926e1_ removed packageIconUrl (it's now deprecated by nuget)<br />
_2a85f81_ fixed counter not resetting on reporting when required [#2](https://github.com/jdvor/appmetrics-applicationinsights/issues/2)<br />
_b305d5f_ (more) correctly disposing TelemetryConfiguration

## v1.0.0
_f2c7eba_ relaxed semantic version regex; +XML comments<br />
_c8cc61a_ init & WIP
