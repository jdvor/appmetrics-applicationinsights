namespace App.Metrics.Reporting.ApplicationInsights
{
    public static class Constants
    {
        public const string UnitKey = "unit";
        public const string DefaultDimensionKey = "item";
        public const string AppMetricsTypeKey = "appMetricsType";

        public const string CounterTypeValue = "counter";
        public const string MeterTypeValue = "meter";
        public const string ApdexTypeValue = "apdex";
        public const string GaugeTypeValue = "gauge";
        public const string HistogramTypeValue = "histogram";
        public const string TimerMeterTypeValue = "timer.meter";
        public const string TimerHistogramTypeValue = "timer.histogram";
    }
}
