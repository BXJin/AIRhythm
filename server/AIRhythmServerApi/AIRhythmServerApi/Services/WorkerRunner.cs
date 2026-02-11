using System.Diagnostics;
using System.Text;

namespace AIRhythmServerApi.Services
{
    public sealed class WorkerRunner
    {
        private const int DefaultTimeoutSeconds = 60;

        public async Task<(int ExitCode, string StdOut, string StdErr)> RunAsync(
            string pythonExe,
            string workerPyPath,
            string wavPath,
            string outJsonPath,
            int durationMs,
            int intervalMs,
            int leadInMs,  // ← 추가
            CancellationToken ct)
        {
            if (!File.Exists(workerPyPath))
                throw new FileNotFoundException("worker.py not found", workerPyPath);

            if (!File.Exists(wavPath))
                throw new FileNotFoundException("input wav not found", wavPath);

            var args = BuildArgs(workerPyPath, wavPath, outJsonPath, durationMs, intervalMs, leadInMs);

            var psi = new ProcessStartInfo
            {
                FileName = pythonExe,
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetDirectoryName(workerPyPath)!
            };

            using var p = new Process { StartInfo = psi };

            var stdout = new StringBuilder();
            var stderr = new StringBuilder();

            p.OutputDataReceived += (_, e) => { if (e.Data is not null) stdout.AppendLine(e.Data); };
            p.ErrorDataReceived += (_, e) => { if (e.Data is not null) stderr.AppendLine(e.Data); };

            if (!p.Start())
                throw new InvalidOperationException("Failed to start python process.");

            p.BeginOutputReadLine();
            p.BeginErrorReadLine();

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(DefaultTimeoutSeconds));

            try
            {
                await p.WaitForExitAsync(timeoutCts.Token);
            }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested)
            {
                try { if (!p.HasExited) p.Kill(entireProcessTree: true); } catch { /* ignore */ }
                throw new TimeoutException($"Worker timed out after {DefaultTimeoutSeconds}s.");
            }

            return (p.ExitCode, stdout.ToString(), stderr.ToString());
        }

        private static string BuildArgs(
            string workerPyPath,
            string wavPath,
            string outJsonPath,
            int durationMs,
            int intervalMs,
            int leadInMs)  // ← 추가
        {
            string Q(string s) => $"\"{s}\"";

            return string.Join(" ",
                Q(workerPyPath),
                "--in", Q(wavPath),
                "--out", Q(outJsonPath),
                "--durationMs", durationMs.ToString(),
                "--intervalMs", intervalMs.ToString(),
                "--leadInMs", leadInMs.ToString()  // ← 추가
            );
        }
    }
}
