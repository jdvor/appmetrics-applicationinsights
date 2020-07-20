namespace App.Metrics.Reporting.ApplicationInsights
{
    using App.Metrics.Apdex;
    using App.Metrics.Counter;
    using App.Metrics.Gauge;
    using App.Metrics.Histogram;
    using App.Metrics.Meter;
    using App.Metrics.Timer;
    using Microsoft.ApplicationInsights.DataContracts;
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    public class ItemsAsCustomPropertyMetricsTranslator : IMetricsTranslator
    {
        public const string DimensionNameKey = "DimensionName";
        private readonly string defaultDimensionName;

        public ItemsAsCustomPropertyMetricsTranslator(string defaultDimensionName)
        {
            this.defaultDimensionName = !string.IsNullOrEmpty(defaultDimensionName)
                ? defaultDimensionName
                : throw new ArgumentException("dimension name must not be null or empty", nameof(defaultDimensionName));
        }

        public IEnumerable<MetricTelemetry> Translate(MetricsContextValueSource context, DateTimeOffset now)
        {
            var contextName = context != null
                ? context.Context
                : throw new ArgumentNullException(nameof(context));

            foreach (var source in context.ApdexScores)
            {
                yield return TranslateApdexSource(source, contextName, now);
            }

            foreach (var source in context.Counters)
            {
                foreach (var mt in TranslateCounterSource(source, contextName, now))
                {
                    yield return mt;
                }
            }

            foreach (var source in context.Gauges)
            {
                yield return TranslateGaugeSource(source, contextName, now);
            }

            foreach (var source in context.Histograms)
            {
                yield return TranslateHistogramSource(source, contextName, now);
            }

            foreach (var source in context.Meters)
            {
                foreach (var mt in TranslateMeterSource(source, contextName, now))
                {
                    yield return mt;
                }
            }

            foreach (var source in context.Timers)
            {
                foreach (var mt in TranslateTimerSource(source, contextName, now))
                {
                    yield return mt;
                }
            }
        }

        private static MetricTelemetry TranslateApdexSource(ApdexValueSource source, string contextName, DateTimeOffset now)
        {
            var mt = MetricFactory.CreateMetric(source, contextName, now);
            TranslateTags(source.Tags, mt);
            mt.Properties[Constants.AppMetricsTypeKey] = Constants.ApdexTypeValue;
            var all = source.Value.Satisfied + source.Value.Tolerating + source.Value.Frustrating;
            mt.Sum = (source.Value.Satisfied + ((double)source.Value.Tolerating / 2)) / all;
            return mt;
        }

        private IEnumerable<MetricTelemetry> TranslateCounterSource(CounterValueSource source, string contextName, DateTimeOffset now)
        {
            var counter = source.ValueProvider.GetValue(source.ResetOnReporting); // https://github.com/jdvor/appmetrics-applicationinsights/issues/2
            if (source.ReportSetItems && counter.Items.Length > 0)
            {
                var dimensionName = GetDimensionName(source.Tags);
                foreach (var item in counter.Items)
                {
                    var mt = MetricFactory.CreateMetric(source, contextName, now);
                    TranslateTags(source.Tags, mt);
                    TranslateTags(item.Tags, mt);
                    mt.Properties[Constants.AppMetricsTypeKey] = Constants.CounterTypeValue;
                    mt.Properties[dimensionName] = item.Item;
                    mt.Sum = item.Count;
                    yield return mt;
                }
            }
            else
            {
                var mt = MetricFactory.CreateMetric(source, contextName, now);
                TranslateTags(source.Tags, mt);
                mt.Properties[Constants.AppMetricsTypeKey] = Constants.CounterTypeValue;
                mt.Sum = counter.Count;
                yield return mt;
            }
        }

        private static MetricTelemetry TranslateGaugeSource(GaugeValueSource source, string contextName, DateTimeOffset now)
        {
            var mt = MetricFactory.CreateMetric(source, contextName, now);
            TranslateTags(source.Tags, mt);
            mt.Properties[Constants.AppMetricsTypeKey] = Constants.GaugeTypeValue;
            mt.Sum = source.Value;
            return mt;
        }

        private static MetricTelemetry TranslateHistogramSource(HistogramValueSource source, string contextName, DateTimeOffset now)
        {
            var mt = MetricFactory.CreateMetric(source, contextName, now);
            TranslateTags(source.Tags, mt);
            TranslateHistogram(source.Value, mt, Constants.HistogramTypeValue);
            return mt;
        }

        private static void TranslateHistogram(HistogramValue value, MetricTelemetry mt, string type)
        {
            mt.Sum = value.Sum;
            mt.Count = Convert.ToInt32(value.Count);
            mt.Min = value.Min;
            mt.Max = value.Max;
            mt.StandardDeviation = value.StdDev;
            mt.Properties[Constants.AppMetricsTypeKey] = type;
        }

        private IEnumerable<MetricTelemetry> TranslateMeterSource(MeterValueSource source, string contextName, DateTimeOffset now)
        {
            var unit = source.Value.RateUnit.ToShortString();
            if (source.ReportSetItems && source.Value.Items.Length > 0)
            {
                var dimensionName = GetDimensionName(source.Tags);
                foreach (var item in source.Value.Items)
                {
                    var mt = MetricFactory.CreateMetric(source, contextName, now);
                    TranslateTags(source.Tags, mt);
                    TranslateTags(item.Tags, mt);
                    TranslateMeterItem(item, mt, unit, dimensionName, Constants.MeterTypeValue);
                    yield return mt;
                }
            }
            else
            {
                var mt = MetricFactory.CreateMetric(source, contextName, now);
                TranslateTags(source.Tags, mt);
                TranslateMeterValue(source.Value, mt, unit, Constants.MeterTypeValue);
                yield return mt;
            }
        }

        private static void TranslateMeterItem(MeterValue.SetItem item, MetricTelemetry mt, string unit, string dimensionName, string type)
        {
            mt.Properties[dimensionName] = item.Item;
            mt.Properties[Constants.UnitKey] = unit;
            mt.Properties[Constants.AppMetricsTypeKey] = type;
            mt.Sum = item.Value.MeanRate;
        }

        private static void TranslateMeterValue(MeterValue value, MetricTelemetry mt, string unit, string type)
        {
            mt.Sum = value.MeanRate;
            mt.Properties[Constants.UnitKey] = unit;
            mt.Properties[Constants.AppMetricsTypeKey] = type;
        }

        private IEnumerable<MetricTelemetry> TranslateTimerSource(TimerValueSource source, string contextName, DateTimeOffset now)
        {
            var mt = MetricFactory.CreateMetric(source, contextName, now);
            TranslateTags(source.Tags, mt);
            TranslateHistogram(source.Value.Histogram, mt, Constants.TimerHistogramTypeValue);
            mt.Properties[Constants.UnitKey] = source.DurationUnit.ToShortString();
            yield return mt;

            var unit = source.Value.Rate.RateUnit.ToShortString();
            if (source.Value.Rate.Items.Length > 0)
            {
                var dimensionName = GetDimensionName(source.Tags);
                foreach (var item in source.Value.Rate.Items)
                {
                    mt = MetricFactory.CreateMetric(source, contextName, now, "rate");
                    TranslateTags(source.Tags, mt);
                    TranslateTags(item.Tags, mt);
                    TranslateMeterItem(item, mt, unit, dimensionName, Constants.TimerMeterTypeValue);
                    yield return mt;
                }
            }
            else
            {
                mt = MetricFactory.CreateMetric(source, contextName, now, "rate");
                TranslateTags(source.Tags, mt);
                TranslateMeterValue(source.Value.Rate, mt, unit, Constants.TimerMeterTypeValue);
                yield return mt;
            }
        }

        private static void TranslateTags(MetricTags tags, MetricTelemetry mt)
        {
            for (int i = 0; i < tags.Count; i++)
            {
                var key = tags.Keys[i];
                if (string.Compare(key, DimensionNameKey, true, CultureInfo.InvariantCulture) == 0)
                {
                    continue;
                }

                mt.Properties[key] = tags.Values[i];
            }
        }

        private string GetDimensionName(MetricTags tags)
        {
            for (int i = 0; i < tags.Count; i++)
            {
                if (string.Compare(tags.Keys[i], DimensionNameKey, true, CultureInfo.InvariantCulture) == 0)
                {
                    return tags.Values[i];
                }
            }

            return defaultDimensionName;
        }
    }
}
