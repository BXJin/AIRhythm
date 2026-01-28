using AIRhythmServerApi.Models;

namespace AIRhythmServerApi.Stores
{
    public interface IChartStore
    {
        void Add(ChartRecord chart);
        bool TryGet(string chartId, out ChartRecord chart);
    }
}
