using AIRhythmServerApi.Models;
using AIRhythmServerApi.Services;
using AIRhythmServerApi.Stores;
using Microsoft.AspNetCore.Mvc;

namespace AIRhythmServerApi.Controllers
{
    [ApiController]
    [Route("api/generate")]
    public sealed class GenerateController : ControllerBase
    {
        private readonly IJobStore _jobs;
        private readonly IJobQueue _queue;

        public GenerateController(IJobStore jobs, IJobQueue queue)
        {
            _jobs = jobs;
            _queue = queue;
        }

        [HttpPost]
        public async Task<IActionResult> Start([FromBody] GenerateRequest req, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(req.Prompt))
                return BadRequest(new { message = "prompt is required" });

            var jobId = $"job_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}";

            var job = new JobRecord
            {
                JobId = jobId,
                Status = JobStatus.Queued,
                Progress = 0f,
                Prompt = req.Prompt,
                Genre = req.Genre ?? "",
                DurationSec = req.DurationSec <= 0 ? 15 : req.DurationSec
            };

            _jobs.Add(job);
            await _queue.EnqueueAsync(jobId, ct);

            return Ok(new { jobId });
        }

        [HttpGet("{jobId}")]
        public IActionResult GetStatus([FromRoute] string jobId)
        {
            if (!_jobs.TryGet(jobId, out var job))
                return NotFound(new { message = "job not found" });

            // 스펙에 맞춰 응답 포맷 구성
            var statusText = job.Status switch
            {
                JobStatus.Queued => "queued",
                JobStatus.Running => "running",
                JobStatus.Done => "done",
                JobStatus.Error => "error",
                _ => "error"
            };

            return Ok(new
            {
                status = statusText,
                progress = job.Progress,
                chartId = job.ChartId,
                message = job.ErrorMessage
            });
        }
    }
}
