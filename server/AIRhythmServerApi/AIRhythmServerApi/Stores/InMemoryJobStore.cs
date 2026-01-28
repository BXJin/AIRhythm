using AIRhythmServerApi.Models;
using System.Collections.Concurrent;

namespace AIRhythmServerApi.Stores
{
    public class InMemoryJobStore : IJobStore
    {
        private readonly ConcurrentDictionary<string, JobRecord> _jobs = new();

        public void Add(JobRecord job) => _jobs[job.JobId] = job;

        public bool TryGet(string jobId, out JobRecord job)
            => _jobs.TryGetValue(jobId, out job!);

        public void Update(JobRecord job) => _jobs[job.JobId] = job;
    }
}
