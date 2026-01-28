namespace AIRhythmServerApi.Models
{
    public sealed class ChartRecord
    {
        public string ChartId { get; init; } = "";
        public string Title { get; init; } = "";
        public string ChartPath { get; init; } = ""; // storage/charts/{chartId}.json
        public string AudioPath { get; init; } = ""; // storage/audio/{chartId}.wav
        public string AudioFileName { get; init; } = ""; // {chartId}.wav
    }
}
