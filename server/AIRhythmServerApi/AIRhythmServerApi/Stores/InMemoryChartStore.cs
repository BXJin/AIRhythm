using AIRhythmServerApi.Models;
using System.Collections.Concurrent;

namespace AIRhythmServerApi.Stores
{
    public sealed class InMemoryChartStore : IChartStore
    {
        private readonly ConcurrentDictionary<string, ChartRecord> _charts = new();

        public void Add(ChartRecord chart) => _charts[chart.ChartId] = chart;

        public bool TryGet(string chartId, out ChartRecord chart)
            => _charts.TryGetValue(chartId, out chart!);
    }
}
