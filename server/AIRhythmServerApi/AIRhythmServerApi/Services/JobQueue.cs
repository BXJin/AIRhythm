using System.Threading.Channels;

namespace AIRhythmServerApi.Services
{
    public interface IJobQueue
    {
        ValueTask EnqueueAsync(string jobId, CancellationToken ct);
        ValueTask<string> DequeueAsync(CancellationToken ct);
    }

    public sealed class InMemoryJobQueue : IJobQueue
    {
        private readonly Channel<string> _channel = Channel.CreateUnbounded<string>(
            new UnboundedChannelOptions { SingleReader = true, SingleWriter = false });

        public ValueTask EnqueueAsync(string jobId, CancellationToken ct)
            => _channel.Writer.WriteAsync(jobId, ct);

        public ValueTask<string> DequeueAsync(CancellationToken ct)
            => _channel.Reader.ReadAsync(ct);
    }
}
