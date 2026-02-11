using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text;

namespace AIRhythmServerApi.Services
{
    public sealed class LyriaOptions
    {
        public string ProjectId { get; set; } = "";
        public string Location { get; set; } = "us-central1";
        public string PythonExe { get; set; } = "python";
        public string ScriptPath { get; set; } = "Tools/Lyria/lyria_generate.py";
        public int TimeoutSec { get; set; } = 120;
    }

    public sealed class LyriaRunner
    {
        private readonly LyriaOptions _opt;
        private readonly IWebHostEnvironment _env;

        public LyriaRunner(IOptions<LyriaOptions> opt, IWebHostEnvironment env)
        {
            _opt = opt.Value;
            _env = env;
        }

        public async Task GenerateWavAsync(
            string prompt,
            string? genre,
            int durationSec,
            string outWavPath,
            CancellationToken ct)
        {
            var scriptAbs = Path.GetFullPath(Path.Combine(_env.ContentRootPath, _opt.ScriptPath));
            if (!File.Exists(scriptAbs))
                throw new FileNotFoundException($"lyria_generate.py not found: {scriptAbs}");

            Directory.CreateDirectory(Path.GetDirectoryName(outWavPath)!);

            // 간단히 prompt에 genre를 합쳐도 됨(Colab 느낌)
            var fullPrompt = string.IsNullOrWhiteSpace(genre) ? prompt : $"Genre: {genre}, {prompt}";

            string Q(string s) => $"\"{s.Replace("\"", "\\\"")}\"";

            var args = string.Join(" ",
                Q(scriptAbs),
                "--project", Q(_opt.ProjectId),
                "--location", Q(_opt.Location),
                "--prompt", Q(fullPrompt),
                "--durationSec", durationSec.ToString(),
                "--out", Q(outWavPath)
            );

            var psi = new ProcessStartInfo
            {
                FileName = _opt.PythonExe,      // "python"
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetDirectoryName(scriptAbs)!
            };

            using var p = new Process { StartInfo = psi };

            var stdout = new StringBuilder();
            var stderr = new StringBuilder();

            p.OutputDataReceived += (_, e) => { if (e.Data is not null) stdout.AppendLine(e.Data); };
            p.ErrorDataReceived += (_, e) => { if (e.Data is not null) stderr.AppendLine(e.Data); };

            if (!p.Start())
                throw new InvalidOperationException("Failed to start Lyria python process.");

            p.BeginOutputReadLine();
            p.BeginErrorReadLine();

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(_opt.TimeoutSec));

            try
            {
                await p.WaitForExitAsync(timeoutCts.Token);
            }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested)
            {
                try { if (!p.HasExited) p.Kill(entireProcessTree: true); } catch { }
                throw new TimeoutException($"Lyria generation timed out after {_opt.TimeoutSec}s.");
            }

            if (p.ExitCode != 0)
                throw new InvalidOperationException($"Lyria generator failed (exit={p.ExitCode}). stderr={stderr} stdout={stdout}");

            if (!File.Exists(outWavPath))
                throw new FileNotFoundException($"Lyria did not create wav: {outWavPath}");
        }
    }
}
