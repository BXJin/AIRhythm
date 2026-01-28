using AIRhythmServerApi.Models;

namespace AIRhythmServerApi.Stores
{
    public interface IJobStore
    {
        void Add(JobRecord job);
        bool TryGet(string jobId, out JobRecord job);
        void Update(JobRecord job);
    }
}
