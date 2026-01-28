namespace AIRhythmServerApi.Models
{
    public sealed class GenerateRequest
    {
        public string Prompt { get; set; } = "";
        public string Genre { get; set; } = "";
        public int DurationSec { get; set; } = 15;
    }
}
