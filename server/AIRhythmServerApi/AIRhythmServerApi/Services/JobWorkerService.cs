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

        public JobWorkerService(
            IJobQueue queue,
            IJobStore jobs,
            IChartStore charts,
            IOptions<StorageOptions> storage)
        {
            _queue = queue;
            _jobs = jobs;
            _charts = charts;
            _storage = storage.Value;
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

                    // MVP: 2~3초 “가짜 생성”
                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                    job.Progress = 0.4f;
                    _jobs.Update(job);

                    var chartId = $"chart_{Guid.NewGuid():N}";
                    var root = Path.GetFullPath(_storage.RootPath);
                    var audioPath = Path.Combine(root, _storage.AudioDir, $"{chartId}.wav");
                    var chartPath = Path.Combine(root, _storage.ChartsDir, $"{chartId}.json");

                    Directory.CreateDirectory(Path.GetDirectoryName(audioPath)!);

                    // 샘플 wav를 결과 wav로 복사 (MVP 통합 테스트용)
                    var samplePath = Path.Combine(AppContext.BaseDirectory, "Samples", "sample.wav");

                    if (!File.Exists(samplePath))
                        throw new FileNotFoundException($"Sample WAV not found: {samplePath}");

                    File.Copy(samplePath, audioPath, overwrite: true);

                    job.Progress = 0.8f;
                    _jobs.Update(job);

                    // JSON 생성: audio.file은 Unity 매칭 규칙대로 "Audio/{chartId}.wav" 고정
                    var chartJson = new
                    {
                        chart_version = "1.0",
                        song_id = chartId,
                        audio = new
                        {
                            file = $"Audio/{chartId}.wav",
                            audio_offset_ms = 0,
                            duration_ms = job.DurationSec * 1000
                        },
                        difficulty = "dev",
                        notes = new[] { new { t_ms = 0 }, new { t_ms = 500 }, new { t_ms = 1000 } }
                    };

                    Directory.CreateDirectory(Path.GetDirectoryName(chartPath)!);
                    var jsonText = JsonSerializer.Serialize(chartJson, new JsonSerializerOptions { WriteIndented = true });
                    await File.WriteAllTextAsync(chartPath, jsonText, stoppingToken);

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
                }
                catch (Exception ex)
                {
                    job.Status = JobStatus.Error;
                    job.Progress = 0f;
                    job.ErrorMessage = ex.Message;
                    _jobs.Update(job);
                }
            }
        }
    }
}
