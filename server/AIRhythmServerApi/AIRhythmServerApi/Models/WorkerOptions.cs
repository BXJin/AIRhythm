namespace AIRhythmServerApi.Models;

public sealed class WorkerOptions
{
    public string PythonPath { get; set; } = "python";
    public string WorkerScriptPath { get; set; } = "";
    public int IntervalMs { get; set; } = 500;
    public int LeadInMs { get; set; } = 500;
}
