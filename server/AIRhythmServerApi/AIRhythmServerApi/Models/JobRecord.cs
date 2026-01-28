namespace AIRhythmServerApi.Models
{
    public enum JobStatus
    {
        Queued,
        Running,
        Done,
        Error
    }
    public sealed class JobRecord
    {
        public string JobId { get; init; } = "";
        public JobStatus Status { get; set; } = JobStatus.Queued;
        public float Progress { get; set; } = 0f;

        public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;

        public string Prompt { get; init; } = "";
        public string Genre { get; init; } = "";
        public int DurationSec { get; init; }

        public string? ChartId { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
