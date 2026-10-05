using System;
using System.Collections.Generic;

namespace Istatistik.Models
{
    /// <summary>
    /// Zaman aralığı filtreleme türü
    /// </summary>
    public enum PeriodType
    {
        Daily,      // Günlük
        Weekly,     // Haftalık
        Monthly,    // Aylık
        Yearly      // Yıllık
    }

    /// <summary>
    /// Trend yönü (karşılaştırma için)
    /// </summary>
    public enum TrendDirection
    {
        Up,         // Artış
        Down,       // Azalış
        Neutral     // Değişim yok
    }

    /// <summary>
    /// Toplanmış veri noktası
    /// </summary>
    public class AggregatedDataPoint
    {
        public DateTime Date { get; set; }
        public decimal Value { get; set; }
        public string Label { get; set; }
        public Dictionary<string, decimal> Metrics { get; set; }

        public AggregatedDataPoint()
        {
            Metrics = new Dictionary<string, decimal>();
        }
    }

    /// <summary>
    /// Toplanmış veri sonucu
    /// </summary>
    public class AggregatedDataResult
    {
        public string DataType { get; set; }
        public PeriodType PeriodType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<AggregatedDataPoint> DataPoints { get; set; }
        public Dictionary<string, decimal> Summary { get; set; }

        public AggregatedDataResult()
        {
            DataPoints = new List<AggregatedDataPoint>();
            Summary = new Dictionary<string, decimal>();
        }
    }

    /// <summary>
    /// Karşılaştırma metriği
    /// </summary>
    public class ComparisonMetric
    {
        public string MetricName { get; set; }
        public decimal Period1Value { get; set; }
        public decimal Period2Value { get; set; }
        public decimal AbsoluteDifference { get; set; }
        public decimal? PercentageChange { get; set; }
        public TrendDirection Trend { get; set; }
    }

    /// <summary>
    /// Dönem karşılaştırma sonucu
    /// </summary>
    public class ComparisonResult
    {
        public string DataType { get; set; }
        public PeriodType PeriodType { get; set; }

        public DateTime Period1Start { get; set; }
        public DateTime Period1End { get; set; }
        public List<AggregatedDataPoint> Period1Data { get; set; }
        public Dictionary<string, decimal> Period1Summary { get; set; }

        public DateTime Period2Start { get; set; }
        public DateTime Period2End { get; set; }
        public List<AggregatedDataPoint> Period2Data { get; set; }
        public Dictionary<string, decimal> Period2Summary { get; set; }

        public List<ComparisonMetric> Differences { get; set; }

        public ComparisonResult()
        {
            Period1Data = new List<AggregatedDataPoint>();
            Period1Summary = new Dictionary<string, decimal>();
            Period2Data = new List<AggregatedDataPoint>();
            Period2Summary = new Dictionary<string, decimal>();
            Differences = new List<ComparisonMetric>();
        }
    }

    /// <summary>
    /// Sayfalanmış tablo sonucu
    /// </summary>
    public class PagedTableResult
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
        public List<Dictionary<string, object>> Rows { get; set; }
        public Dictionary<string, decimal> Summary { get; set; }

        public PagedTableResult()
        {
            Rows = new List<Dictionary<string, object>>();
            Summary = new Dictionary<string, decimal>();
        }
    }
}
