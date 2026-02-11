using AIRhythmServerApi.Models;
using AIRhythmServerApi.Stores;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace AIRhythmServerApi.Services
{
    public sealed class JobWorkerService : BackgroundService
    {
        private readonly IJobQueue _queue;
        private readonly IJobStore _jobs;
        private readonly IChartStore _charts;
        private readonly StorageOptions _storage;
        private readonly WorkerOptions _worker;
        private readonly WorkerRunner _workerRunner;
        private readonly ILogger<JobWorkerService> _logger;

        public JobWorkerService(
            IJobQueue queue,
            IJobStore jobs,
            IChartStore charts,
            IOptions<StorageOptions> storage,
            IOptions<WorkerOptions> worker,
            WorkerRunner workerRunner,
            ILogger<JobWorkerService> logger)
        {
            _queue = queue;
            _jobs = jobs;
            _charts = charts;
            _storage = storage.Value;
            _worker = worker.Value;
            _workerRunner = workerRunner;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var jobId = await _queue.DequeueAsync(stoppingToken);

                if (!_jobs.TryGet(jobId, out var job))
                    continue;

                try
                {
                    job.Status = JobStatus.Running;
                    job.Progress = 0.1f;
                    _jobs.Update(job);

                    var chartId = $"chart_{Guid.NewGuid():N}";
                    var root = Path.GetFullPath(_storage.RootPath);
                    var audioPath = Path.Combine(root, _storage.AudioDir, $"{chartId}.wav");
                    var chartPath = Path.Combine(root, _storage.ChartsDir, $"{chartId}.json");
                    var notesPath = Path.Combine(root, _storage.ChartsDir, $"{chartId}_notes.json");

                    // 디렉토리 생성
                    Directory.CreateDirectory(Path.GetDirectoryName(audioPath)!);
                    Directory.CreateDirectory(Path.GetDirectoryName(chartPath)!);

                    // MVP: 샘플 wav 복사 (나중에 Lyria2로 교체)
                    var samplePath = Path.Combine(AppContext.BaseDirectory, "Samples", "sample.wav");
                    if (!File.Exists(samplePath))
                        throw new FileNotFoundException($"Sample WAV not found: {samplePath}");

                    File.Copy(samplePath, audioPath, overwrite: true);

                    job.Progress = 0.3f;
                    _jobs.Update(job);

                    // WAV duration 계산 (서버에서)
                    _logger.LogInformation("Reading WAV duration for {ChartId}", chartId);
                    var durationMs = WavDurationReader.GetDurationMs(audioPath);
                    _logger.LogInformation("WAV duration: {DurationMs}ms", durationMs);

                    job.Progress = 0.4f;
                    _jobs.Update(job);

                    // Python Worker 실행 (notes만 생성)
                    var workerScript = Path.GetFullPath(
                        Path.Combine(AppContext.BaseDirectory, _worker.WorkerScriptPath));

                    _logger.LogInformation("Running worker for job {JobId}, chartId {ChartId}", jobId, chartId);

                    var (exitCode, stdout, stderr) = await _workerRunner.RunAsync(
                        pythonExe: _worker.PythonPath,
                        workerPyPath: workerScript,
                        wavPath: audioPath,
                        outJsonPath: notesPath,
                        durationMs: durationMs,  // 서버가 계산한 duration 전달
                        intervalMs: _worker.IntervalMs,
                        leadInMs: _worker.LeadInMs,
                        ct: stoppingToken
                    );

                    job.Progress = 0.7f;
                    _jobs.Update(job);

                    if (exitCode != 0)
                    {
                        _logger.LogError("Worker failed with exit code {ExitCode}. Stderr: {Stderr}",
                            exitCode, stderr);
                        throw new InvalidOperationException(
                            $"Worker failed (exit {exitCode}): {stderr.Trim()}");
                    }

                    if (!string.IsNullOrWhiteSpace(stdout))
                        _logger.LogInformation("Worker stdout: {StdOut}", stdout.Trim());

                    // Notes 파일 읽기
                    if (!File.Exists(notesPath))
                        throw new FileNotFoundException($"Worker did not produce notes file: {notesPath}");

                    var notesJson = await File.ReadAllTextAsync(notesPath, stoppingToken);
                    var notesPayload = JsonSerializer.Deserialize<JsonElement>(notesJson);
                    
                    // {"notes": [...]} 구조에서 notes 배열 추출
                    if (!notesPayload.TryGetProperty("notes", out var notesArray))
                        throw new InvalidDataException("Worker output missing 'notes' property.");

                    // Chart JSON 생성 (서버에서)
                    var chartData = new
                    {
                        chart_version = "1.0",
                        song_id = chartId,
                        audio = new
                        {
                            file = $"Audio/{chartId}.wav",
                            audio_offset_ms = 0,
                            duration_ms = durationMs
                        },
                        difficulty = "dev",
                        notes = notesArray
                    };

                    var chartJsonText = JsonSerializer.Serialize(chartData, new JsonSerializerOptions { WriteIndented = true });
                    await File.WriteAllTextAsync(chartPath, chartJsonText, stoppingToken);

                    // notes 임시 파일 삭제
                    File.Delete(notesPath);

                    job.Progress = 0.9f;
                    _jobs.Update(job);

                    // Chart 등록
                    var chart = new ChartRecord
                    {
                        ChartId = chartId,
                        Title = job.Prompt.Length > 20 ? job.Prompt[..20] : job.Prompt,
                        ChartPath = chartPath,
                        AudioPath = audioPath,
                        AudioFileName = $"{chartId}.wav"
                    };
                    _charts.Add(chart);

                    job.Status = JobStatus.Done;
                    job.Progress = 1.0f;
                    job.ChartId = chartId;
                    job.ErrorMessage = null;
                    _jobs.Update(job);

                    _logger.LogInformation("Job {JobId} completed successfully. ChartId: {ChartId}",
                        jobId, chartId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Job {JobId} failed", jobId);
                    job.Status = JobStatus.Error;
                    job.Progress = 0f;
                    job.ErrorMessage = ex.Message;
                    _jobs.Update(job);
                }
            }
        }
    }
}
